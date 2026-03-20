namespace PianoMidiLab.Models;

internal readonly record struct Event(uint Tick, byte Status, byte D1, byte D2, byte[]? Dvl)
    : IComparable<Event> {
    public const byte NoteOff = 0x80, NoteOn = 0x90, CC = 0xB0, Sys = 0xF0, EOX = 0xF7, Meta = 0xFF;
    public static (byte Hi, byte Lo) SplitStatus(byte s) => ((byte)(s & 0xF0), (byte)(s & 0x0F));

    public static byte DataLen(byte status) =>
        status switch {
            0xF1 or 0xF3 or >= 0xC0 and < 0xE0 => 1,
            0xF2 or < Sys => 2,
            Sys or EOX or Meta => 3, // 变长
            _ => 0
        };

    #region 比较器

    private byte Weight =>
        (byte)(Status & 0xF0) switch {
            Sys or EOX => 0, NoteOff => 2, NoteOn when D2 == 0 => 2, NoteOn => 3, _ => 1
        };

    private (uint, byte, byte, byte, byte) OrdTup => (Tick, Weight, Status, D1, D2);
    public int CompareTo(Event other) => OrdTup.CompareTo(other.OrdTup);

    #endregion 比较器
}
