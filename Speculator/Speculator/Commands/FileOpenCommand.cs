// -----------------------------------------------------------------------
//  <copyright file="FileOpenCommand.cs" company="Dean Edis">
//      Copyright (c) 2023 Dean Edis. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Dean Edis. does not warrant or make any representations regarding the use or
//  results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Speculator.Extensions;

namespace Speculator.Commands;

public class FileOpenCommand : CommandBase
{
    private readonly string m_title;
    private readonly string m_filterName;
    private readonly string[] m_filterExtensions;
    
    public event EventHandler<FileInfo> FileSelected;

    public FileOpenCommand(string title, string filterName, string[] filterExtensions)
    {
        m_title = title;
        m_filterName = filterName;
        m_filterExtensions = filterExtensions;
    }

    public override async void Execute(object parameter)
    {
        var mainWindow = Application.Current?.GetMainWindow();
        if (mainWindow == null)
            return; // Cannot find the main application window.

        var files =
            await TopLevel
                .GetTopLevel(mainWindow)
                .StorageProvider
                .OpenFilePickerAsync(
                                     new FilePickerOpenOptions
                                     {
                                         Title = m_title,
                                         AllowMultiple = false,
                                         FileTypeFilter = new[]
                                         {
                                             new FilePickerFileType(m_filterName)
                                             {
                                                 Patterns = m_filterExtensions
                                             }
                                         }
                                     });
        var selectedFile = files.FirstOrDefault();
        if (selectedFile != null)
            FileSelected?.Invoke(this, new FileInfo(selectedFile.Path.LocalPath));
    }
}