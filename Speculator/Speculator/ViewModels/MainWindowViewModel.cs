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
using CSharp.Utils.Extensions;
using CSharp.Utils.ViewModels;
using Speculator.Commands;
using Speculator.Core;

namespace Speculator.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    public ZxSpectrum Speccy { get; }
    public ZxDisplay Display { get; }
    public bool IsSoundEnabled { get; private set; } = true;
    public bool IsFullThrottle { get; private set; }
    public bool IsDebuggingEnabled { get; private set; }

    public MainWindowViewModel()
    {
        Display = new ZxDisplay();
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
        
        if (IsSoundEnabled && IsFullThrottle)
            ToggleFullThrottle(); // Enabling sound turns off full throttle.
        
        OnPropertyChanged(nameof(IsSoundEnabled));
        Speccy.SoundHandler.SetSoundEnabled(IsSoundEnabled);
    }

    public void ToggleFullThrottle()
    {
        IsFullThrottle = !IsFullThrottle;
        
        if (IsFullThrottle && IsSoundEnabled)
            ToggleSound(); // Full throttle turns off sound.
        
        OnPropertyChanged(nameof(IsFullThrottle));
        Speccy.TheCpu.FullThrottle = IsFullThrottle;
    }
    
    public void ToggleDebugging()
    {
        IsDebuggingEnabled = !IsDebuggingEnabled;
        
        if (IsDebuggingEnabled && IsSoundEnabled)
            ToggleSound(); // Debugging turns off sound.
        
        OnPropertyChanged(nameof(IsDebuggingEnabled));
        Speccy.TheCpu.IsDebugging = IsDebuggingEnabled;
    }

    public void Dispose() => Speccy.Dispose();
}