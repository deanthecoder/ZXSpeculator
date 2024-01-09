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

namespace Speculator.Core;

public partial class Z80Instructions
{
    // todo - Repeat for FD CB instruction (IY)
    private IEnumerable<Instruction> GetDdcbInstructions()
    {
        // Handled explicitly?
        foreach (var instr in Instructions.Where(o => o.HexTemplate.Replace(" ", string.Empty).StartsWith("DDCB")))
            yield return instr;

        // Handled using Instruction callback method.
        var regs = new[]
        {
            'B', 'C', 'D', 'E', 'H', 'L', '\0', 'A'
        };
        for (var i = 0; i < regs.Length; i++)
        {
            var r = regs[i];
            if (r == 0)
                continue;
            yield return CreateRlcIxPlusDInstruction(r, i);
            yield return CreateRrcIxPlusDInstruction(r, i);
            yield return CreateRlIxPlusDInstruction(r, i);
            yield return CreateRrIxPlusDInstruction(r, i);
        }
    }
    
    private static Instruction CreateRlcIxPlusDInstruction(char regName, int regIndex)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RLC_addrIX_plus_d_undoc, $"RLC (IX+d),{regName}", $"DD CB d {0x00 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.RotateLeftCircular(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateRrcIxPlusDInstruction(char regName, int regIndex)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RRC_addrIX_plus_d_undoc, $"RRC (IX+d),{regName}", $"DD CB d {0x08 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.RotateRightCircular(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }

    private static Instruction CreateRlIxPlusDInstruction(char regName, int regIndex)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RL_addrIX_plus_d_undoc, $"RL (IX+d),{regName}", $"DD CB d {0x10 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.RotateLeft(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateRrIxPlusDInstruction(char regName, int regIndex)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RR_addrIX_plus_d_undoc, $"RR (IX+d),{regName}", $"DD CB d {0x18 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.RotateRight(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
}