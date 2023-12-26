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
using CSharp.Utils;
using CSharp.Utils.Extensions;
using Speculator.Core.Utils;

namespace Speculator.Core;

public class ZxFileIo // todo - Implement .tap loading to support https://github.com/raxoft/z80test
{
    private readonly CPU m_cpu;
    private readonly ZxDisplay m_zxDisplay;
    public static string[] FileFilters { get; } = { "*.z80", "*.bin", "*.scr", "*.sna", "*.zip" };

    public ZxFileIo(CPU cpu, ZxDisplay zxDisplay)
    {
        m_cpu = cpu;
        m_zxDisplay = zxDisplay;
    }

    public void LoadFile(FileInfo fileInfo)
    {
        using var _ = m_cpu.ClockSync.CreatePauser();

        fileInfo.Refresh();
        if (!fileInfo.Exists)
            throw new FileNotFoundException(fileInfo.FullName);

        lock (m_cpu.CpuStepLock)
            LoadFileInternal(fileInfo);
    }

    private void LoadFileInternal(FileInfo fileInfo)
    {
        switch (fileInfo.Extension)
        {
            case ".zip":
            {
                var tempFile = ZipExtractor.ExtractZxFile(fileInfo);
                if (tempFile?.Exists != true)
                    return;
                LoadFileInternal(tempFile);
                tempFile.Delete();
                return;
            }
            case ".bin":
                LoadBIN(fileInfo);
                return;
            case ".sna":
                LoadSNA(fileInfo);
                return;
            case ".scr":
                LoadSCR(fileInfo);
                return;
            case ".z80":
                LoadZ80(fileInfo);
                return;
        }
    }

    private void LoadZ80(FileInfo file)
    {
        using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
        m_cpu.TheRegisters.Clear();
        m_cpu.TheRegisters.Main.A = (byte)stream.ReadByte();
        m_cpu.TheRegisters.Main.F = (byte)stream.ReadByte();
        m_cpu.TheRegisters.Main.BC = ReadZxWord(stream);
        m_cpu.TheRegisters.Main.HL = ReadZxWord(stream);
        m_cpu.TheRegisters.PC = ReadZxWord(stream);
        m_cpu.TheRegisters.SP = ReadZxWord(stream);
        m_cpu.TheRegisters.I = (byte)stream.ReadByte();
        m_cpu.TheRegisters.R = (byte)(stream.ReadByte() & 0x7F);

        var byte12 = (byte)stream.ReadByte();
        if (byte12 == 0xFF)
            byte12 = 0x01; // Version 1
        if ((byte12 & 0x01) != 0)
            m_cpu.TheRegisters.R |= 0x80;

        if (m_zxDisplay != null)
            m_zxDisplay.BorderAttr = (byte)((byte12 & 0x0e) >> 1);

        var isDataCompressed = (byte12 & 0x20) != 0;

        m_cpu.TheRegisters.Main.DE = ReadZxWord(stream);
        m_cpu.TheRegisters.Alt.BC = ReadZxWord(stream);
        m_cpu.TheRegisters.Alt.DE = ReadZxWord(stream);
        m_cpu.TheRegisters.Alt.HL = ReadZxWord(stream);
        m_cpu.TheRegisters.Alt.A = (byte)stream.ReadByte();
        m_cpu.TheRegisters.Alt.F = (byte)stream.ReadByte();
        m_cpu.TheRegisters.IY = ReadZxWord(stream);
        m_cpu.TheRegisters.IX = ReadZxWord(stream);
        m_cpu.TheRegisters.IFF1 = stream.ReadByte() != 0;
        m_cpu.TheRegisters.IFF2 = stream.ReadByte() != 0;
        m_cpu.TheRegisters.IM = (byte)(stream.ReadByte() & 0x03);

        Debug.Assert(stream.Position == 30);

        var isVersion1 = m_cpu.TheRegisters.PC != 0x0000;
        if (!isVersion1)
        {
            Logger.Instance.Warn("Unsupported Z80 version (Only v1 implemented).");
            return;
        }
        
        // Version 1
        var bytesToRead = (int)(stream.Length - stream.Position);
        var data = new List<byte>();
        for (var i = 0; i < bytesToRead; i++)
            data.Add((byte)stream.ReadByte());

        if (isDataCompressed)
        {
            var offset = 0;
            while (offset < data.Count - 4)
            {
                if (data[offset] == 0xED && data[offset + 1] == 0xED)
                {
                    var count = data[offset + 2];
                    var b = data[offset + 3];

                    data.RemoveRange(offset, 4);
                    for (var i = 0; i < count; i++)
                        data.Insert(offset, b);

                    offset += count;
                }
                else
                {
                    offset++;
                }
            }

            // Remove trailer.
            data.RemoveRange(data.Count - 4, 4);
        }

        Debug.Assert(data.Count <= 48 * 1024);
        m_cpu.MainMemory.LoadData(data, 0x4000);
    }

