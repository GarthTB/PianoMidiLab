namespace PianoMidiLab.Models;

internal readonly record struct Pedal(uint Tick, byte CcNum, byte Val, byte Ch) {
    public const byte Sustain = 64, Sostenuto = 66, Soft = 67, Hold2 = 69;
    public Event CC => new(Tick, (byte)(Event.CC | Ch), CcNum, Val, null);
}
