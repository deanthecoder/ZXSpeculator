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

[DebuggerDisplay("{MnemonicTemplate} ({HexTemplate})")]
public class Instruction
{
    private readonly int m_TStateCount;
    private readonly string m_hexTemplate;
    private readonly Z80Instructions.InstructionID m_id;
    private readonly string m_mnemonicTemplate;

    private string m_flagModifiers;

    internal string FlagModifiers
    {
        get
        {
            return m_flagModifiers;
        }
        set
        {
            Debug.Assert(string.IsNullOrEmpty(m_flagModifiers), "FlagModifiers already set.");
            Debug.Assert(value.Length == 6);
            m_flagModifiers = value;
        }
    }

    protected internal string ResultRegName { get; set; }

    public Instruction(Z80Instructions.InstructionID id, string mnemonicTemplate, string hexTemplate, int TStateCount = 0)
    {
        m_id = id;
        m_mnemonicTemplate = mnemonicTemplate;

        Debug.Assert(hexTemplate.Length >= 2, "Zero length opcodes are invalid.");
        Debug.Assert(!hexTemplate.Contains("nn"), "Invalid hex template. (Should be 'n n'?)");
        m_hexTemplate = hexTemplate;
        ByteCount = HexTemplate.Split(' ').Length;
        m_TStateCount = TStateCount;
    }

    public string MnemonicTemplate
    {
        get { return m_mnemonicTemplate; }
    }

    public string HexTemplate
    {
        get { return m_hexTemplate; }
    }

    public int TStateCount
    {
        get { return m_TStateCount; }
    }

    public Z80Instructions.InstructionID ID
    {
        get { return m_id; }
    }

    public int ByteCount { get; set; }

    private int m_valueByteOffset = -1;
    public int ValueByteOffset
    {
        get
        {
            if (m_valueByteOffset == -1)
            {
                m_valueByteOffset = 0;
                var hex = HexTemplate.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < hex.Length; i++)
                {
                    if (hex[i] == "n" || hex[i] == "d")
                    {
                        m_valueByteOffset = i;
                        break;
                    }
                }
            }

            return m_valueByteOffset;
        }
    }

    private byte?[] m_opcodeBytes;
    public bool startsWithOpcodeBytes(Memory mainMemory, int addr)
    {
        if (m_opcodeBytes == null)
        {
            // Retrieve opcode byte sequence.
            var opcodes = HexTemplate.Split(new[] { ' ' });
            Debug.Assert(opcodes.Length >= 1, "Opcode must consist of at least one byte.");

            m_opcodeBytes = new byte?[opcodes.Length];

            var i = 0;
            foreach (var opcode in opcodes)
            {
                if (opcode.IndexOfAny(new[] { 'n', 'd' }) < 0)
                    m_opcodeBytes[i] = Convert.ToByte(opcode, 16);
                else
                    m_opcodeBytes[i] = null;

                i++;
            }
        }

        // Now try to match all bytes.
        foreach (var opcodeByte in m_opcodeBytes)
        {
            if (opcodeByte != null && mainMemory.Peek(addr) != opcodeByte)
                return false;
            addr++;
        }

        return true;
    }

    private static char FlagNameForBit(int n)
    {
        return new[] { 'S', 'Z', 'H', 'P', 'N', 'C' }[n];
    }

    readonly int[] RegisterFlagBits = new[] { 7, 6, 4, 2, 1, 0 };

    public bool CheckFlagChanges(byte oldFlags, CPU cpu)
    {
        if (string.IsNullOrEmpty(FlagModifiers)) return true;

        var newFlags = cpu.TheRegisters.Main.F;

        // Known state tests.
        var i = 0;
        foreach (var bit in RegisterFlagBits)
        {
            var oldBit = (oldFlags & (1 << bit)) != 0;
            var newBit = (newFlags & (1 << bit)) != 0;

            switch (FlagModifiers[i])
            {
                case '0': 
                    if (newBit)
                        Debug.Fail(string.Format("Flag bit {0} should be 0. ({1})", FlagNameForBit(i), ID));
                    break;
                case '1':
                    if (!newBit)
                        Debug.Fail(string.Format("Flag bit {0} should be 1. ({1})", FlagNameForBit(i), ID));
                    break;
                case ' ': 
                    if (oldBit != newBit)
                        Debug.Fail(string.Format("Flag bit {0} should be unchanged. ({1})", FlagNameForBit(i), ID));
                    break;
            }

            i++;
        }

        if (ResultRegName != null)
        {
            var result = cpu.RegisterValue(ResultRegName);

            // Sign flag tests.
            if (FlagModifiers[0] == 'X')
            {
                bool expected;

                if (ResultRegName.Length == 2)
                    expected = (result & 0x8000) != 0;
                else
                    expected = (result & 0x80) != 0;

                if (expected != cpu.TheRegisters.SignFlag)
                    Debug.Fail(string.Format("Sign bit should be {0}. ({1} - Checking register {2})", expected, ID, ResultRegName));
            }

            // Zero flag tests.
            if (FlagModifiers[1] == 'X')
            {
                var expected = result == 0;
                if (expected != cpu.TheRegisters.ZeroFlag)
                    Debug.Fail(string.Format("Zero bit should be {0}. ({1} - Checking register {2})", expected, ID, ResultRegName));
            }

            // Parity flag tests.
            switch (FlagModifiers[3])
            {
                case 'P':
                    var expected = ALU.isEvenParity((byte)result);
                    if (expected != cpu.TheRegisters.ParityFlag)
                        Debug.Fail(string.Format("Parity bit should be {0}. ({1} - Checking register {2})", expected, ID, ResultRegName));
                    break;
            }
        }

        return true;
    }

    public void SetFlagModifiers(string flags, string resultRegName = null)
    {
        FlagModifiers = flags;
        ResultRegName = resultRegName;
    }
}