    private void LoadSCR(FileInfo file) =>
        m_cpu.MainMemory.LoadData(file.ReadAllBytes(), ZxDisplay.ScreenBase);

    public void SaveFile(FileInfo file)
    {
        using var _ = m_cpu.ClockSync.CreatePauser();
        lock (m_cpu.CpuStepLock)
        {
            switch (file.Extension)
            {
                case ".sna":
                    SaveSNA(file);
                    return;
            }
        }
    }
    
    private void LoadSNA(FileInfo file)
    {
        using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        {
            m_cpu.TheRegisters.Clear();
            m_cpu.TheRegisters.I = (byte)stream.ReadByte();
            m_cpu.TheRegisters.Alt.HL = ReadZxWord(stream);
            m_cpu.TheRegisters.Alt.DE = ReadZxWord(stream);
            m_cpu.TheRegisters.Alt.BC = ReadZxWord(stream);
            m_cpu.TheRegisters.Alt.AF = ReadZxWord(stream);
            m_cpu.TheRegisters.Main.HL = ReadZxWord(stream);
            m_cpu.TheRegisters.Main.DE = ReadZxWord(stream);
            m_cpu.TheRegisters.Main.BC = ReadZxWord(stream);
            m_cpu.TheRegisters.IY = ReadZxWord(stream);
            m_cpu.TheRegisters.IX = ReadZxWord(stream);
            var IFF = (byte)stream.ReadByte();
            m_cpu.TheRegisters.IFF1 = m_cpu.TheRegisters.IFF2 = (IFF & 0x02) != 0;
            m_cpu.TheRegisters.R = (byte)stream.ReadByte();
            m_cpu.TheRegisters.Main.AF = ReadZxWord(stream);
            m_cpu.TheRegisters.SP = ReadZxWord(stream);
            m_cpu.TheRegisters.IM = (byte)stream.ReadByte();
            m_zxDisplay.BorderAttr = (byte)stream.ReadByte();
            for (var i = 16384; i <= 65535; i++)
                m_cpu.MainMemory.Poke((ushort)i, (byte)stream.ReadByte());
        }

        m_cpu.RETN();
    }

    private static ushort ReadZxWord(Stream stream)
    {
        return (ushort)(stream.ReadByte() + (stream.ReadByte() << 8));
    }

    private static void WriteSNAWord(Stream stream, int n)
    {
        stream.WriteByte((byte)(n & 0x00FF));
        stream.WriteByte((byte)(n >> 8 & 0xFF));
    }

    private void SaveSNA(FileInfo file)
    {
        using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write);
        try
        {
            m_cpu.TheRegisters.SP -= 2;
            m_cpu.MainMemory.Poke(m_cpu.TheRegisters.SP, m_cpu.TheRegisters.PC);

            stream.WriteByte(m_cpu.TheRegisters.I);
            WriteSNAWord(stream, m_cpu.TheRegisters.Alt.HL);
            WriteSNAWord(stream, m_cpu.TheRegisters.Alt.DE);
            WriteSNAWord(stream, m_cpu.TheRegisters.Alt.BC);
            WriteSNAWord(stream, m_cpu.TheRegisters.Alt.AF);
            WriteSNAWord(stream, m_cpu.TheRegisters.Main.HL);
            WriteSNAWord(stream, m_cpu.TheRegisters.Main.DE);
            WriteSNAWord(stream, m_cpu.TheRegisters.Main.BC);
            WriteSNAWord(stream, m_cpu.TheRegisters.IY);
            WriteSNAWord(stream, m_cpu.TheRegisters.IX);
            stream.WriteByte((byte)(m_cpu.TheRegisters.IFF2 ? 0x02 : 0x00));
            stream.WriteByte(m_cpu.TheRegisters.R);
            WriteSNAWord(stream, m_cpu.TheRegisters.Main.AF);
            WriteSNAWord(stream, m_cpu.TheRegisters.SP);
            stream.WriteByte(m_cpu.TheRegisters.IM);
            stream.WriteByte(m_zxDisplay.BorderAttr);
            for (var i = 16384; i <= 65535; i++)
                stream.WriteByte(m_cpu.MainMemory.Peek((ushort)i));

        }
        finally
        {
            m_cpu.TheRegisters.SP += 2;
        }
    }

    private void LoadBIN(FileInfo file)
    {
        m_cpu.MainMemory.LoadData(file.ReadAllBytes(), 0x8000);
        m_cpu.TheRegisters.PC = 0x8000;
    }
}