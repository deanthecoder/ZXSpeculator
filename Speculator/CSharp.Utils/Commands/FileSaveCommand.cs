// -----------------------------------------------------------------------
//  <copyright file="FileSaveCommand.cs" company="Dean Edis">
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
using Avalonia.Platform.Storage;
using CSharp.Utils.Extensions;

namespace CSharp.Utils.Commands;

public class FileSaveCommand : CommandBase
{
    private readonly string m_title;
    private readonly string m_filterName;
    private readonly string[] m_filterExtensions;
    private readonly string m_defaultName;
    private readonly Func<bool> m_canExecute;

    public event EventHandler<FileInfo> FileSelected;

    public FileSaveCommand(string title, string filterName, string[] filterExtensions, string defaultName = null, Func<bool> canExecute = null)
    {
        m_title = title ?? throw new ArgumentNullException(nameof(title));
        m_filterName = filterName ?? throw new ArgumentNullException(nameof(filterName));
        m_filterExtensions = filterExtensions ?? throw new ArgumentNullException(nameof(filterExtensions));
        m_defaultName = defaultName;
        m_canExecute = canExecute ?? (() => true);
    }

    public override bool CanExecute(object parameter) =>
        base.CanExecute(parameter) && m_canExecute();

    public override async void Execute(object parameter)
    {
        var mainWindow = Application.Current?.GetMainWindow();
        if (mainWindow == null)
            return; // Cannot find the main application window.

        var selectedFile =
            await TopLevel
                .GetTopLevel(mainWindow)
                .StorageProvider
                .SaveFilePickerAsync(
                                     new FilePickerSaveOptions
                                     {
                                         Title = m_title,
                                         ShowOverwritePrompt = true,
                                         SuggestedFileName = m_defaultName,
                                         DefaultExtension = m_filterExtensions.FirstOrDefault()?.TrimStart('*'),
                                         FileTypeChoices = new[]
                                         {
                                             new FilePickerFileType(m_filterName)
                                             {
                                                 Patterns = m_filterExtensions
                                             }
                                         }
                                     });
        if (selectedFile != null)
            FileSelected?.Invoke(this, new FileInfo(selectedFile.Path.AbsolutePath));
    }
}