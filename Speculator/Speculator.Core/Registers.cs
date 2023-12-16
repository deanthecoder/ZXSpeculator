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
    public int PC { get; set; }

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

        public int AF
        {
            get { return (A << 8) + F; }
            set
            {
                A = (byte)((value & 0xFF00) >> 8);
                F = (byte)(value & 0x00FF);
            }
        }

        public int BC
        {
            get { return (B << 8) + C; }
            set
            {
                B = (byte)((value & 0xFF00) >> 8);
                C = (byte)(value & 0x00FF);
            }
        }

        public int DE
        {
            get { return (D << 8) + E; }
            set
            {
                D = (byte)((value & 0xFF00) >> 8);
                E = (byte)(value & 0x00FF);
            }
        }

        public int HL
        {
            get { return (H << 8) + L; }
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
        get { return m_storageRegisters[m_mainRegIndex]; }
        set
        {
            m_storageRegisters[m_mainRegIndex] = value;
        }
    }

    public StorageRegisters Alt
    {
        get { return m_storageRegisters[(m_mainRegIndex + 1) % 2]; }
        set
        {
            m_storageRegisters[(m_mainRegIndex + 1) % 2] = value;
        }
    }

    // Hardware control.
    public byte I { get; set; }
    public byte R { get; set; }
    public bool IFF1 { get; set; }
    public bool IFF2 { get; set; }
    public byte IM { get; set; }

    public int IX
    {
        get { return (IXH << 8) + IXL; }
        set
        {
            IXH = (byte)((value & 0xFF00) >> 8);
            IXL = (byte)(value & 0x00FF);
        }
    }
    public byte IXH { get; set; }
    public byte IXL { get; set; }
    public int IY
    {
        get { return (IYH << 8) + IYL; }
        set
        {
            IYH = (byte)((value & 0xFF00) >> 8);
            IYL = (byte)(value & 0x00FF);
        }
    }
    public byte IYH { get; set; }
    public byte IYL { get; set; }
    public int SP { get; set; }

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
        set { Main.F = value ? (byte) (Main.F | (1 << 7)) : (byte) (Main.F & ~(1 << 7)); }
        get { return (Main.F & (1 << 7)) != 0; }
    }

    public bool ZeroFlag
    {
        set { Main.F = value ? (byte) (Main.F | (1 << 6)) : (byte) (Main.F & ~(1 << 6)); }
        get { return (Main.F & (1 << 6)) != 0; }
    }

    public bool HalfCarryFlag
    {
        set { Main.F = value ? (byte) (Main.F | (1 << 4)) : (byte) (Main.F & ~(1 << 4)); }
        get { return (Main.F & (1 << 4)) != 0; }
    }

    public bool ParityFlag
    {
        set { Main.F = value ? (byte) (Main.F | (1 << 2)) : (byte) (Main.F & ~(1 << 2)); }
        get { return (Main.F & (1 << 2)) != 0; }
    }

    public bool SubtractFlag
    {
        set { Main.F = value ? (byte) (Main.F | (1 << 1)) : (byte) (Main.F & ~(1 << 1)); }
        get { return (Main.F & (1 << 1)) != 0; }
    }

    public bool CarryFlag
    {
        set { Main.F = value ? (byte) (Main.F | (1 << 0)) : (byte) (Main.F & ~(1 << 0)); }
        get { return (Main.F & (1 << 0)) != 0; }
    }

    public override string ToString()
    {
        return string.Format("AF:{0:X4}, BC:{1:X4}, DE:{2:X4}, HL:{3:X4}, IX:{4:X4}, IY:{5:X4}, SP:{6:X4}, PC:{7:X4}, {8}, I:{9:X2}, R:{10:X2}", Main.AF, Main.BC, Main.DE, Main.HL, IX, IY, SP, PC, FlagsAsString(), I, R);
    }

    public string FlagsAsString()
    {
        return FlagsAsString(Main.F);
    }

    static public string FlagsAsString(int flags)
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
}