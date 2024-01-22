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
using System.Linq;
using System.Reflection;
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
    public Settings Settings => Settings.Instance;
    public MruFiles Mru { get; }

    public IEnumerable<FileInfo> RomFiles { get; }
    public Action<FileInfo> LoadBasicRomAction { get; }

    public MainWindowViewModel()
    {
        RomFiles = Assembly.GetExecutingAssembly().GetDirectory().EnumerateFiles("ROMs/*.rom").ToArray();
        Settings.RomFile = RomFiles.FirstOrDefault(o => o.FullName == Settings.RomFile?.FullName) ??
                           RomFiles.FirstOrDefault(o => o.LeafName() == "Standard Spectrum 48K BASIC") ??
                           RomFiles.FirstOrDefault();
        
        Display = new ZxDisplay();
        Speccy = new ZxSpectrum(Display).LoadBasicRom(Settings.RomFile);
        Speccy.PortHandler.EmulateCursorJoystick = Settings.EmulateCursorJoystick;
        Speccy.TheCpu.LoadRequested += (_, _) =>
        {
            if (!Speccy.TheTapeLoader.IsLoading)
                Dispatcher.UIThread.InvokeAsync(LoadRom);
        };

        LoadBasicRomAction = romFile =>
        {
            Speccy.LoadBasicRom(romFile);
            Settings.RomFile = romFile;
            Speccy.TheCpu.ResetAsync();
        };
        
        Mru = new MruFiles().InitFromString(Settings.MruFiles);
        Mru.OpenRequested += (_, file) => Speccy.LoadRom(file);

        Settings.PropertyChanged += (_, _) => OnSettingsChanged();
        OnSettingsChanged();
        return;
        
        void OnSettingsChanged()
        {
            Display.IsCrt = Settings.IsCrt;
            Speccy.PortHandler.EmulateCursorJoystick = Settings.EmulateCursorJoystick;
            Speccy.SoundHandler.IsEnabled = Settings.IsSoundEnabled;
        }
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
                Mru.Add(info);
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
            confirmed =>
            {
                if (confirmed)
                    Speccy.TheCpu.ResetAsync();
            },
            MaterialIconKind.Power);

    public void SetCursorJoystick(bool b) =>
        Settings.EmulateCursorJoystick = b;

    public void CloseCommand() =>
        Application.Current.GetMainWindow().Close();

    public void SetCrtMode(bool b) =>
        Settings.IsCrt = b;
    
    public void ToggleFullThrottle()
    {
        IsFullThrottle = !IsFullThrottle;
        OnPropertyChanged(nameof(IsFullThrottle));
        Speccy.TheCpu.FullThrottle = IsFullThrottle;
    }

    public void ToggleAmbientBlur() =>
        Settings.IsAmbientBlurred = !Settings.IsAmbientBlurred;

    public void OpenProjectPage() => new Uri("https://github.com/deanthecoder/ZXSpeculator").Open();
    
    public void Dispose()
    {
        Speccy.Dispose();
        Settings.MruFiles = Mru.AsString();
    }
}
