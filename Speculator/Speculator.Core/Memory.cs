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

using System.Diagnostics;

namespace Speculator.Core;

public class Memory
{
    private readonly ZxDisplay m_zxDisplay;

    public int RomSize { get; set; }

    public Memory(ZxDisplay zxDisplay)
    {
        m_zxDisplay = zxDisplay;
        RomSize = 0;
        Data = new byte[0x10000];
    }

    public byte[] Data { get; }

    public void ClearAll()
    {
        RomSize = 0;
        for (var i = 0; i < Data.Length; i++)
            Poke((ushort)i, 0);
    }

    public void Poke(ushort addr, byte value)
    {
        if (addr < RomSize)
            return; // Can't write to ROM.

        if (Data[addr] == value)
            return; // No change.

        // Write to pixel or color area?
        m_zxDisplay.OnMemoryWrite(addr);
        
        Data[addr] = value;
    }

    public byte Peek(ushort addr) => Data[addr];

    public string ReadAsHexString(ushort addr, ushort byteCount, bool wantSpaces = false)
    {
        Debug.Assert(byteCount > 0, "byteCount must be positive.");

        var result = string.Empty;
        for (var i = 0; i < byteCount && addr + i < 0xffff; i++)
        {
            result += Peek((ushort)(addr + i)).ToString("X2");
            if (wantSpaces)
                result += " ";
        }

        return result.Trim();
    }

    public ushort PeekWord(ushort addr)
    {
        return (ushort)(Data[(ushort)(addr + 1)] << 8 | Data[addr]);
    }

    public void PokeWord(ushort addr, int v)
    {
        Poke(addr, (byte)(v & 0x00ff));
        Poke((ushort)(addr + 1), (byte)(v >> 8));
    }

    public void LoadBasicROM(string systemROM)
    {
        Debug.WriteLine($"Loading ROM '{systemROM}'.");

        Debug.Assert(File.Exists(systemROM), "BASIC ROM file does not exist: " + systemROM);

        var fileStream = File.OpenRead(systemROM);
        Debug.WriteLine("ROM size: {0} bytes.", fileStream.Length);
        Debug.Assert(fileStream.Length < 0xffff, "ROM is too large to fit in memory.");

        ClearAll();
        for (var i = 0; i < fileStream.Length; i++)
            Data[i] = (byte)fileStream.ReadByte();
            
        RomSize = (int)fileStream.Length;
    }
}