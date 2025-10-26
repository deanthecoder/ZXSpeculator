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

namespace Speculator.Core;

public partial class Z80Instructions
{
    private IEnumerable<Instruction> GetFdCbInstructions()
    {
        // Handled explicitly?
        foreach (var instr in m_instructions.Where(o => o.HexTemplate.Replace(" ", string.Empty).StartsWith("FDCB")))
            yield return instr;

        // Handled using Instruction callback method.
        var regs = new[]
        {
            'B', 'C', 'D', 'E', 'H', 'L', '\0', 'A'
        };
        for (var i = 0; i < regs.Length; i++)
        {
            var regSuffix = regs[i] != '\0' ? $",{regs[i]}" : string.Empty;
            yield return CreateRlcIyPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateRrcIyPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateRlIyPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateRrIyPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSlaIyPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSraIyPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSllIyPlusDInstruction(regs[i], i, regSuffix);
            yield return CreateSrlIyPlusDInstruction(regs[i], i, regSuffix);

            for (byte bit = 0; bit < 8; bit++)
            {
                yield return CreateBitIyPlusDInstruction(i, bit);
                yield return CreateResIyPlusDInstruction(regs[i], i, regSuffix, bit);
                yield return CreateSetIyPlusDInstruction(regs[i], i, regSuffix, bit);
            }
        }
    }
    
    private static Instruction CreateRlcIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RLC_addrIY_plus_d, $"RLC (IY+d){regSuffix}", $"FD CB d {0x00 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.RotateLeftCircular(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateRrcIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RRC_addrIY_plus_d, $"RRC (IY+d){regSuffix}", $"FD CB d {0x08 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.RotateRightCircular(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }

    private static Instruction CreateRlIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RL_addrIY_plus_d, $"RL (IY+d){regSuffix}", $"FD CB d {0x10 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.RotateLeft(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateRrIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.RR_addrIY_plus_d, $"RR (IY+d){regSuffix}", $"FD CB d {0x18 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.RotateRight(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSlaIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SLA_addrIY_plus_d, $"SLA (IY+d){regSuffix}", $"FD CB d {0x20 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftLeft(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSraIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SRA_addrIY_plus_d, $"SRA (IY+d){regSuffix}", $"FD CB d {0x28 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftRightArithmetic(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSllIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SLL_addrIY_plus_d, $"SLL (IY+d){regSuffix}", $"FD CB d {0x30 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftLeftLogical(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSrlIyPlusDInstruction(char regName, int regIndex, string regSuffix)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SRL_addrIY_plus_d, $"SRL (IY+d){regSuffix}", $"FD CB d {0x38 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = alu.ShiftRightLogical(memory.Peek(iyPlusD));
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateBitIyPlusDInstruction(int regIndex, byte bit)
    {
        const int tStates = 20;
        return new Instruction(InstructionID.BIT_n_addrIY_plus_d, $"BIT {bit},(IY+d)", $"FD CB d {0x40 + bit * 8 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var b = memory.Peek(iyPlusD);
            registers.ZeroFlag = !b.IsBitSet(bit);
            registers.SubtractFlag = false;
            registers.HalfCarryFlag = true;

            // From 'undocumented' docs.
            registers.ParityFlag = registers.ZeroFlag;
            registers.SignFlag = bit == 7 && !Alu.IsBytePositive(b);
            registers.SetFlags53From((byte)(iyPlusD >> 8));
            return tStates;
        }
    }

    private static Instruction CreateResIyPlusDInstruction(char regName, int regIndex, string regSuffix, byte bit)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SRL_addrIY_plus_d, $"RES {bit},(IY+d){regSuffix}", $"FD CB d {0x80 + bit * 8 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = memory.Peek(iyPlusD).ResetBit(bit);
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
    
    private static Instruction CreateSetIyPlusDInstruction(char regName, int regIndex, string regSuffix, byte bit)
    {
        const int tStates = 23;
        return new Instruction(InstructionID.SET_n_addrIY_plus_d, $"SET {bit},(IY+d){regSuffix}", $"FD CB d {0xC0 + bit * 8 + regIndex:X02}", tStates)
        {
            Run = Impl
        };

        int Impl(Memory memory, Registers registers, Alu alu, ushort valueAddress)
        {
            var iyPlusD = registers.IYPlusD(memory.Peek(valueAddress));
            var result = memory.Peek(iyPlusD).SetBit(bit);
            memory.Poke(iyPlusD, result);
            registers.Main.SetRegister(regName, result);
            return tStates;
        }
    }
}