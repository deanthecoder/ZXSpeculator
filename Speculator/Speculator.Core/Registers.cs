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

using CSharp.Utils.ViewModels;
// ReSharper disable InconsistentNaming

namespace Speculator.Core;

public class Registers : ViewModelBase
{
    private readonly StorageRegisters[] m_storageRegisters = new StorageRegisters[2];

    private int MainRegIndex { get; set; }

    public ushort PC { get; set; }

    public class StorageRegisters : ViewModelBase
    {
        internal StorageRegisters()
        {
            Clear();
        }

        internal void Clear()
        {
            A = B = C = D = E = F = H = L = 0xFF;
        }

        public byte A { get; set; }
        public byte F { get; set; }
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

    public StorageRegisters Main => m_storageRegisters[MainRegIndex];
    public StorageRegisters Alt => m_storageRegisters[(MainRegIndex + 1) % 2];

    // Hardware control.
    public byte I { get; set; }
    
    public byte R { get; set; }

    /// <summary>
    /// True if interrupts are enabled.
    /// </summary>
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

        Clear();
    }

    public void ExchangeRegisterSet()
    {
        var mainAF = Main.AF;
        var altAF = Alt.AF;
        MainRegIndex = (MainRegIndex + 1) % 2;
        Main.AF = mainAF;
        Alt.AF = altAF;
    }

    public void Clear()
    {
        PC = 0;
        MainRegIndex = 0;
        SP = IX = IY = 0xFFFF;
        IFF1 = IFF2 = false;
        IM = 0;
        I = R = 0;

        Main.Clear();
        Alt.Clear();
    }

    public bool SignFlag
    {
        set => Main.F = value ? (byte)(Main.F | (1 << 7)) : (byte)(Main.F & ~(1 << 7));
        get => (Main.F & (1 << 7)) != 0;
    }

    public bool ZeroFlag
    {
        set => Main.F = value ? (byte)(Main.F | (1 << 6)) : (byte)(Main.F & ~(1 << 6));
        get => (Main.F & (1 << 6)) != 0;
    }

    public bool Flag5
    {
        private set => Main.F = value ? (byte)(Main.F | (1 << 5)) : (byte)(Main.F & ~(1 << 5));
        get => (Main.F & (1 << 5)) != 0;
    }

    public bool HalfCarryFlag
    {
        set => Main.F = value ? (byte)(Main.F | (1 << 4)) : (byte)(Main.F & ~(1 << 4));
        get => (Main.F & (1 << 4)) != 0;
    }

    public bool Flag3
    {
        private set => Main.F = value ? (byte)(Main.F | (1 << 3)) : (byte)(Main.F & ~(1 << 3));
        get => (Main.F & (1 << 3)) != 0;
    }

    public bool ParityFlag
    {
        set => Main.F = value ? (byte)(Main.F | (1 << 2)) : (byte)(Main.F & ~(1 << 2));
        get => (Main.F & (1 << 2)) != 0;
    }

    public bool SubtractFlag
    {
        set => Main.F = value ? (byte)(Main.F | (1 << 1)) : (byte)(Main.F & ~(1 << 1));
        get => (Main.F & (1 << 1)) != 0;
    }

    public bool CarryFlag
    {
        set => Main.F = value ? (byte)(Main.F | (1 << 0)) : (byte)(Main.F & ~(1 << 0));
        get => (Main.F & (1 << 0)) != 0;
    }
    
    public void SetFlags53From(ushort w) => SetFlags53From((byte)((w & 0xff00) >> 8));

    public void SetFlags53From(byte b)
    {
        Flag5 = (b & 0x20) != 0;
        Flag3 = (b & 0x08) != 0;
    }
    
    public void SetFlags53FromA() => SetFlags53From(Main.A);
}
