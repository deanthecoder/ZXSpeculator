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
    private readonly byte[] m_data;

    private int m_romSize;
    public int ROMSize
    {
        get => m_romSize;
        set
        {
            Debug.Assert(value <= m_maxAddress);
            m_romSize = value;
        }
    }

    private readonly int m_maxAddress;
    internal Memory(int maxAddress)
    {
        Debug.Assert(maxAddress > 0, "Invalid memory value specified.");
        m_maxAddress = maxAddress;

        ROMSize = 0;

        m_data = new byte[m_maxAddress];
    }

    public byte[] Data => m_data;

    public int MaxAddress => m_maxAddress;

    public void ClearAll()
    {
        ROMSize = 0;
        Array.Clear(Data, 0, Data.Length);
    }

    public void Poke(int addr, byte value)
    {
        if (addr < ROMSize)
            return;

        Data[addr] = value;
    }

    public byte Peek(int addr)
    {
        return Data[addr];
    }

    public string readAsHexString(int addr, int byteCount, bool wantSpaces = false)
    {
        Debug.Assert(byteCount > 0, "byteCount must be positive.");

        var result = string.Empty;
        for (var i = 0; i < byteCount && addr + i < MaxAddress; i++)
        {
            result += Peek(addr + i).ToString("X2");
            if (wantSpaces)
                result += " ";
        }

        return result.Trim();
    }

    public int PeekWord(int addr)
    {
        return Data[addr + 1] << 8 | Data[addr];
    }

    public void PokeWord(int addr, int v)
    {
        Debug.Assert(v >= 0 && v <= 0xffff);
        Poke(addr, (byte)(v & 0x00ff));
        Poke(addr + 1, (byte)(v >> 8));
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
            
        ROMSize = (int)fileStream.Length;
    }
}