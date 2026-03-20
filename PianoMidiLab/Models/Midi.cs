namespace PianoMidiLab.Models;

using System.IO;
using static System.Buffers.Binary.BinaryPrimitives;
using static System.IO.Path;
using InDaEx = System.IO.InvalidDataException;

internal sealed class Midi {
    private readonly ushort _division, _format;
    private readonly string _path;
    private readonly List<Track> _tracks = [];

    public Midi(string path) {
        _path = path;
        var data = File.ReadAllBytes(path).AsSpan();
        var fName = $"'{GetFileNameWithoutExtension(path)}'";

        if (data.Length < 14) throw new InDaEx($"{fName}文件过短");
        if (!data[..4].SequenceEqual("MThd"u8) || ReadUInt32BigEndian(data[4..]) != 6)
            throw new InDaEx($"{fName}文件头异常");
        _format = ReadUInt16BigEndian(data[8..]);
        if (_format > 1) throw new NotSupportedException($"仅支持格式0/1，{fName}为格式{_format}");
        var tracks = _tracks.Capacity = ReadUInt16BigEndian(data[10..]);
        if (tracks == 0) throw new InDaEx($"{fName}无音轨");
        _division = ReadUInt16BigEndian(data[12..]);

        for (var (pos, t) = (14, 0); t < tracks; t++) {
            var tName = $"{fName}第{t + 1}音轨";

            if (pos + 8 > data.Length) throw new InDaEx($"{tName}过短");
            if (!data.Slice(pos, 4).SequenceEqual("MTrk"u8)) throw new InDaEx($"{tName}头异常");
            var len = (int)ReadUInt32BigEndian(data[(pos + 4)..]);
            if (pos + 8 + len > data.Length) throw new InDaEx($"{tName}数据不全");

            _tracks.Add(new(data.Slice(pos + 8, len)));
            pos += 8 + len;
        }
    }

    public int RemoveNotes(Predicate<Note> match) => _tracks.Sum(t => t.RemoveNotes(match));

    public void Save() {
        var dir = GetDirectoryName(_path) ?? ".";
        var name = GetFileNameWithoutExtension(_path);
        var ext = GetExtension(_path);
        var outPath = Combine(dir, $"{name}_PML{ext}");
        for (var i = 2; File.Exists(outPath); i++) outPath = Combine(dir, $"{name}_PML_{i}{ext}");

        using var fs = File.Create(outPath);

        var fHeader = (stackalloc byte[14]);
        "MThd"u8.CopyTo(fHeader);
        WriteUInt32BigEndian(fHeader[4..], 6);
        WriteUInt16BigEndian(fHeader[8..], _format);
        WriteUInt16BigEndian(fHeader[10..], (ushort)_tracks.Count);
        WriteUInt16BigEndian(fHeader[12..], _division);
        fs.Write(fHeader);

        var tHeader = (stackalloc byte[8]);
        "MTrk"u8.CopyTo(tHeader);
        foreach (var tData in _tracks.Select(static t => t.ToBytes())) {
            WriteUInt32BigEndian(tHeader[4..], (uint)tData.Length);
            fs.Write(tHeader);
            fs.Write(tData);
        }
    }
}
