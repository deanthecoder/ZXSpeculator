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

// ReSharper disable InconsistentNaming
namespace Speculator.Core;

public class Registers
{
    public ushort PC { get; set; }

    public class StorageRegisters
    {
        internal StorageRegisters()
        {
            clear();
        }

        internal void clear()
        {
            A = B = C = D = E = F = H = L = 0xFF;
        }

        public byte A { get; set; }
        public byte F { get; set; } // Flags
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }

        public ushort AF
        {
            get => (ushort)((A << 8) + F);
            set
            {
                A = (byte)((value & 0xFF00) >> 8);
                F = (byte)(value & 0x00FF);
            }
        }

        public ushort BC
        {
            get => (ushort)((B << 8) + C);
            set
            {
                B = (byte)((value & 0xFF00) >> 8);
                C = (byte)(value & 0x00FF);
            }
        }

        public ushort DE
        {
            get => (ushort)((D << 8) + E);
            set
            {
                D = (byte)((value & 0xFF00) >> 8);
                E = (byte)(value & 0x00FF);
            }
        }

        public ushort HL
        {
            get => (ushort)((H << 8) + L);
            set
            {
                H = (byte)((value & 0xFF00) >> 8);
                L = (byte)(value & 0x00FF);
            }
        }
    }

    private readonly StorageRegisters[] m_storageRegisters = new StorageRegisters[2];
    private int m_mainRegIndex;
    public StorageRegisters Main
    {
        get => m_storageRegisters[m_mainRegIndex];
        set => m_storageRegisters[m_mainRegIndex] = value;
    }

    public StorageRegisters Alt
    {
        get => m_storageRegisters[(m_mainRegIndex + 1) % 2];
        set => m_storageRegisters[(m_mainRegIndex + 1) % 2] = value;
    }

    // Hardware control.
    public byte I { get; set; }
    public byte R { get; set; }
    public bool IFF1 { get; set; }
    public bool IFF2 { get; set; }
    public byte IM { get; set; }

    public ushort IX
    {
        get => (ushort)((IXH << 8) + IXL);
        set
        {
            IXH = (byte)((value & 0xFF00) >> 8);
            IXL = (byte)(value & 0x00FF);
        }
    }
    
    public byte IXH { get; set; }
    public byte IXL { get; set; }
    
    public ushort IY
    {
        get => (ushort)((IYH << 8) + IYL);
        set
        {
            IYH = (byte)((value & 0xFF00) >> 8);
            IYL = (byte)(value & 0x00FF);
        }
    }
    public byte IYH { get; set; }
    public byte IYL { get; set; }
    public ushort SP { get; set; }

    internal Registers()
    {
        m_storageRegisters[0] = new StorageRegisters();
        m_storageRegisters[1] = new StorageRegisters();

        clear();
    }

    public void exchangeRegisterSet()
    {
        var mainAF = Main.AF;
        var altAF = Alt.AF;
        m_mainRegIndex = (m_mainRegIndex + 1) % 2;
        Main.AF = mainAF;
        Alt.AF = altAF;
    }

    public void clear()
    {
        PC = 0;
        m_mainRegIndex = 0;
        SP = IX = IY = 0xFFFF;
        IFF1 = IFF2 = false;
        IM = 0;

        Main.clear();
        Alt.clear();
    }

    public bool SignFlag
    {
        set => Main.F = value ? (byte) (Main.F | (1 << 7)) : (byte) (Main.F & ~(1 << 7));
        get => (Main.F & (1 << 7)) != 0;
    }

    public bool ZeroFlag
    {
        set => Main.F = value ? (byte) (Main.F | (1 << 6)) : (byte) (Main.F & ~(1 << 6));
        get => (Main.F & (1 << 6)) != 0;
    }

    public bool Flag5
    {
        private set => Main.F = value ? (byte)(Main.F | (1 << 5)) : (byte)(Main.F & ~(1 << 5));
        get => (Main.F & (1 << 5)) != 0;
    }

    public bool HalfCarryFlag
    {
        set => Main.F = value ? (byte) (Main.F | (1 << 4)) : (byte) (Main.F & ~(1 << 4));
        get => (Main.F & (1 << 4)) != 0;
    }

    public bool Flag3
    {
        private set => Main.F = value ? (byte)(Main.F | (1 << 3)) : (byte)(Main.F & ~(1 << 3));
        get => (Main.F & (1 << 3)) != 0;
    }
    
    public bool ParityFlag
    {
        set => Main.F = value ? (byte) (Main.F | (1 << 2)) : (byte) (Main.F & ~(1 << 2));
        get => (Main.F & (1 << 2)) != 0;
    }

    public bool SubtractFlag
    {
        set => Main.F = value ? (byte) (Main.F | (1 << 1)) : (byte) (Main.F & ~(1 << 1));
        get => (Main.F & (1 << 1)) != 0;
    }

    public bool CarryFlag
    {
        set => Main.F = value ? (byte) (Main.F | (1 << 0)) : (byte) (Main.F & ~(1 << 0));
        get => (Main.F & (1 << 0)) != 0;
    }

    public override string ToString()
    {
        return $"AF:{Main.AF:X4}, BC:{Main.BC:X4}, DE:{Main.DE:X4}, HL:{Main.HL:X4}, IX:{IX:X4}, IY:{IY:X4}, SP:{SP:X4}, PC:{PC:X4}, {FlagsAsString()}, I:{I:X2}, R:{R:X2}";
    }

    public string FlagsAsString()
    {
        return FlagsAsString(Main.F);
    }

    public static string FlagsAsString(int flags)
    {
        var s = (flags & (1 << 7)) != 0 ? "S" : "s";
        s += (flags & (1 << 6)) != 0 ? "Z" : "z";
        s += "-";
        s += (flags & (1 << 4)) != 0 ? "H" : "h";
        s += "-";
        s += (flags & (1 << 2)) != 0 ? "P" : "p";
        s += (flags & (1 << 1)) != 0 ? "N" : "n";
        s += (flags & (1 << 0)) != 0 ? "C" : "c";
        return s;
    }

    public int RegisterValue(string regName)
    {
        switch (regName)
        {
            case "A":
                return Main.A;
            case "B":
                return Main.B;
            case "C":
                return Main.C;
            case "D":
                return Main.D;
            case "E":
                return Main.E;
            case "H":
                return Main.H;
            case "L":
                return Main.L;
            case "BC":
                return Main.BC;
            case "DE":
                return Main.DE;
            case "HL":
                return Main.HL;
            case "SP":
                return SP;
            case "PC":
                return PC;
            case "IX":
                return IX;
            case "IY":
                return IY;
            case "IXL":
                return IXL;
            case "IXH":
                return IXH;
            case "IYL":
                return IYL;
            case "IYH":
                return IYH;
            case "I":
                return I;
            case "R":
                return R;
            default:
                Debug.Fail("Unknown register name specified: " + regName);
                return 0;
        }
    }
    
    public void SetFlags53From(ushort w) => SetFlags53From((byte)((w & 0xff00) >> 8));

    public void SetFlags53From(byte b)
    {
        Flag5 = (b & 0x20) != 0;
        Flag3 = (b & 0x08) != 0;
    }
    
    public void SetFlags53FromA() => SetFlags53From(Main.A);
}