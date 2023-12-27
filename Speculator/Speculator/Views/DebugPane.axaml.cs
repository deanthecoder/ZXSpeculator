using Avalonia.Controls;
using Avalonia.Interactivity;
using Speculator.Core.Debugger;

namespace Speculator.Views;

public partial class DebugPane : UserControl
{
    public DebugPane()
    {
        InitializeComponent();
    }
    private void OnStepPressed(object sender, RoutedEventArgs e)
    {
        ((Debugger)DataContext)?.MemoryDump.Refresh();
    }
}