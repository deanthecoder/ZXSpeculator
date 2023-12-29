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

    public void Poke(ushort addr, byte value)
    {
        if (IsRom(addr))
            return; // Can't write to ROM.
        Data[addr] = value;
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
        Debug.WriteLine($"Loading ROM '{systemRom}'.");

        Debug.Assert(File.Exists(systemRom), "ROM file does not exist: " + systemRom);

        var fileStream = File.OpenRead(systemRom);
        Debug.WriteLine("ROM size: {0} bytes.", fileStream.Length);
        Debug.Assert(fileStream.Length <= 0xffff, "ROM is too large to fit in memory.");

        Array.Clear(Data);
        var romBytes = new FileInfo(systemRom).ReadAllBytes();
        m_romSize = romBytes.Length;
        LoadData(romBytes, 0x0000);
    }
    
    public bool IsRom(ushort addr) => addr < m_romSize;
    
    /// <summary>
    /// Bulk load data into memory (such as from disk).
    /// </summary>
    public void LoadData(IList<byte> data, int addr)
    {
        data.CopyTo(Data, addr);
        DataLoaded?.Invoke(this, EventArgs.Empty);
    }
}
