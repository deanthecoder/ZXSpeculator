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
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CSharp.Utils.Extensions;

namespace Speculator.Views;

public partial class RomSelector : UserControl
{
    private IList<FileInfo> m_romFiles;
    private FileInfo m_selectedRom;
    private string m_romDetails;
    private Action<FileInfo> m_loadAction;

    public static readonly DirectProperty<RomSelector, IList<FileInfo>> RomFilesProperty = AvaloniaProperty.RegisterDirect<RomSelector, IList<FileInfo>>(nameof(RomFiles), o => o.RomFiles, (o, v) => o.RomFiles = v);
    public static readonly DirectProperty<RomSelector, FileInfo> SelectedRomProperty = AvaloniaProperty.RegisterDirect<RomSelector, FileInfo>(nameof(SelectedRom), o => o.SelectedRom, (o, v) => o.SelectedRom = v);
    public static readonly DirectProperty<RomSelector, string> RomDetailsProperty = AvaloniaProperty.RegisterDirect<RomSelector, string>(nameof(RomDetails), o => o.RomDetails, (o, v) => o.RomDetails = v);
    public static readonly DirectProperty<RomSelector, Action<FileInfo>> LoadActionProperty = AvaloniaProperty.RegisterDirect<RomSelector, Action<FileInfo>>(nameof(LoadAction), o => o.LoadAction, (o, v) => o.LoadAction = v);

    public RomSelector()
    {
        InitializeComponent();
    }
    
    public IList<FileInfo> RomFiles
    {
        get => m_romFiles;
        set => SetAndRaise(RomFilesProperty, ref m_romFiles, value);
    }
    
    public FileInfo SelectedRom
    {
        get => m_selectedRom;
        set
        {
            if (SetAndRaise(SelectedRomProperty, ref m_selectedRom, value))
                RomDetails = value?.Directory?.GetFile($"{SelectedRom.LeafName()}.txt").ReadAllText() ?? "No information.";
        }
    }

    public string RomDetails
    {
        get => m_romDetails;
        set => SetAndRaise(RomDetailsProperty, ref m_romDetails, value);
    }
    
    public Action<FileInfo> LoadAction
    {
        get => m_loadAction;
        set => SetAndRaise(LoadActionProperty, ref m_loadAction, value);
    }
    
    private void OnLoadAndReset(object sender, RoutedEventArgs e) =>
        LoadAction?.Invoke(m_selectedRom);
}