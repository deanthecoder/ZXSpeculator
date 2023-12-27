using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
    
    private void OnHistoryPaneLoaded(object sender, RoutedEventArgs e)
    {
        var listBox = (ListBox)sender;
        var items = (ObservableCollection<string>)listBox.ItemsSource;
        items.CollectionChanged += (_, _) => Dispatcher.UIThread.Invoke(() => listBox.SelectedIndex = items.Count - 1);
        listBox.AutoScrollToSelectedItem = true;
    }
}