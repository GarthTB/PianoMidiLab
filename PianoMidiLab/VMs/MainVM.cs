namespace PianoMidiLab.VMs;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Models;
using static System.Windows.MessageBox;
using static System.Windows.MessageBoxButton;
using static System.Windows.MessageBoxImage;

internal sealed partial class MainVM: ObservableObject {
    #region 文件

    public ObservableCollection<string> Paths { get; } = [];

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemovePathCommand))]
    public partial string? SelPath { get; set; }

    private bool HasPath => Paths.Count > 0;
    private bool PathSelected => SelPath is {};

    public void AddPaths(IEnumerable<string> paths) {
        foreach (var p in paths.Where(p => !Paths.Contains(p))) Paths.Add(p);
        RunCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void AddPaths() {
        const string filter = "MIDI文件|*.mid;*.midi;*.kar|所有文件|*.*";
        var ofd = new OpenFileDialog { Title = "添加MIDI文件", Multiselect = true, Filter = filter };
        if (ofd.ShowDialog() == true) AddPaths(ofd.FileNames);
    }

    [RelayCommand(CanExecute = nameof(PathSelected))]
    private void RemovePath() {
        Paths.Remove(SelPath!);
        RunCommand.NotifyCanExecuteChanged();
    }

    #endregion 文件

    #region 批处理

    [ObservableProperty] public partial bool Idle { get; set; } = true;

    [RelayCommand(CanExecute = nameof(HasPath), IncludeCancelCommand = true)]
    private async Task RunAsync(CancellationToken ct) {
        Idle = false;
        try {
            while (Paths is [var path, ..]) {
                await Task.Run(() => new Midi(path).Save(), ct);
                Paths.RemoveAt(0);
            }
            Show("全部处理完成", "成功", OK, Information);
        } catch (OperationCanceledException) {} catch (Exception ex) {
            Show($"批处理时：\n{ex}", "异常", OK, Error);
        } finally { Idle = true; }
    }

    #endregion 批处理
}
