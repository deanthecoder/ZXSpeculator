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

using Speculator.Core.Extensions;

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
            var regSuffix = regs[i] != '\0' ? $",{regs}" : string.Empty;
            yield return CreateRlcIxPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateRrcIxPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateRlIxPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateRrIxPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSlaIxPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSraIxPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSllIxPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSrlIxPlusDInstruction(regs[i], i, regSuffix);

            for (byte bit = 0; bit < 8; bit++)
            {
                yield return CreateBitIxPlusDInstruction(i, bit);
                yield return CreateResIxPlusDInstruction(regs[i], i, regSuffix, bit);
            }
        }
    }
    
    private static Instruction CreateRlcIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RLC_addrIX_plus_d, $"RLC (IX+d){regSuffix}", $"DD CB d {0x00 + regIndex:X02}", tStates)
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
    
    private static Instruction CreateRrcIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RRC_addrIX_plus_d, $"RRC (IX+d){regSuffix}", $"DD CB d {0x08 + regIndex:X02}", tStates)
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

    private static Instruction CreateRlIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RL_addrIX_plus_d, $"RL (IX+d){regSuffix}", $"DD CB d {0x10 + regIndex:X02}", tStates)
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
    
    private static Instruction CreateRrIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RR_addrIX_plus_d, $"RR (IX+d){regSuffix}", $"DD CB d {0x18 + regIndex:X02}", tStates)
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
    
    private static Instruction CreateSlaIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SLA_addrIX_plus_d, $"SLA (IX+d){regSuffix}", $"DD CB d {0x20 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftLeft(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSraIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SRA_addrIX_plus_d, $"SRA (IX+d){regSuffix}", $"DD CB d {0x28 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftRightArithmetic(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSllIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SLL_addrIX_plus_d, $"SLL (IX+d){regSuffix}", $"DD CB d {0x30 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftLeftLogical(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSrlIxPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SRL_addrIX_plus_d, $"SRL (IX+d){regSuffix}", $"DD CB d {0x38 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftRightLogical(memory.Peek(ixPlusD));
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateBitIxPlusDInstruction(int regIndex, byte bit)
    {
        const int tStates = 20;
        return new Instruction(InstructionID.BIT_n_addrIX_plus_d, $"BIT {bit},(IX+d)", $"DD CB d {0x40 + bit * 8 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var b = memory.Peek(registers.IXPlusD(memory.Peek(valueAddress)));
            registers.ZeroFlag = !b.IsBitSet(bit);
            registers.SubtractFlag = false;
            registers.HalfCarryFlag = true;

            // From 'undocumented' docs.
            registers.ParityFlag = registers.ZeroFlag;
            registers.SignFlag = bit == 7 && !Alu.IsBytePositive(b);
            return tStates;
        }
    }

    private static Instruction CreateResIxPlusDInstruction(char regName, int regIndex, string regSuffix, byte bit)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SRL_addrIX_plus_d, $"RES {bit},(IX+d){regSuffix}", $"DD CB d {0x80 + bit * 8 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var ixPlusD = registers.IXPlusD(memory.Peek(valueAddress));
            var result = memory.Peek(ixPlusD).ResetBit(bit);
            memory.Poke(ixPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
}