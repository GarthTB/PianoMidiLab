namespace PianoMidiLab.Models;

internal readonly record struct Note(
    uint Start,
    uint Dur,
    uint MaxDur, // 下一个同音的距离
    byte Pitch,
    byte OnVel,
    byte OffVel,
    byte Ch) {
    public Event NoteOn => new(Start, (byte)(Event.NoteOn | Ch), Pitch, OnVel, null);

    public Event EndEvent =>
        OffVel == 0
            ? new(Start + Dur, (byte)(Event.NoteOn | Ch), Pitch, 0, null)
            : new(Start + Dur, (byte)(Event.NoteOff | Ch), Pitch, OffVel, null);
}
