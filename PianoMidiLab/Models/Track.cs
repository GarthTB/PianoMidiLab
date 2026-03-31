namespace PianoMidiLab.Models;

using System.IO;
using static Event;
using static Pedal;

internal sealed class Track {
    private readonly List<Event> _misc = new(128); // 只读透传事件
    private readonly List<Note> _notes = new(2048);
    private readonly List<Pedal> _pedals = new(2048);

    public Track(ReadOnlySpan<byte> data) {
        var tick = 0u;
        byte status = 0;
        var noteOns = (stackalloc (uint Tick, byte Vel)[2048]);
        noteOns.Clear(); // 初始化为默认值
        var pedalVals = (stackalloc byte[2048]);
        pedalVals.Clear();
        for (var pos = 0; pos < data.Length;) {
            tick += ReadVLQ(data, ref pos);

            if (data[pos] >= 0x80) status = data[pos++];
            if (status == Meta && data[pos] == 0x2F) return; // 跳过EOT

            var (hi, lo) = SplitStatus(status);
            if (hi is NoteOff or NoteOn) {
                byte pitch = data[pos++], vel = data[pos++];
                ref var noteOn = ref noteOns[pitch * 16 + lo];
                if (noteOn.Vel > 0 && tick > noteOn.Tick) // 丢弃0长音符
                    _notes.Add(new(noteOn.Tick, tick - noteOn.Tick, pitch, noteOn.Vel, vel, lo));
                noteOn = hi == NoteOn && vel > 0
                    ? (tick, vel)
                    : default;
            } else if (hi == CC && data[pos] is Sustain or Sostenuto or Soft or Hold2) {
                byte ccNum = data[pos++], val = data[pos++];
                ref var pedalVal = ref pedalVals[ccNum * 16 + lo];
                if (pedalVal != val) _pedals.Add(new(tick, ccNum, pedalVal = val, lo)); // 丢弃同值CC
            } else { // 包括非踏板CC
                byte d1 = 0, d2 = 0;
                int len = DataLen(status);
                if (len is 1 or 2 || status == Meta) d1 = data[pos++];
                if (len == 2) d2 = data[pos++];
                if (len == 3) {
                    len = (int)ReadVLQ(data, ref pos);
                    _misc.Add(new(tick, status, d1, d2, [..data.Slice(pos, len)]));
                    pos += len;
                } else
                    _misc.Add(new(tick, status, d1, d2, null));
                if (status is >= Sys and <= EOX or Meta) status = 0;
            }
        }
    }

    public void RemNotes(Predicate<Note> match) => _notes.RemoveAll(match);

    public byte[] ToBytes() {
        List<Event> events = new(_notes.Count * 2 + _pedals.Count + _misc.Count);
        foreach (var note in _notes) {
            events.Add(note.NoteOn);
            events.Add(note.EndEvent);
        }
        events.AddRange(_pedals.Select(static pedal => pedal.CC));
        events.AddRange(_misc);
        events.Sort();

        using MemoryStream ms = new(events.Count * 6);
        var tick = 0u;
        byte status = 0;
        foreach (var e in events) {
            WriteVLQ(ms, e.Tick - tick);
            tick = e.Tick;

            if (e.Status != status || e.Status >= 0xF0) ms.WriteByte(status = e.Status);
            if (e.Status is >= Sys and <= EOX or Meta) status = 0;

            var len = DataLen(e.Status);
            if (len is 1 or 2 || e.Status == Meta) ms.WriteByte(e.D1);
            if (len == 2) ms.WriteByte(e.D2);
            if (len != 3) continue;
            WriteVLQ(ms, (uint)e.Dvl!.Length);
            ms.Write(e.Dvl);
        }

        WriteVLQ(ms, 0); // 还原EOT
        ms.Write([0xFF, 0x2F, 0]);
        return ms.ToArray();
    }

    private static uint ReadVLQ(ReadOnlySpan<byte> data, ref int pos) {
        var v = 0u;
        for (byte b = 0x80; b >= 0x80; v = (v << 7) | (b & 0x7Fu)) b = data[pos++];
        return v;
    }

    private static void WriteVLQ(Stream stream, uint v) {
        var b = (stackalloc byte[5]);
        var i = 4;
        for (b[4] = (byte)(v & 0x7F); (v >>= 7) > 0; b[--i] = (byte)(v | 0x80)) ;
        stream.Write(b[i..]);
    }
}
