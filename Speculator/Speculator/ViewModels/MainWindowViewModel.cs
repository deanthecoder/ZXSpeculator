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
using Avalonia;
using CSharp.Utils.Commands;
using CSharp.Utils.Extensions;
using CSharp.Utils.ViewModels;
using Speculator.Core;

namespace Speculator.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    public ZxSpectrum Speccy { get; }
    public ZxDisplay Display { get; }
    public bool IsFullThrottle { get; private set; }

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

    public void ToggleCursorJoystick() =>
        Speccy.PortHandler.EmulateCursorJoystick = !Speccy.PortHandler.EmulateCursorJoystick;

    public void CloseCommand() =>
        Application.Current.GetMainWindow().Close();

    public void ToggleSound()
    {
        Speccy.SoundHandler.IsEnabled = !Speccy.SoundHandler.IsEnabled;
        
        if (Speccy.SoundHandler.IsEnabled && IsFullThrottle)
            ToggleFullThrottle(); // Enabling sound turns off full throttle.
    }

    public void ToggleFullThrottle()
    {
        IsFullThrottle = !IsFullThrottle;
        
        if (IsFullThrottle)
            Speccy.SoundHandler.IsEnabled = false; // Full throttle turns off sound.
        
        OnPropertyChanged(nameof(IsFullThrottle));
        Speccy.TheCpu.FullThrottle = IsFullThrottle;
    }

    public void OpenProjectPage() => new Uri("https://github.com/deanthecoder/ZXSpeculator").Open();
    
    public void Dispose() => Speccy.Dispose();
}
