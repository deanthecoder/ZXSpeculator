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
using CSharp.Utils.Extensions;
using CSharp.Utils.ViewModels;
using Speculator.Core;

namespace Speculator.ViewModels;

/// <summary>
/// Responsible for allowing the user to choose the boot ROM, and
/// customizing it at runtime.
/// </summary>
public class RomSelectorViewModel : ViewModelBase
{
    public Action<FileInfo> LoadBasicRomAction { get; }
    public IEnumerable<FileInfo> RomFiles { get; }
    public FileInfo RomFile { get; set; }
    public bool UseSpeccyColors { get; set; }
    public bool UseBbcColors { get; set; }
    public bool UseC64Colors { get; set; }

    public RomSelectorViewModel(ZxSpectrum speccy)
    {
        RomFiles = Assembly.GetExecutingAssembly().GetDirectory().EnumerateFiles("ROMs/*.rom").ToArray();
        RomFile = RomFiles.FirstOrDefault(o => o.FullName == Settings.Instance.RomFile?.FullName) ??
                  RomFiles.FirstOrDefault(o => o.LeafName() == "Standard Spectrum 48K BASIC") ??
                  RomFiles.FirstOrDefault();

        UseSpeccyColors = Settings.Instance.UseSpeccyColors;
        UseBbcColors = Settings.Instance.UseBbcColors;
        UseC64Colors = Settings.Instance.UseC64Colors;

        LoadBasicRomAction = romFile =>
        {
            speccy.LoadBasicRom(romFile);
            
            Settings.Instance.RomFile = romFile;
            Settings.Instance.UseSpeccyColors = UseSpeccyColors;
            Settings.Instance.UseBbcColors = UseBbcColors;
            Settings.Instance.UseC64Colors = UseC64Colors;

            // Theme.
            int paper;
            int pen;
            if (UseSpeccyColors)
            {
                paper = 7;
                pen = 0;
            }
            else if (UseBbcColors)
            {
                paper = 0;
                pen = 7;
            }
            else
            {
                paper = 1;
                pen = 5;
            }

            // Set editor colors.
            speccy.TheCpu.MainMemory.Data[0x1265] = 0X3E;
            speccy.TheCpu.MainMemory.Data[0x1266] = (byte)((paper << 3) + pen);

            // Message screen border.
            var isStandardRom = speccy.TheCpu.MainMemory.Data[0x11CC] == 0x3E;
            if (isStandardRom)
            {
                // Speccy ROM.
                speccy.TheCpu.MainMemory.Data[0x11CD] = (byte)paper;
            }
            else
            {
                // JGH only.
                speccy.TheCpu.MainMemory.Data[0x11CD] = 0X3E;
                speccy.TheCpu.MainMemory.Data[0x11CE] = (byte)(0X38 + paper);
                speccy.TheCpu.MainMemory.Data[0x11CF] = 0xD3;
                speccy.TheCpu.MainMemory.Data[0x11D0] = 0XFE;
            }

            speccy.TheCpu.ResetAsync();
        };
    }
}