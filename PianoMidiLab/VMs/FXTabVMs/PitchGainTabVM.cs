namespace PianoMidiLab.VMs.FXTabVMs;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using static Math;

internal sealed partial class PitchGainTabVM: ObservableObject {
    private static readonly bool[] HasAnchor = new bool[126];
    [ObservableProperty] public partial bool Enabled { get; set; }

    public double Pitch0Gain {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, .05, 20)))
                Pitch0VelOut = (int)Round(Pow((Pitch0VelIn - 1) / 126d, 1 / Pitch0Gain) * 126) + 1;
        }
    } = 1;

    public int Pitch0VelIn {
        get;
        set {
            if (SetProperty(ref field, Clamp(value, 1, 127)))
                Pitch0VelOut = (int)Round(Pow((Pitch0VelIn - 1) / 126d, 1 / Pitch0Gain) * 126) + 1;
        }
    } = 64;

    [ObservableProperty] public partial int Pitch0VelOut { get; private set; } = 64;
    public double Pitch127Gain { get; set => SetProperty(ref field, Clamp(value, .05, 20)); } = 1;
    public ObservableCollection<Anchor> Anchors { get; } = [];
    private bool CanAddAnchor => Anchors.Count < 126;
    private bool CanRemAnchor => SelAnchor is {};

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemAnchorCommand))]
    public partial Anchor? SelAnchor { get; set; }

    [RelayCommand(CanExecute = nameof(CanAddAnchor))]
    private void AddAnchor() {
        var i = HasAnchor.IndexOf(false);
        Anchors.Add(new(i + 1));
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
         || (Abs(Pitch0Gain - 1) < .01
          && Abs(Pitch127Gain - 1) < .01
          && Anchors.All(static a => Abs(a.Gain - 1) < .01)))
            return;

        var anchors = Anchors.Select(static a => (a.Pitch, a.Gain))
            .Append(new(0, Pitch0Gain))
            .Append(new(127, Pitch127Gain))
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

    public sealed class Anchor(int pitch): ObservableObject {
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

        public double Gain { get; set => SetProperty(ref field, Clamp(value, .05, 20)); } = 1;
    }
}
