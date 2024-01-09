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

using System.Diagnostics;

namespace Speculator.Core;

public class Instruction
{
    public Instruction(Z80Instructions.InstructionID id, string mnemonicTemplate, string hexTemplate, int TStateCount = 0)
    {
        Id = id;
        MnemonicTemplate = mnemonicTemplate;

        Debug.Assert(hexTemplate.Length >= 2, "Zero length opcodes are invalid.");
        Debug.Assert(!hexTemplate.Contains("nn"), "Invalid hex template. (Should be 'n n'?)");
        HexTemplate = hexTemplate;
        ByteCount = (byte)HexTemplate.Split(' ').Length;
        this.TStateCount = TStateCount;
    }

    public string MnemonicTemplate { get; }

    public string HexTemplate { get; }

    public int TStateCount { get; }

    public Z80Instructions.InstructionID Id { get; }

    public byte ByteCount { get; }

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
    
    /// <summary>
    /// Callback function allowing the Instruction to handle its own action.
    /// </summary>
    public Func<Memory, Registers, Alu, ushort, int> Run { get; internal set; }

    private byte?[] m_opcodeBytes;
    
    public bool StartsWithOpcodeBytes(Memory mainMemory, ushort addr)
    {
        m_opcodeBytes ??= InitOpcodeBytesLookup();

        // Now try to match all bytes.
        foreach (var opcodeByte in m_opcodeBytes)
        {
            if (opcodeByte != null && mainMemory.Peek(addr) != opcodeByte)
                return false;
            addr++;
        }

        return true;
    }
    
    private byte?[] InitOpcodeBytesLookup()
    {
        // Retrieve opcode byte sequence.
        var opcodes = HexTemplate.Split(new[] { ' ' });
        Debug.Assert(opcodes.Length >= 1, "Opcode must consist of at least one byte.");

        var opcodeBytes = new byte?[opcodes.Length];

        var i = 0;
        foreach (var opcode in opcodes)
        {
            if (opcode.IndexOfAny(new[] { 'n', 'd' }) < 0)
                opcodeBytes[i] = Convert.ToByte(opcode, 16);
            else
                opcodeBytes[i] = null;

            i++;
        }

        return opcodeBytes;
    }

    public override string ToString() => $"{MnemonicTemplate} ({HexTemplate})";
}
