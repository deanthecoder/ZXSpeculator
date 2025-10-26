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
using System.Runtime.CompilerServices;

namespace Speculator.Core;

public class Instruction
{
    public Instruction(Z80Instructions.InstructionID id, string mnemonicTemplate, string hexTemplate, int tStateCount = 0)
    {
        Id = id;
        MnemonicTemplate = mnemonicTemplate;

        Debug.Assert(hexTemplate.Length >= 2, "Zero length opcodes are invalid.");
        Debug.Assert(!hexTemplate.Contains("nn"), "Invalid hex template. (Should be 'n n'?)");
        HexTemplate = hexTemplate;
        ByteCount = (byte)HexTemplate.Split(' ').Length;
        TStateCount = tStateCount;
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
    public Func<Memory, Registers, Alu, ushort, int> Run { get; internal init; }

    // Optimized matching cache.
    private byte[] m_fixedPrefix;
    private (byte Offset, byte Value)[] m_fixedOtherBytes;
    private int m_totalOpcodeLength;

    public bool StartsWithOpcodeBytes(Memory mainMemory, ushort addr)
    {
        EnsureOpcodePattern();

        // Fast path: compare against the raw memory span to avoid method-call overhead.
        var span = mainMemory.Data.AsSpan(addr);
        if (span.Length < m_totalOpcodeLength)
            return false;

        // Compare contiguous fixed prefix using vectorized SequenceEqual.
        if (m_fixedPrefix.Length != 0 && !span[..m_fixedPrefix.Length].SequenceEqual(m_fixedPrefix))
            return false;

        // Compare any remaining fixed bytes beyond the first variable.
        var other = m_fixedOtherBytes!;
        for (var i = 0; i < other.Length; i++)
        {
            var p = other[i];
            if (span[p.Offset] != p.Value)
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureOpcodePattern()
    {
        if (m_fixedPrefix != null)
            return;

        // Build fixed prefix and any scattered fixed bytes after the first variable (n/d).
        var tokens = HexTemplate.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        Debug.Assert(tokens.Length >= 1, "Opcode must consist of at least one byte.");
        m_totalOpcodeLength = tokens.Length;

        var prefix = new List<byte>(tokens.Length);
        var others = new List<(byte Offset, byte Value)>();

        var seenVariable = false;
        for (byte i = 0; i < tokens.Length; i++)
        {
            var tok = tokens[i];
            var isVar = tok.IndexOfAny(new[] { 'n', 'd' }) >= 0;
            if (!isVar)
            {
                var val = Convert.ToByte(tok, 16);
                if (!seenVariable)
                    prefix.Add(val);
                else
                    others.Add((i, val));
            }

            if (isVar)
                seenVariable = true;
        }

        m_fixedPrefix = prefix.ToArray();
        m_fixedOtherBytes = others.ToArray();
    }

    public override string ToString() => $"{MnemonicTemplate} ({HexTemplate})";
}
