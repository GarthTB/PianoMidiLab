namespace PianoMidiLab.VMs.FXTabVMs;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using static Math;

internal sealed partial class MapVelTabVM: ObservableObject {
    private static readonly bool[] HasAnchor = new bool[125];
    [ObservableProperty] public partial bool Enabled { get; set; }
    public int Vel1To { get; set => SetProperty(ref field, Clamp(value, 1, 127)); } = 1;
    public int Vel127To { get; set => SetProperty(ref field, Clamp(value, 1, 127)); } = 127;
    public ObservableCollection<Anchor> Anchors { get; } = [];
    private bool CanAddAnchor => Anchors.Count < 125;
    private bool CanRemAnchor => SelAnchor is {};

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemAnchorCommand))]
    public partial Anchor? SelAnchor { get; set; }

    [RelayCommand(CanExecute = nameof(CanAddAnchor))]
    private void AddAnchor() {
        var i = HasAnchor.IndexOf(false);
        Anchors.Add(new(i + 2, i + 2));
        HasAnchor[i] = true;
        AddAnchorCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemAnchor))]
    private void RemAnchor() {
        HasAnchor[SelAnchor!.VelIn - 2] = false;
        Anchors.Remove(SelAnchor);
        AddAnchorCommand.NotifyCanExecuteChanged();
    }

    public void Apply(Midi midi) {
        if (!Enabled
         || (Vel1To == 1 && Vel127To == 127 && Anchors.All(static a => a.VelIn == a.VelOut)))
            return;

        var anchors = Anchors.Select(static a => (a.VelIn, a.VelOut))
            .Append(new(1, Vel1To))
            .Append(new(127, Vel127To))
            .OrderBy(static a => a.VelIn)
            .ToArray();
        var map = new byte[127];
        for (int i = 0, v = 1; v < 128; v++) {
            if (anchors[i + 1].VelIn < v) i++;
            var (i1, o1) = anchors[i];
            var (i2, o2) = anchors[i + 1];
            map[v - 1] = (byte)Round(o1 + (o2 - o1) * (double)(v - i1) / (i2 - i1));
        }
        midi.MapNoteOnVel((v, _) => map[v - 1]);
    }

    public sealed class Anchor(int velIn, int velOut): ObservableObject {
        public int VelIn {
            get;
            set {
                var oldVal = field;
                var newVal = Clamp(value, 2, 126);
                if (HasAnchor[newVal - 2])
                    OnPropertyChanged();
                else if (SetProperty(ref field, newVal)) {
                    HasAnchor[oldVal - 2] = false;
                    HasAnchor[newVal - 2] = true;
                }
            }
        } = velIn;

        public int VelOut { get; set => SetProperty(ref field, Clamp(value, 1, 127)); } = velOut;
    }
}
