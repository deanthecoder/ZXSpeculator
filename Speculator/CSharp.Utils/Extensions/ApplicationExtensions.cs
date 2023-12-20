// -----------------------------------------------------------------------
//  <copyright file="ApplicationExtensions.cs" company="Dean Edis">
//      Copyright (c) 2023 Dean Edis. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Dean Edis. does not warrant or make any representations regarding the use or
//  results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace CSharp.Utils.Extensions;

public static class ApplicationExtensions
{
    public static Window GetMainWindow(this Application app) =>
        (app?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
}