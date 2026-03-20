namespace PianoMidiLab.Models;

internal readonly record struct Note(
    uint Start,
    uint Dur,
    byte Pitch,
    byte OnVel,
    byte OffVel,
    byte Ch) {
    public uint End => Start + Dur;
    public Event NoteOn => new(Start, (byte)(Event.NoteOn | Ch), Pitch, OnVel, null);

    public Event EndEvent =>
        OffVel == 0
            ? new(End, (byte)(Event.NoteOn | Ch), Pitch, 0, null)
            : new(End, (byte)(Event.NoteOff | Ch), Pitch, OffVel, null);
}
