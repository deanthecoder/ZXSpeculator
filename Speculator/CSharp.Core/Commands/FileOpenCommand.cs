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

public class FileOpenCommand : CommandBase
{
    private readonly string m_title;
    private readonly string m_filterName;
    private readonly string[] m_filterExtensions;
    
    public event EventHandler<FileInfo> FileSelected;
    public event EventHandler<FileInfo> Cancelled;

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
        var selectedFile = files.FirstOrDefault()?.ToFileInfo();
        if (selectedFile != null)
            FileSelected?.Invoke(this, selectedFile);
        else
            Cancelled?.Invoke(this, null);
    }
}
