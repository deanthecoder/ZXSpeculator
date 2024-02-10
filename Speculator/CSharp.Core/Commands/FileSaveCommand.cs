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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CSharp.Core.Extensions;

namespace CSharp.Core.Commands;

public class FileSaveCommand : CommandBase
{
    private readonly string m_title;
    private readonly string m_filterName;
    private readonly string[] m_filterExtensions;
    private readonly string m_defaultName;
    private readonly Func<bool> m_canExecute;

    public event EventHandler<FileInfo> FileSelected;
    public event EventHandler<FileInfo> Cancelled;

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
        else
            Cancelled?.Invoke(this, null);
    }
}
