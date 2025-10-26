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

using CSharp.Core.Extensions;

// ReSharper disable InconsistentNaming

namespace Speculator.Core;

public class Registers
{
    private readonly StorageRegisters[] m_storageRegisters = new StorageRegisters[2];

    private int MainRegIndex { get; set; }

    public ushort PC { get; set; }

    public class StorageRegisters
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
        
        /// <summary>
        /// Assigns a byte to the specified register.
        /// </summary>
        /// <remarks>A no-op if the register name is '\0'.</remarks>
        public void SetRegister(char regName, byte value)
        {
            switch (regName)
            {
                case '\0':
                    return; // No op
                case 'A':
                    A = value;
                    return;
                case 'B':
                    B = value;
                    return;
                case 'C':
                    C = value;
                    return;
                case 'D':
                    D = value;
                    return;
                case 'E':
                    E = value;
                    return;
                case 'H':
                    H = value;
                    return;
                case 'L':
                    L = value;
                    return;
                default:
                    throw new ArgumentException("Unknown register name.", nameof(regName));
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

    /// <summary>
    /// S:Bit 7 (7...0)
    /// </summary>
    public bool SignFlag
    {
        set => Main.F = value ? Main.F.SetBit(7) : Main.F.ResetBit(7);
        get => Main.F.IsBitSet(7);
    }

    /// <summary>
    /// Z:Bit 6 (7...0)
    /// </summary>
    public bool ZeroFlag
    {
        set => Main.F = value ? Main.F.SetBit(6) : Main.F.ResetBit(6);
        get => Main.F.IsBitSet(6);
    }

    /// <summary>
    /// 5:Bit 5 (7...0)
    /// </summary>
    public bool Flag5
    {
        internal set => Main.F = value ? Main.F.SetBit(5) : Main.F.ResetBit(5);
        get => Main.F.IsBitSet(5);
    }

    /// <summary>
    /// H:Bit 4 (7...0)
    /// </summary>
    public bool HalfCarryFlag
    {
        set => Main.F = value ? Main.F.SetBit(4) : Main.F.ResetBit(4);
        get => Main.F.IsBitSet(4);
    }

    /// <summary>
    /// 3:Bit 3 (7...0)
    /// </summary>
    public bool Flag3
    {
        internal set => Main.F = value ? Main.F.SetBit(3) : Main.F.ResetBit(3);
        get => Main.F.IsBitSet(3);
    }

    /// <summary>
    /// P:Bit 2 (7...0)
    /// </summary>
    public bool ParityFlag
    {
        set => Main.F = value ? Main.F.SetBit(2) : Main.F.ResetBit(2);
        get => Main.F.IsBitSet(2);
    }

    /// <summary>
    /// N:Bit 1 (7...0)
    /// </summary>
    public bool SubtractFlag
    {
        set => Main.F = value ? Main.F.SetBit(1) : Main.F.ResetBit(1);
        get => Main.F.IsBitSet(1);
    }

    /// <summary>
    /// C:Bit 0 (7...0)
    /// </summary>
    public bool CarryFlag
    {
        set => Main.F = value ? Main.F.SetBit(0) : Main.F.ResetBit(0);
        get => Main.F.IsBitSet(0);
    }
    
    public void SetFlags53From(ushort w) => SetFlags53From((byte)((w & 0xff00) >> 8));

    public void SetFlags53From(byte b)
    {
        Flag5 = b.IsBitSet(5);
        Flag3 = b.IsBitSet(3);
    }
    
    public void SetFlags53FromA() => SetFlags53From(Main.A);

    public ushort IXPlusD(byte d) =>
        (ushort)(IX + Alu.FromTwosCompliment(d));

    public ushort IYPlusD(byte d) =>
        (ushort)(IY + Alu.FromTwosCompliment(d));
}
