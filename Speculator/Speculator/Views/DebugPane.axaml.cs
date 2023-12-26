using Avalonia.Controls;
using Avalonia.Interactivity;
using Speculator.ViewModels;

namespace Speculator.Views;

public partial class DebugPane : UserControl
{
    public DebugPane()
    {
        InitializeComponent();
    }
    private void OnStepPressed(object sender, RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext)?.MemoryDump.Refresh();
    }
}