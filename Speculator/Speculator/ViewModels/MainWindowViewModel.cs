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
using Avalonia;
using Speculator.Commands;
using Speculator.Core;
using Speculator.Extensions;

namespace Speculator.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    public ZxSpectrum Speccy { get; }
    public ZxDisplay Display { get; } = new ZxDisplay();
    public bool IsSoundEnabled { get; private set; } = true;

    public MainWindowViewModel()
    {
        Speccy = new ZxSpectrum(Display).LoadBasicRom();
    }

    public void LoadRom()
    {
        var command = new FileOpenCommand("Load ROM file", "ROM Files", ZxFileIo.FileFilters);
        command.FileSelected += (_, info) =>
        {
            Speccy.LoadRom(info);
        };
        command.Execute(null);
    }
    
    public void ResetMachine() =>
        Speccy.TheCpu.ResetAsync();

    public void CloseCommand() =>
        Application.Current.GetMainWindow().Close();

    public void ToggleSound()
    {
        IsSoundEnabled = !IsSoundEnabled;
        OnPropertyChanged(nameof(IsSoundEnabled));
        Speccy.SetSoundEnabled(IsSoundEnabled);
    }

    public void Dispose() => Speccy.Dispose();
}