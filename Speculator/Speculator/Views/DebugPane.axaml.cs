using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CSharp.Utils;
using Speculator.Core.Debugger;

namespace Speculator.Views;

public partial class DebugPane : UserControl
{
    private ActionConsolidator m_scrollAction;
    
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
        if (m_scrollAction != null)
            return;
        
        var listBox = (ListBox)sender;
        listBox.AutoScrollToSelectedItem = true;

        m_scrollAction = new ActionConsolidator(() => ScrollToEnd(listBox));
        var items = (ObservableCollection<string>)listBox.ItemsSource;
        items.CollectionChanged += (_, _) => m_scrollAction.Invoke();
    }
    
    private static void ScrollToEnd(SelectingItemsControl listBox)
    {
        try
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var items = (ObservableCollection<string>)listBox.ItemsSource;
                return listBox.SelectedIndex = items.Count - 1;
            });
        }
        catch (TaskCanceledException)
        {
            // Shutting down - This is ok.
        }
    }
}