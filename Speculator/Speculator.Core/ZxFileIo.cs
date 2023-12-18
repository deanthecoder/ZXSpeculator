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
using Speculator.Core.Utils;

namespace Speculator.Core;

public static class ZxFileIo
{
    public static string[] FileFilters { get; } = { "*.z80", "*.bin", "*.scr", "*.sna", "*.zip" };

    public static void LoadFile(CPU cpu, FileInfo fileInfo)
    {
        using var _ = cpu.ClockSync.CreatePauser();
        
        fileInfo.Refresh();
        if (!fileInfo.Exists)
            throw new FileNotFoundException(fileInfo.FullName);
        
        var resetDebuggingFlag = false;
        try
        {
            if (!cpu.IsDebugging)
            {
                cpu.IsDebugging = true;
                resetDebuggingFlag = true;
                Thread.Sleep(500);
            }

            LoadFileInternal(cpu, fileInfo);
        }
        finally
        {
            if (resetDebuggingFlag)
                cpu.IsDebugging = false;
        }
    }
    private static void LoadFileInternal(CPU cpu, FileInfo fileInfo)
    {

        switch (fileInfo.Extension)
        {
            case ".zip":
            {
                var tempFile = ZipExtractor.ExtractZxFile(fileInfo);
                if (tempFile?.Exists != true)
                    return;
                LoadFileInternal(cpu, tempFile);
                tempFile.Delete();
                return;
            }
            case ".bin":
                LoadBIN(fileInfo, cpu);
                return;
            case ".sna":
                LoadSNA(fileInfo, cpu);
                return;
            case ".scr":
                LoadSCR(fileInfo, cpu);
                return;
            case ".z80":
                LoadZ80(fileInfo, cpu);
                return;
        }
    }

    private static void LoadZ80(FileInfo file, CPU cpu)
    {
        using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
        cpu.TheRegisters.clear();
        cpu.TheRegisters.Main.A = (byte)stream.ReadByte();
        cpu.TheRegisters.Main.F = (byte)stream.ReadByte();
        cpu.TheRegisters.Main.BC = ReadSNAWord(stream);
        cpu.TheRegisters.Main.HL = ReadSNAWord(stream);
        cpu.TheRegisters.PC = ReadSNAWord(stream);
        cpu.TheRegisters.SP = ReadSNAWord(stream);
        cpu.TheRegisters.I = (byte)stream.ReadByte();
        cpu.TheRegisters.R = (byte)(stream.ReadByte() & 0x7F);

        var byte12 = (byte) stream.ReadByte();
        if (byte12 == 0xFF)
            byte12 = 0x01; // Version 1
        cpu.TheRegisters.R |= (byte)((byte12 & 0x01) * 0x80);
        var isDataCompressed = (byte12 & 0x20) != 0;

        cpu.TheRegisters.Main.DE = ReadSNAWord(stream);
        cpu.TheRegisters.Alt.BC = ReadSNAWord(stream);
        cpu.TheRegisters.Alt.DE = ReadSNAWord(stream);
        cpu.TheRegisters.Alt.HL = ReadSNAWord(stream);
        cpu.TheRegisters.Alt.A = (byte)stream.ReadByte();
        cpu.TheRegisters.Alt.F = (byte)stream.ReadByte();
        cpu.TheRegisters.IY = ReadSNAWord(stream);
        cpu.TheRegisters.IX = ReadSNAWord(stream);

        var IFF = (byte)stream.ReadByte();
        cpu.TheRegisters.IFF1 = cpu.TheRegisters.IFF2 = IFF != 0;
        stream.ReadByte(); // IFF2

        var byte29 = (byte)stream.ReadByte();
        cpu.TheRegisters.IM = (byte) (byte29 & 0x03);

        Debug.Assert(stream.Position == 30);

        var isVersion1 = cpu.TheRegisters.PC != 0x0000;

        if (isVersion1)
        {
            // Version 1
            var bytesToRead = (int) (stream.Length - stream.Position);
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
                        Debug.Assert(count >= 5);

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
                Debug.Assert(data[data.Count - 4] == 0x00);
                Debug.Assert(data[data.Count - 3] == 0xED);
                Debug.Assert(data[data.Count - 2] == 0xED);
                Debug.Assert(data[data.Count - 1] == 0x00);
                data.RemoveRange(data.Count - 4, 4);
            }

            Debug.Assert(data.Count <= 48 * 1024);

            data.CopyTo(cpu.MainMemory.Data, 0x4000);
            cpu.MainMemory.VideoMemoryChanged = true;
            return;
        }
        
