namespace PianoMidiLab.VMs.FXTabVMs;

using CommunityToolkit.Mvvm.ComponentModel;
using Models;
using static Math;

internal sealed partial class CleaningTabVM: ObservableObject {
    [ObservableProperty] public partial bool RemLong { get; set; }
    [ObservableProperty] public partial bool RemShort { get; set; }
    [ObservableProperty] public partial bool RemHigh { get; set; }
    [ObservableProperty] public partial bool RemLow { get; set; }
    [ObservableProperty] public partial bool RemForte { get; set; }
    [ObservableProperty] public partial bool RemPiano { get; set; }

    public uint LongLim {
        get;
        set {
            var v = Clamp(value, 2, uint.MaxValue - 1);
            if (SetProperty(ref field, v) && ShortLim > field) ShortLim = field;
        }
    } = uint.MaxValue - 1;

    public uint ShortLim {
        get;
        set {
            var v = Clamp(value, 2, uint.MaxValue - 1);
            if (SetProperty(ref field, v) && LongLim < field) LongLim = field;
        }
    } = 2;

    public int HighLim {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, 1, 126)) && LowLim > field) LowLim = field;
        }
    } = 108;

    public int LowLim {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, 1, 126)) && HighLim < field) HighLim = field;
        }
    } = 21;

    public int ForteLim {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, 2, 126)) && PianoLim > field) PianoLim = field;
        }
    } = 126;

    public int PianoLim {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, 2, 126)) && ForteLim < field) ForteLim = field;
        }
    } = 2;

    public void Apply(Midi midi) {
        if (RemLong || RemShort || RemHigh || RemLow || RemForte || RemPiano)
            midi.RemNotes(n => (RemLong && n.Dur > LongLim)
                            || (RemShort && n.Dur < ShortLim)
                            || (RemHigh && n.Pitch > HighLim)
                            || (RemLow && n.Pitch < LowLim)
                            || (RemForte && n.OnVel > ForteLim)
                            || (RemPiano && n.OnVel < PianoLim));
    }
}
