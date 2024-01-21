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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CSharp.Utils.UI;
using Speculator.ViewModels;

namespace Speculator.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class App : Application
{
    private IDisposable m_keyBlocker;
    
    public App()
    {
        DataContext = new AppViewModel();
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            AppCloseHandler.Instance.Init(desktop);
            
            var viewModel = new MainWindowViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            desktop.MainWindow.Deactivated += (_, _) => m_keyBlocker = viewModel.Speccy.PortHandler.CreateKeyBlocker();
            desktop.MainWindow.Activated += (_, _) => m_keyBlocker?.Dispose();

            if (!Design.IsDesignMode)
            {
                viewModel.Speccy.PortHandler.StartKeyboardHook();
                viewModel.Speccy.PowerOnAsync();

                desktop.MainWindow.Closed += (_, _) => Settings.Instance.Dispose();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
