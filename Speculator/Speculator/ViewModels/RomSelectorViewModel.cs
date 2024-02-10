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
using CSharp.Core.Extensions;
using CSharp.Core.ViewModels;
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
            speccy.LoadBasicRom(romFile ?? RomFile);

            Settings.Instance.RomFile = romFile;
            Settings.Instance.UseSpeccyColors = UseSpeccyColors;
            Settings.Instance.UseBbcColors = UseBbcColors;
            Settings.Instance.UseC64Colors = UseC64Colors;

            // Theme.
            const string speccyFont = "AAAAAAAAAAAAEBAQEAAQAAAkJAAAAAAAACR+JCR+JAAACD4oPgo+CABiZAgQJkYAABAoECpEOgAACBAAAAAAAAAECAgICAQAACAQEBAQIAAAABQIPggUAAAACAg+CAgAAAAAAAAICBAAAAAAPgAAAAAAAAAAGBgAAAACBAgQIAAAPEZKUmI8AAAYKAgICD4AADxCAjxAfgAAPEIMAkI8AAAIGChIfggAAH5AfAJCPAAAPEB8QkI8AAB+AgQIEBAAADxCPEJCPAAAPEJCPgI8AAAAABAAABAAAAAQAAAQECAAAAQIEAgEAAAAAD4APgAAAAAQCAQIEAAAPEIECAAIAAA8SlZeQDwAADxCQn5CQgAAfEJ8QkJ8AAA8QkBAQjwAAHhEQkJEeAAAfkB8QEB+AAB+QHxAQEAAADxCQE5CPAAAQkJ+QkJCAAA+CAgICD4AAAICAkJCPAAAREhwSERCAABAQEBAQH4AAEJmWkJCQgAAQmJSSkZCAAA8QkJCQjwAAHxCQnxAQAAAPEJCUko8AAB8QkJ8REIAADxAPAJCPAAA/hAQEBAQAABCQkJCQjwAAEJCQkIkGAAAQkJCQlokAABCJBgYJEIAAIJEKBAQEAAAfgQIECB+AAAOCAgICA4AAABAIBAIBAAAcBAQEBBwAAAQOFQQEBAAAAAAAAAAAP8AHCJ4ICB+AAAAOAQ8RDwAACAgPCIiPAAAABwgICAcAAAEBDxERDwAAAA4RHhAPAAADBAYEBAQAAAAPEREPAQ4AEBAeERERAAAEAAwEBA4AAAEAAQEBCQYACAoMDAoJAAAEBAQEBAMAAAAaFRUVFQAAAB4RERERAAAADhEREQ4AAAAeEREeEBAAAA8REQ8BAYAABwgICAgAAAAOEA4BHgAABA4EBAQDAAAAEREREQ4AAAAREQoKBAAAABEVFRUKAAAAEQoEChEAAAAREREPAQ4AAB8CBAgfAAADggwCAgOAAAICAgICAgAAHAQDBAQcAAAFCgAAAAAADxCmaGhmUI8";
            const string bbcFont = "AAAAAAAAAAAYGBgYGAAYAGxsbAAAAAAANjZ/Nn82NgAMP2g+C34YAGBmDBgwZgYAOGxsOG1mOwAMGDAAAAAAAAwYMDAwGAwAMBgMDAwYMAAAGH48fhgAAAAYGH4YGAAAAAAAAAAYGDAAAAB+AAAAAAAAAAAAGBgAAAYMGDBgAAA8Zm5+dmY8ABg4GBgYGH4APGYGDBgwfgA8ZgYcBmY8AAwcPGx+DAwAfmB8BgZmPAAcMGB8ZmY8AH4GDBgwMDAAPGZmPGZmPAA8ZmY+Bgw4AAAAGBgAGBgAAAAYGAAYGDAMGDBgMBgMAAAAfgB+AAAAMBgMBgwYMAA8ZgwYGAAYADxmbmpuYDwAPGZmfmZmZgB8ZmZ8ZmZ8ADxmYGBgZjwAeGxmZmZseAB+YGB8YGB+AH5gYHxgYGAAPGZgbmZmPABmZmZ+ZmZmAH4YGBgYGH4APgwMDAxsOABmbHhweGxmAGBgYGBgYH4AY3d/a2tjYwBmZnZ+bmZmADxmZmZmZjwAfGZmfGBgYAA8ZmZmamw2AHxmZnxsZmYAPGZgPAZmPAB+GBgYGBgYAGZmZmZmZjwAZmZmZmY8GABjY2trf3djAGZmPBg8ZmYAZmZmPBgYGAB+BgwYMGB+AHxgYGBgYHwAAGAwGAwGAAA+BgYGBgY+ABg8ZkIAAAAAAAAAAAAAAP8cNjB8MDB+AAAAPAY+Zj4AYGB8ZmZmfAAAADxmYGY8AAYGPmZmZj4AAAA8Zn5gPAAcMDB8MDAwAAAAPmZmPgY8YGB8ZmZmZgAYADgYGBg8ABgAOBgYGBhwYGBmbHhsZgA4GBgYGBg8AAAANn9ra2MAAAB8ZmZmZgAAADxmZmY8AAAAfGZmfGBgAAA+ZmY+BgcAAGx2YGBgAAAAPmA8BnwAMDB8MDAwHAAAAGZmZmY+AAAAZmZmPBgAAABja2t/NgAAAGY8GDxmAAAAZmZmPgY8AAB+DBgwfgAMGBhwGBgMABgYGAAYGBgAMBgYDhgYMAAxa0YAAAAAADxCmaGhmUI8";

            int paper;
            int pen;
            string font;
            if (UseSpeccyColors)
            {
                paper = 7;
                pen = 0;
                font = speccyFont;
            }
            else if (UseBbcColors)
            {
                paper = 0;
                pen = 7;
                font = bbcFont;
            }
            else
            {
                paper = 1;
                pen = 5;
                font = bbcFont;
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

            // Patch the bitmap font.
            speccy.TheCpu.MainMemory.LoadData(Convert.FromBase64String(font), 0x3D00);

            speccy.TheCpu.ResetAsync();
        };
    }
}