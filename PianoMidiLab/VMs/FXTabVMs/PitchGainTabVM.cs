namespace PianoMidiLab.VMs.FXTabVMs;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using static Math;

internal sealed partial class PitchGainTabVM: ObservableObject {
    private static readonly bool[] HasAnchor = new bool[126];
    [ObservableProperty] public partial bool Enabled { get; set; }

    public double Gain0 {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, .05, 20)))
                VelOut0 = (int)Round(Pow((PreviewVelIn - 1) / 126d, 1 / field) * 126) + 1;
        }
    } = 1;

    public double Gain127 {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, .05, 20)))
                VelOut127 = (int)Round(Pow((PreviewVelIn - 1) / 126d, 1 / field) * 126) + 1;
        }
    } = 1;

    public ObservableCollection<Anchor> Anchors { get; } = [];
    private bool CanAddAnchor => Anchors.Count < 126;
    private bool CanRemAnchor => SelAnchor is {};

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemAnchorCommand))]
    public partial Anchor? SelAnchor { get; set; }

    [RelayCommand(CanExecute = nameof(CanAddAnchor))]
    private void AddAnchor() {
        var i = HasAnchor.IndexOf(false);
        Anchors.Add(new(i + 1, PreviewVelIn));
        HasAnchor[i] = true;
        AddAnchorCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemAnchor))]
    private void RemAnchor() {
        HasAnchor[SelAnchor!.Pitch - 1] = false;
        Anchors.Remove(SelAnchor);
        AddAnchorCommand.NotifyCanExecuteChanged();
    }

    public void Apply(Midi midi) {
        if (!Enabled
         || (Abs(Gain0 - 1) < .01
          && Abs(Gain127 - 1) < .01
          && Anchors.All(static a => Abs(a.Gain - 1) < .01)))
            return;

        var anchors = Anchors.Select(static a => (a.Pitch, a.Gain))
            .Append(new(0, Gain0))
            .Append(new(127, Gain127))
            .OrderBy(static a => a.Pitch)
            .ToArray();
        var map = new byte[128, 127];
        for (int i = 0, p = 0; p < 128; p++) {
            if (anchors[i + 1].Pitch < p) i++;
            var (p1, g1) = anchors[i];
            var (p2, g2) = anchors[i + 1];
            var invG = 1d / (g1 + (g2 - g1) * (p - p1) / (p2 - p1));
            for (var v = 0; v < 127; v++) map[p, v] = (byte)(Round(Pow(v / 126d, invG) * 126) + 1);
        }
        midi.MapNoteOnVel((v, p) => map[p, v - 1]);
    }

    public sealed partial class Anchor(int pitch, int velIn): ObservableObject {
        public int VelIn {
            private get;
            set {
                if (field == value) return;
                field = value;
                VelOut = (int)Round(Pow((field - 1) / 126d, 1 / Gain) * 126) + 1;
            }
        } = velIn;

        public int Pitch {
            get;
            set {
                var oldVal = field;
                var newVal = Clamp(value, 1, 126);
                if (HasAnchor[newVal - 1])
                    OnPropertyChanged();
                else if (SetProperty(ref field, newVal)) {
                    HasAnchor[oldVal - 1] = false;
                    HasAnchor[newVal - 1] = true;
                }
            }
        } = pitch;

        public double Gain {
            get;
            set {
                if (SetProperty(ref field, Clamp(value, .05, 20)))
                    VelOut = (int)Round(Pow((VelIn - 1) / 126d, 1 / field) * 126) + 1;
            }
        } = 1;

        [ObservableProperty] public partial int VelOut { get; private set; } = velIn;
    }

    #region 预览力度

    public int PreviewVelIn {
        get;
        set {
            if (!SetProperty(ref field, Clamp(value, 1, 127))) return;
            VelOut0 = (int)Round(Pow((field - 1) / 126d, 1 / Gain0) * 126) + 1;
            VelOut127 = (int)Round(Pow((field - 1) / 126d, 1 / Gain127) * 126) + 1;
            foreach (var a in Anchors) a.VelIn = field;
        }
    } = 64;

    [ObservableProperty] public partial int VelOut0 { get; private set; } = 64;
    [ObservableProperty] public partial int VelOut127 { get; private set; } = 64;

    #endregion 预览力度
}
