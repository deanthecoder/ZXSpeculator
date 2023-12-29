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

namespace Speculator.Core.Extensions;

public static class CpuExtensions
{
    public static ushort Disassemble(this CPU theCpu, ushort addr, ref string hexBytes, out string mnemonics)
    {
        var memory = theCpu.MainMemory;
        var instruction = theCpu.InstructionSet.findInstructionAtMemoryLocation(memory, addr);

        if (instruction == null)
        {
            // Unknown instruction.
            hexBytes = memory.ReadAsHexString(addr, 1);
            mnemonics = "??";
            return 1;
        }

        // Format the instruction as hex bytes.
        for (var i = 0; i < instruction.ByteCount; i++)
        {
            if (i > 0)
                hexBytes += " ";
            hexBytes += memory.ReadAsHexString((ushort)(addr + i), 1);
        }

        // Format the instruction as opcodes.
        var hexParts = instruction.HexTemplate.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hexValues = new List<string>();
        for (var i = 0; i < hexParts.Length; i++)
        {
            switch (hexParts[i])
            {
                case "n":
                    hexValues.Add(memory.ReadAsHexString((ushort)(addr + i), 1));
                    break;
                case "d":
                    hexValues.Add(Alu.FromTwosCompliment(memory.Peek((ushort)(addr + i))).ToString());
                    break;
            }
        }

        // LD hl,nn = LD hl,1234 = A3 n n => hexValues [34, 12]
        mnemonics = instruction.MnemonicTemplate;
        foreach (var hexValue in hexValues)
        {
            var index = Math.Max(mnemonics.LastIndexOf('n'), mnemonics.LastIndexOf('d'));
            mnemonics = mnemonics.Insert(index, hexValue).Remove(index + hexValue.Length, 1);
        }

        return instruction.ByteCount;
    }
}
