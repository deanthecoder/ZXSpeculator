// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If modified, please retain this copyright header, and consider telling us
// about your changes.  We're always glad to see how people use our code!
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// We do not accept any liability for damage caused by executing
// or modifying this code.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Speculator.ViewModels;

namespace Speculator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Closed += (_, _) => (DataContext as IDisposable)?.Dispose();
    }
    
    private void OnScreenBitmapLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext == null)
            return;
        
        // Kick the UI to update the screen when the emulator updates it.
        var action = new Action(() => (sender as Image)?.InvalidateVisual());
        ((MainWindowViewModel)DataContext).Display.Refreshed += (_, _) => Dispatcher.UIThread.InvokeAsync(action);
    }
}