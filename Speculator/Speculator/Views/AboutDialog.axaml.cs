// -----------------------------------------------------------------------
//  <copyright file="AboutDialog.axaml.cs" company="Dean Edis">
//      Copyright (c) 2023 Dean Edis. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Dean Edis. does not warrant or make any representations regarding the use or
//  results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using System.Reflection;
using Avalonia.Controls;

namespace Speculator.Views;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();

        AppVersion.Text = $"Version {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
    }
}