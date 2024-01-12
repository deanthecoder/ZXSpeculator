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

using System.Diagnostics;
using CSharp.Utils;
using CSharp.Utils.Extensions;

namespace Speculator.Core;

public class Memory
{
    private int m_romSize;

    /// <summary>
    /// Raised when a large chunk of data is loaded from an external source (I.e. Disk).
    /// </summary>
    public event EventHandler DataLoaded;

    public byte[] Data { get; } = new byte[0x10000];

    public byte Poke(ushort addr, byte value)
    {
        if (IsRom(addr))
            return Data[addr]; // Can't write to ROM.
        Data[addr] = value;
        return value;
    }

    public void Poke(ushort addr, ushort v)
    {
        Poke(addr, (byte)(v & 0x00ff));
        Poke((ushort)(addr + 1), (byte)(v >> 8));
    }

    public byte Peek(ushort addr) => Data[addr];
    
    public ushort PeekWord(ushort addr)
    {
        return (ushort)(Data[(ushort)(addr + 1)] << 8 | Data[addr]);
    }

    public string ReadAsHexString(ushort addr, ushort byteCount, bool wantSpaces = false)
    {
        Debug.Assert(byteCount > 0, "byteCount must be positive.");

        var result = string.Empty;
        for (var i = 0; i < byteCount && addr + i <= 0xffff; i++)
        {
            result += Peek((ushort)(addr + i)).ToString("X2");
            if (wantSpaces)
                result += " ";
        }

        return result.Trim();
    }

    public void LoadRom(string systemRom)
    {
        Logger.Instance.Info($"Loading ROM '{systemRom}'.");
        Debug.Assert(File.Exists(systemRom), "ROM file does not exist: " + systemRom);

        var romBytes = new FileInfo(systemRom).ReadAllBytes();
        Logger.Instance.Info($"ROM size: {romBytes.Length} bytes.");
        if (romBytes.Length > 0xffff)
        {
            Logger.Instance.Error("ROM is too large to fit in memory.");
            return;
        }

        Array.Clear(Data);
        m_romSize = romBytes.Length;
        LoadData(romBytes, 0x0000);
    }
    
    public bool IsRom(ushort addr) => addr < m_romSize;
    
    /// <summary>
    /// Bulk load data into memory (such as from disk).
    /// </summary>
    public void LoadData(IList<byte> data, ushort addr)
    {
        data.CopyTo(Data, addr);
        DataLoaded?.Invoke(this, EventArgs.Empty);
    }
}
