// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CSharp.Core;
using Speculator.Core.Debugger;

namespace Speculator.Views;

public partial class DebugPane : UserControl
{
    private ActionConsolidator m_scrollAction;
    
    public DebugPane()
    {
        InitializeComponent();
    }
    
    private void OnStepPressed(object sender, RoutedEventArgs e) =>
        ((Debugger)DataContext)?.MemoryDump.Refresh();

    private void OnHistoryPaneLoaded(object sender, RoutedEventArgs e)
    {
        if (m_scrollAction != null)
            return;
        
        var listBox = (ListBox)sender;
        listBox.AutoScrollToSelectedItem = true;

        m_scrollAction = new ActionConsolidator(() => ScrollToEnd(listBox));
        var items = (ObservableCollection<string>)listBox.ItemsSource;
        if (items != null)
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
