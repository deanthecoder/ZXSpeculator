// -----------------------------------------------------------------------
//  <copyright file="AppViewModel.cs" company="Dean Edis">
//      Copyright (c) 2023 Dean Edis. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Dean Edis. does not warrant or make any representations regarding the use or
//  results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using System.Windows.Input;
using Avalonia;
using CSharp.Utils.Commands;
using CSharp.Utils.Extensions;
using ReactiveUI;
using Speculator.Views;

namespace Speculator.ViewModels;

public class AppViewModel : ReactiveObject
{
    public ICommand AboutCommand { get; }

    public AppViewModel()
    {
        var isOpen = false;
        AboutCommand = new RelayCommand(
                                        _ =>
                                        {
                                            if (isOpen)
                                                return;
                                            var dialog = new AboutDialog();
                                            dialog.Opened += (_, _) => isOpen = true;
                                            dialog.Closed += (_, _) => isOpen = false;
                                            dialog.ShowDialog(Application.Current?.GetMainWindow());
                                        });
    }
}