        // todo - pop up a message.
        Console.WriteLine("Unsupported Z80 version (Only v1 implemented).");
    }

    private static void LoadSCR(FileInfo file, CPU cpu)
    {
        File.ReadAllBytes(file.FullName).CopyTo(cpu.MainMemory.Data, ZxDisplay.ScreenBase);
        cpu.MainMemory.VideoMemoryChanged = true;
    }

    public static void SaveFile(CPU cpu, FileInfo file)
    {
        var resetDebuggingFlag = false;
        try
        {
            if (!cpu.IsDebugging)
            {
                cpu.IsDebugging = true;
                resetDebuggingFlag = true;
                Thread.Sleep(500);
            }

            switch (file.Extension)
            {
                case ".sna":
                    SaveSNA(file, cpu);
                    return;
            }
        }
        finally
        {
            if (resetDebuggingFlag)
                cpu.IsDebugging = false;
        }
    }
    
    private static void LoadSNA(FileInfo file, CPU cpu)
    {
        using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        {
            cpu.TheRegisters.clear();
            cpu.TheRegisters.I = (byte)stream.ReadByte();
            cpu.TheRegisters.Alt.HL = ReadSNAWord(stream);
            cpu.TheRegisters.Alt.DE = ReadSNAWord(stream);
            cpu.TheRegisters.Alt.BC = ReadSNAWord(stream);
            cpu.TheRegisters.Alt.AF = ReadSNAWord(stream);
            cpu.TheRegisters.Main.HL = ReadSNAWord(stream);
            cpu.TheRegisters.Main.DE = ReadSNAWord(stream);
            cpu.TheRegisters.Main.BC = ReadSNAWord(stream);
            cpu.TheRegisters.IY = ReadSNAWord(stream);
            cpu.TheRegisters.IX = ReadSNAWord(stream);
            var IFF = (byte)stream.ReadByte();
            cpu.TheRegisters.IFF1 = cpu.TheRegisters.IFF2 = (IFF & 0x02) != 0;
            cpu.TheRegisters.R = (byte)stream.ReadByte();
            cpu.TheRegisters.Main.AF = ReadSNAWord(stream);
            cpu.TheRegisters.SP = ReadSNAWord(stream);
            cpu.TheRegisters.IM = (byte)stream.ReadByte();
            stream.ReadByte(); // Border color.
            for (var i = 16384; i <= 65535; i++)
                cpu.MainMemory.Poke(i, (byte)stream.ReadByte());
        }

        cpu.RETN();
    }

    private static int ReadSNAWord(Stream stream)
    {
        return stream.ReadByte() + (stream.ReadByte() << 8);
    }

    private static void WriteSNAWord(Stream stream, int n)
    {
        stream.WriteByte((byte)(n & 0x00FF));
        stream.WriteByte((byte)((n >> 8) & 0xFF));
    }

    private static void SaveSNA(FileInfo file, CPU cpu)
    {
        using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write);
        try
        {
            cpu.TheRegisters.SP -= 2;
            cpu.MainMemory.PokeWord(cpu.TheRegisters.SP, cpu.TheRegisters.PC);

            stream.WriteByte(cpu.TheRegisters.I);
            WriteSNAWord(stream, cpu.TheRegisters.Alt.HL);
            WriteSNAWord(stream, cpu.TheRegisters.Alt.DE);
            WriteSNAWord(stream, cpu.TheRegisters.Alt.BC);
            WriteSNAWord(stream, cpu.TheRegisters.Alt.AF);
            WriteSNAWord(stream, cpu.TheRegisters.Main.HL);
            WriteSNAWord(stream, cpu.TheRegisters.Main.DE);
            WriteSNAWord(stream, cpu.TheRegisters.Main.BC);
            WriteSNAWord(stream, cpu.TheRegisters.IY);
            WriteSNAWord(stream, cpu.TheRegisters.IX);
            stream.WriteByte((byte)(cpu.TheRegisters.IFF2 ? 0x02 : 0x00));
            stream.WriteByte(cpu.TheRegisters.R);
            WriteSNAWord(stream, cpu.TheRegisters.Main.AF);
            WriteSNAWord(stream, cpu.TheRegisters.SP);
            stream.WriteByte(cpu.TheRegisters.IM);
            stream.WriteByte(0x07); // Border color.
            for (var i = 16384; i <= 65535; i++)
                stream.WriteByte(cpu.MainMemory.Peek(i));

        }
        finally
        {
            cpu.TheRegisters.SP += 2;
        }
    }

    private static void LoadBIN(FileInfo file, CPU cpu)
    {
        using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
        var length = stream.Length;
        for (var i = 0; i < length; i++)
            cpu.MainMemory.Poke(0x8000 + i, (byte)stream.ReadByte());
        cpu.TheRegisters.PC = 0x8000;
    }
}