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
            var newVal = Clamp(value, 2, 268435454);
            if (SetProperty(ref field, newVal) && ShortLim > newVal) ShortLim = newVal;
        }
    } = 268435454;

    public uint ShortLim {
        get;
        set {
            var newVal = Clamp(value, 2, 268435454);
            if (SetProperty(ref field, newVal) && LongLim < newVal) LongLim = newVal;
        }
    } = 2;

    public uint HighLim {
        get;
        set {
            var newVal = Clamp(value, 1, 126);
            if (SetProperty(ref field, newVal) && LowLim > newVal) LowLim = newVal;
        }
    } = 108;

    public uint LowLim {
        get;
        set {
            var newVal = Clamp(value, 1, 126);
            if (SetProperty(ref field, newVal) && HighLim < newVal) HighLim = newVal;
        }
    } = 21;

    public uint ForteLim {
        get;
        set {
            var newVal = Clamp(value, 2, 126);
            if (SetProperty(ref field, newVal) && PianoLim > newVal) PianoLim = newVal;
        }
    } = 126;

    public uint PianoLim {
        get;
        set {
            var newVal = Clamp(value, 2, 126);
            if (SetProperty(ref field, newVal) && ForteLim < newVal) ForteLim = newVal;
        }
    } = 2;

    public void Apply(Midi midi) {
        if (RemLong || RemShort || RemHigh || RemLow || RemForte || RemPiano)
            midi.RemoveNotes(n =>
                (RemLong && n.Dur > LongLim)
             || (RemShort && n.Dur < ShortLim)
             || (RemHigh && n.Pitch > HighLim)
             || (RemLow && n.Pitch < LowLim)
             || (RemForte && n.OnVel > ForteLim)
             || (RemPiano && n.OnVel < PianoLim));
    }
}
