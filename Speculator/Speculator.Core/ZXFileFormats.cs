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

public static class ZXFileFormats
{
    private const string FileFilters = "Z80 Files|*.z80|SNA Files|*.sna|BIN Files|*.bin|Screen Files|*.scr|All Supported Files|*.bin;*.sna;*.scr;*.z80";
    private const int DefaultFilterIndex = 5;

    public static void LoadFile(CPU cpu, FileInfo fileInfo)
    {
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

            switch (fileInfo.Extension)
            {
                case ".bin":
                    LoadBIN(fileInfo.FullName, cpu);
                    return;
                case ".sna":
                    LoadSNA(fileInfo.FullName, cpu);
                    return;
                case ".scr":
                    LoadSCR(fileInfo.FullName, cpu);
                    return;
                case ".z80":
                    LoadZ80(fileInfo.FullName, cpu);
                    return;
            }
        }
        finally
        {
            if (resetDebuggingFlag)
                cpu.IsDebugging = false;
        }
    }

    private static void LoadZ80(string fileName, CPU cpu)
    {
        if (!File.Exists(fileName))
            return;

        using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            cpu.TheRegisters.clear();
            cpu.TheRegisters.Main.A = (byte)stream.ReadByte();
            cpu.TheRegisters.Main.F = (byte)stream.ReadByte();
            cpu.TheRegisters.Main.BC = readSNAWord(stream);
            cpu.TheRegisters.Main.HL = readSNAWord(stream);
            cpu.TheRegisters.PC = readSNAWord(stream);
            cpu.TheRegisters.SP = readSNAWord(stream);
            cpu.TheRegisters.I = (byte)stream.ReadByte();
            cpu.TheRegisters.R = (byte)(stream.ReadByte() & 0x7F);

            var byte12 = (byte) stream.ReadByte();
            if (byte12 == 0xFF)
                byte12 = 0x01;  // Version 1
            cpu.TheRegisters.R |= (byte)((byte12 & 0x01) * 0x80);
            var isDataCompressed = (byte12 & 0x20) != 0;

            cpu.TheRegisters.Main.DE = readSNAWord(stream);
            cpu.TheRegisters.Alt.BC = readSNAWord(stream);
            cpu.TheRegisters.Alt.DE = readSNAWord(stream);
            cpu.TheRegisters.Alt.HL = readSNAWord(stream);
            cpu.TheRegisters.Alt.A = (byte)stream.ReadByte();
            cpu.TheRegisters.Alt.F = (byte)stream.ReadByte();
            cpu.TheRegisters.IY = readSNAWord(stream);
            cpu.TheRegisters.IX = readSNAWord(stream);

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
            }
            else
            {
                Debug.Fail("Unsupported Z80 version (Only v1 implemented).");
            }
        }
    }

    private static void LoadSCR(string fullName, CPU cpu)
    {
        File.ReadAllBytes(fullName).CopyTo(cpu.MainMemory.Data, ZXDisplay.ScreenBase);
    }

    public static void SaveFile(CPU cpu, string fileName)
    {
        var fileInfo = new FileInfo(fileName);

        var resetDebuggingFlag = false;
        try
        {
            if (!cpu.IsDebugging)
            {
                cpu.IsDebugging = true;
                resetDebuggingFlag = true;
                Thread.Sleep(500);
            }

            switch (fileInfo.Extension)
            {
                case ".sna":
                    SaveSNA(fileInfo.FullName, cpu);
                    return;
            }
        }
        finally
        {
            if (resetDebuggingFlag)
                cpu.IsDebugging = false;
        }
    }

    /*
    private static string m_lastSaveFolder;
    public static void SaveFile(CPU cpu)
    {
        SaveFileDialog dlg = new SaveFileDialog
        {
            Filter = FileFilters,
            InitialDirectory = m_lastSaveFolder ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
            FilterIndex = DefaultFilterIndex
        };

        bool? dialogResult = dlg.ShowDialog();
        if ((bool)(!dialogResult)) return;

        m_lastSaveFolder = new FileInfo(dlg.FileName).DirectoryName;
        SaveFile(cpu, dlg.FileName);
    }
    */

    private static void LoadSNA(string fileName, CPU cpu)
    {
        if (!File.Exists(fileName))
            return;

        using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            cpu.TheRegisters.clear();
            cpu.TheRegisters.I = (byte)stream.ReadByte();
            cpu.TheRegisters.Alt.HL = readSNAWord(stream);
            cpu.TheRegisters.Alt.DE = readSNAWord(stream);
            cpu.TheRegisters.Alt.BC = readSNAWord(stream);
            cpu.TheRegisters.Alt.AF = readSNAWord(stream);
            cpu.TheRegisters.Main.HL = readSNAWord(stream);
            cpu.TheRegisters.Main.DE = readSNAWord(stream);
            cpu.TheRegisters.Main.BC = readSNAWord(stream);
            cpu.TheRegisters.IY = readSNAWord(stream);
            cpu.TheRegisters.IX = readSNAWord(stream);
            var IFF = (byte)stream.ReadByte();
            cpu.TheRegisters.IFF1 = cpu.TheRegisters.IFF2 = (IFF & 0x02) != 0;
            cpu.TheRegisters.R = (byte)stream.ReadByte();
            cpu.TheRegisters.Main.AF = readSNAWord(stream);
            cpu.TheRegisters.SP = readSNAWord(stream);
            cpu.TheRegisters.IM = (byte)stream.ReadByte();
            stream.ReadByte(); // Border color.
            for (var i = 16384; i <= 65535; i++)
                cpu.MainMemory.Poke(i, (byte)stream.ReadByte());
        }

        cpu.RETN();
    }

    private static int readSNAWord(FileStream stream)
    {
        return stream.ReadByte() + (stream.ReadByte() << 8);
    }

    private static void writeSNAWord(FileStream stream, int n)
    {
        stream.WriteByte((byte)(n & 0x00FF));
        stream.WriteByte((byte)((n >> 8) & 0xFF));
    }

    private static void SaveSNA(string fileName, CPU cpu)
    {
        using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            try
            {
                cpu.TheRegisters.SP -= 2;
                cpu.MainMemory.PokeWord(cpu.TheRegisters.SP, cpu.TheRegisters.PC);

                stream.WriteByte(cpu.TheRegisters.I);
                writeSNAWord(stream, cpu.TheRegisters.Alt.HL);
                writeSNAWord(stream, cpu.TheRegisters.Alt.DE);
                writeSNAWord(stream, cpu.TheRegisters.Alt.BC);
                writeSNAWord(stream, cpu.TheRegisters.Alt.AF);
                writeSNAWord(stream, cpu.TheRegisters.Main.HL);
                writeSNAWord(stream, cpu.TheRegisters.Main.DE);
                writeSNAWord(stream, cpu.TheRegisters.Main.BC);
                writeSNAWord(stream, cpu.TheRegisters.IY);
                writeSNAWord(stream, cpu.TheRegisters.IX);
                stream.WriteByte((byte)(cpu.TheRegisters.IFF2 ? 0x02 : 0x00));
                stream.WriteByte(cpu.TheRegisters.R);
                writeSNAWord(stream, cpu.TheRegisters.Main.AF);
                writeSNAWord(stream, cpu.TheRegisters.SP);
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
    }

    private static void LoadBIN(string fileName, CPU cpu)
    {
        if (!File.Exists(fileName))
            return;

        using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            var length = stream.Length;
            for (var i = 0; i < length; i++)
                cpu.MainMemory.Poke(0x8000 + i, (byte)stream.ReadByte());
            cpu.TheRegisters.PC = 0x8000;
        }
    }
}