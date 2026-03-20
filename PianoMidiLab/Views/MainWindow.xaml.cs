namespace PianoMidiLab.Views;

using System.Windows;
using VMs;
using static System.IO.File;

public sealed partial class MainWindow {
    public MainWindow() => InitializeComponent();

    private void DropMidiPaths(object s, DragEventArgs e) {
        if (DataContext is MainVM vm && e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            vm.AddPaths(paths.Where(Exists));
    }
}
