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
using Avalonia.Threading;
using CSharp.Utils.Commands;
using CSharp.Utils.Extensions;
using CSharp.Utils.UI;
using CSharp.Utils.ViewModels;
using Material.Icons;
using Speculator.Core;

namespace Speculator.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    public ZxSpectrum Speccy { get; }
    public ZxDisplay Display { get; }
    public bool IsFullThrottle { get; private set; }
    
    public bool IsCrt
    {
        get => Display.IsCrt;
        private set
        {
            Display.IsCrt = value;
            OnPropertyChanged();
        }
    }

    public MainWindowViewModel()
    {
        Display = new ZxDisplay();
        Speccy = new ZxSpectrum(Display).LoadBasicRom();

        Speccy.TheCpu.LoadRequested += (_, _) =>
        {
            if (!Speccy.TheTapeLoader.IsLoading)
                Dispatcher.UIThread.InvokeAsync(LoadRom);
        };
    }

    public void LoadRom()
    {
        var keyBlocker = Speccy.PortHandler.CreateKeyBlocker();
        var command = new FileOpenCommand("Load ROM file", "ROM Files", ZxFileIo.OpenFilters);
        command.FileSelected += (_, info) =>
        {
            try
            {
                Speccy.LoadRom(info);
            }
            finally
            {
                keyBlocker.Dispose();
            }
        };
        command.Cancelled += (_, _) => keyBlocker.Dispose();
        command.Execute(null);
    }
    
    public void SaveRom()
    {
        var keyBlocker = Speccy.PortHandler.CreateKeyBlocker();
        var command = new FileSaveCommand("Save ROM file", "ROM Files", ZxFileIo.SaveFilters);
        command.FileSelected += (_, info) =>
        {
            try
            {
                Speccy.SaveRom(info);
            }
            finally
            {
                keyBlocker.Dispose();
            }
        };
        command.Cancelled += (_, _) => keyBlocker.Dispose();
        command.Execute(null);
    }
    
    public void ResetMachine() =>
        DialogService.Instance.Warn(
            "Reset Emulator?",
            "This will simulate a restart of the ZX Spectrum.",
            "CANCEL",
            "RESET",
            () => Speccy.TheCpu.ResetAsync(),
            MaterialIconKind.Power);

    public void ToggleCursorJoystick() =>
        Speccy.PortHandler.EmulateCursorJoystick = !Speccy.PortHandler.EmulateCursorJoystick;

    public void CloseCommand() =>
        Application.Current.GetMainWindow().Close();

    public void ToggleCrt() =>
        IsCrt = !IsCrt;

    public void ToggleSound() =>
        Speccy.SoundHandler.IsEnabled = !Speccy.SoundHandler.IsEnabled;

    public void ToggleFullThrottle()
    {
        IsFullThrottle = !IsFullThrottle;
        OnPropertyChanged(nameof(IsFullThrottle));
        Speccy.TheCpu.FullThrottle = IsFullThrottle;
    }

    public void OpenProjectPage() => new Uri("https://github.com/deanthecoder/ZXSpeculator").Open();
    
    public void Dispose() => Speccy.Dispose();
}
