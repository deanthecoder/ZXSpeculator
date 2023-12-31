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

using CSharp.Utils;
using Speculator.Core.Extensions;

namespace Speculator.Core;

public partial class CPU
{
    private bool m_opcodeWarningIssued;

    /// <summary>
    /// Execute the instruction at the current (program counter) address.
    /// </summary>
    /// <remarks>The program counter will automatically be incremented.</remarks>
    /// <returns>The number of TStates used by the instruction.</returns>
    private int Tick()
    {
        var opcodeByte = MainMemory.Peek(TheRegisters.PC);
        var hasOpcodePrefix = opcodeByte is 0xDD or 0xFD or 0xED or 0xCB;
        if (hasOpcodePrefix)
        {
            // R increased each time an opcode is read - The prefix is a 'bonus' +1.
            IncrementR();
        }
        
        var instruction = InstructionSet.FindInstructionAtMemoryLocation(MainMemory, TheRegisters.PC);
        if (instruction != null)
            return ExecuteInstruction(instruction);

        // Note: Some of these prefixed instructions will be handled above.
        try
        {
            switch (opcodeByte)
            {
                case 0xDD:
                case 0xFD:
                    // These prefixes mean 'treat HL in next instruction as IX/IY'.
                    // z80-documented-v0.91.pdf says we can treat this opcode prefix as a NOP
                    // and process the next opcode as normal.
                    if (!m_opcodeWarningIssued)
                        Logger.Instance.Warn($"Ignoring {opcodeByte:X2} prefix for opcode {MainMemory.ReadAsHexString(TheRegisters.PC, 4, true)} (Disabling future warnings).");
                    var nop = ExecuteInstruction(InstructionSet.Nop, false); // Don't increment R - We did it above.
                    return nop + Tick();

                case 0xED:
                    var nextByte = MainMemory.Peek((ushort)(TheRegisters.PC + 1));
                    if (nextByte <= 0x3F || nextByte >= 0x80)
                    {
                        // These are legit able to be treated as 'nop nop' instructions.
                        // See https://mdfs.net/Docs/Comp/Z80/UnDocOps
                    }
                    else
                    {
                        // Not yet supported in this emulator.
                        if (!m_opcodeWarningIssued)
                            Logger.Instance.Warn($"Ignoring ED prefix for opcode {MainMemory.ReadAsHexString(TheRegisters.PC, 4, true)} (Disabling future warnings).");
                    }
                    
                    return ExecuteInstruction(InstructionSet.NopNop);

                default:
                    throw new UnsupportedInstruction(this);
            }
        }
        finally
        {
            m_opcodeWarningIssued = true;
        }
    }

    /// <summary>
    /// Execute a specific instruction.
    /// </summary>
    /// <remarks>The program counter will automatically be incremented.</remarks>
    /// <returns>The number of TStates used by the instruction.</returns>
    private int ExecuteInstruction(Instruction instruction, bool incrementR = true)
    {
        var instructionAddress = TheRegisters.PC;
        TheRegisters.PC += instruction.ByteCount;
        var valueAddress = (ushort)(instructionAddress + instruction.ValueByteOffset);

        // R increased each time an opcode is read.
        if (incrementR)
            IncrementR();

        switch (instruction.Id)
        {
            case Z80Instructions.InstructionID.NOP:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.NOPNOP:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_n:
                TheRegisters.Main.A = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_n:
                TheRegisters.Main.B = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_n:
                TheRegisters.Main.C = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_n:
                TheRegisters.Main.D = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_n:
                TheRegisters.Main.E = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_n:
                TheRegisters.Main.H = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_n:
                TheRegisters.Main.L = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addr:
                TheRegisters.Main.A = MainMemory.Peek(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_A:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_B:
                TheRegisters.Main.A = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_C:
                TheRegisters.Main.A = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_D:
                TheRegisters.Main.A = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_E:
                TheRegisters.Main.A = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_H:
                TheRegisters.Main.A = TheRegisters.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_L:
                TheRegisters.Main.A = TheRegisters.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_A:
                TheRegisters.Main.B = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_B:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_C:
                TheRegisters.Main.B = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_D:
                TheRegisters.Main.B = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_E:
                TheRegisters.Main.B = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_H:
                TheRegisters.Main.B = TheRegisters.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_L:
                TheRegisters.Main.B = TheRegisters.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_A:
                TheRegisters.Main.C = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_B:
                TheRegisters.Main.C = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_C:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_D:
                TheRegisters.Main.C = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_E:
                TheRegisters.Main.C = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_H:
                TheRegisters.Main.C = TheRegisters.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_L:
                TheRegisters.Main.C = TheRegisters.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_A:
                TheRegisters.Main.D = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_B:
                TheRegisters.Main.D = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_C:
                TheRegisters.Main.D = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_D:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_E:
                TheRegisters.Main.D = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_H:
                TheRegisters.Main.D = TheRegisters.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_L:
                TheRegisters.Main.D = TheRegisters.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_A:
                TheRegisters.Main.E = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_B:
                TheRegisters.Main.E = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_C:
                TheRegisters.Main.E = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_D:
                TheRegisters.Main.E = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_E:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_H:
                TheRegisters.Main.E = TheRegisters.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_L:
                TheRegisters.Main.E = TheRegisters.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_A:
                TheRegisters.Main.H = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_B:
                TheRegisters.Main.H = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_C:
                TheRegisters.Main.H = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_D:
                TheRegisters.Main.H = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_E:
                TheRegisters.Main.H = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_H:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_L:
                TheRegisters.Main.H = TheRegisters.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_A:
                TheRegisters.Main.L = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_B:
                TheRegisters.Main.L = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_C:
                TheRegisters.Main.L = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_D:
                TheRegisters.Main.L = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_E:
                TheRegisters.Main.L = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_H:
                TheRegisters.Main.L = TheRegisters.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_L:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrHL:
                TheRegisters.Main.A = MainMemory.Peek(TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_addrHL:
                TheRegisters.Main.B = MainMemory.Peek(TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_addrHL:
                TheRegisters.Main.C = MainMemory.Peek(TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_addrHL:
                TheRegisters.Main.D = MainMemory.Peek(TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_addrHL:
                TheRegisters.Main.E = MainMemory.Peek(TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_addrHL:
                TheRegisters.Main.H = MainMemory.Peek(TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_addrHL:
                TheRegisters.Main.L = MainMemory.Peek(TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrIXplus_d:
                TheRegisters.Main.A =
                    MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrIYplus_d:
                TheRegisters.Main.A =
                    MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_addrIXplus_d:
                TheRegisters.Main.B =
                    MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_addrIYplus_d:
                TheRegisters.Main.B =
                    MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_addrIXplus_d:
                TheRegisters.Main.C =
                    MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_addrIYplus_d:
                TheRegisters.Main.C =
                    MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_addrIXplus_d:
                TheRegisters.Main.D =
                    MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_addrIYplus_d:
                TheRegisters.Main.D =
                    MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_addrIXplus_d:
                TheRegisters.Main.E =
                    MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_addrIYplus_d:
                TheRegisters.Main.E =
                    MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_addrIXplus_d:
                TheRegisters.Main.H =
                    MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_addrIYplus_d:
                TheRegisters.Main.H =
                    MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_addrIXplus_d:
                TheRegisters.Main.L =
                    MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_addrIYplus_d:
                TheRegisters.Main.L =
                    MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IX_nn:
                TheRegisters.IX = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IY_nn:
                TheRegisters.IY = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IX_addr:
                TheRegisters.IX = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IY_addr:
                TheRegisters.IY = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_n:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_A:
                MainMemory.Poke(TheRegisters.Main.HL, TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_B:
                MainMemory.Poke(TheRegisters.Main.HL, TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_C:
                MainMemory.Poke(TheRegisters.Main.HL, TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_D:
                MainMemory.Poke(TheRegisters.Main.HL, TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_E:
                MainMemory.Poke(TheRegisters.Main.HL, TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_H:
                MainMemory.Poke(TheRegisters.Main.HL, TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_L:
                MainMemory.Poke(TheRegisters.Main.HL, TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_n:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), MainMemory.Peek((ushort)(valueAddress + 1)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_n:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), MainMemory.Peek((ushort)(valueAddress + 1)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_A:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_B:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_C:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_D:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_E:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_H:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_L:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_A:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_B:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_C:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_D:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_E:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_H:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_L:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrBC:
                TheRegisters.Main.A = MainMemory.Peek(TheRegisters.Main.BC);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrDE:
                TheRegisters.Main.A = MainMemory.Peek(TheRegisters.Main.DE);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrBC_A:
                MainMemory.Poke(TheRegisters.Main.BC, TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrDE_A:
                MainMemory.Poke(TheRegisters.Main.DE, TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_A:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_I:
                TheRegisters.Main.A = TheRegisters.I;
                TheRegisters.SignFlag = !Alu.IsBytePositive(TheRegisters.I);
                TheRegisters.ZeroFlag = TheRegisters.I == 0;
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.ParityFlag = TheRegisters.IFF2;
                TheRegisters.SubtractFlag = false;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_R:
                TheRegisters.Main.A = TheRegisters.R;
                TheRegisters.SignFlag = !Alu.IsBytePositive(TheRegisters.R);
                TheRegisters.ZeroFlag = TheRegisters.R == 0;
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.ParityFlag = TheRegisters.IFF2;
                TheRegisters.SubtractFlag = false;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_I_A:
                TheRegisters.I = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_R_A:
                TheRegisters.R = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_BC_nn:
                TheRegisters.Main.BC = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_DE_nn:
                TheRegisters.Main.DE = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_HL_nn:
                TheRegisters.Main.HL = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_nn:
                TheRegisters.SP = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_BC_addr:
                TheRegisters.Main.BC = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_DE_addr:
                TheRegisters.Main.DE = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_HL_addr:
                TheRegisters.Main.HL = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_addr:
                TheRegisters.SP = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_BC:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), TheRegisters.Main.BC);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_DE:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), TheRegisters.Main.DE);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_SP:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), TheRegisters.SP);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_IX:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), TheRegisters.IX);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_IY:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), TheRegisters.IY);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_HL:
                TheRegisters.SP = TheRegisters.Main.HL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_IX:
                TheRegisters.SP = TheRegisters.IX;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_IY:
                TheRegisters.SP = TheRegisters.IY;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_AF:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.Main.AF);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_BC:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.Main.BC);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_DE:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.Main.DE);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_HL:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_IX:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.IX);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_IY:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.IY);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_AF:
                TheRegisters.Main.AF = MainMemory.PeekWord(TheRegisters.SP);
                TheRegisters.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_BC:
                TheRegisters.Main.BC = MainMemory.PeekWord(TheRegisters.SP);
                TheRegisters.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_DE:
                TheRegisters.Main.DE = MainMemory.PeekWord(TheRegisters.SP);
                TheRegisters.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_HL:
                TheRegisters.Main.HL = MainMemory.PeekWord(TheRegisters.SP);
                TheRegisters.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_IX:
                TheRegisters.IX = MainMemory.PeekWord(TheRegisters.SP);
                TheRegisters.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_IY:
                TheRegisters.IY = MainMemory.PeekWord(TheRegisters.SP);
                TheRegisters.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.EX_DE_HL:
            {
                (TheRegisters.Main.DE, TheRegisters.Main.HL) = (TheRegisters.Main.HL, TheRegisters.Main.DE);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EX_AF_altAF:
            {
                (TheRegisters.Main.AF, TheRegisters.Alt.AF) = (TheRegisters.Alt.AF, TheRegisters.Main.AF);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EXX:
                TheRegisters.ExchangeRegisterSet();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.EX_addrSP_HL:
            {
                var v = TheRegisters.Main.HL;
                TheRegisters.Main.HL = MainMemory.PeekWord(TheRegisters.SP);
                MainMemory.Poke(TheRegisters.SP, v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EX_addrSP_IX:
            {
                var v = TheRegisters.IX;
                TheRegisters.IX = MainMemory.PeekWord(TheRegisters.SP);
                MainMemory.Poke(TheRegisters.SP, v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EX_addrSP_IY:
            {
                var v = TheRegisters.IY;
                TheRegisters.IY = MainMemory.PeekWord(TheRegisters.SP);
                MainMemory.Poke(TheRegisters.SP, v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.LDD:
                doLDD();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LDDR:
            {
                doLDD();
                if (TheRegisters.Main.BC != 0)
                    TheRegisters.PC -= 2;
                return TheRegisters.Main.BC != 0 ? 21 : 16;
            }
            case Z80Instructions.InstructionID.LDI:
                doLDI();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LDIR:
            {
                doLDI();
                if (TheRegisters.Main.BC != 0)
                    TheRegisters.PC -= 2;
                return TheRegisters.Main.BC != 0 ? 21 : 16;
            }
            case Z80Instructions.InstructionID.CPD:
                doCPD();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CPDR:
            {
                doCPD();
                if (!TheRegisters.ZeroFlag && TheRegisters.Main.BC != 0)
                    TheRegisters.PC -= 2;
                TheRegisters.ParityFlag = TheRegisters.Main.BC != 0;
                return TheRegisters.Main.BC != 0 ? 21 : 16;
            }
            case Z80Instructions.InstructionID.CPI:
                doCPI();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CPIR:
            {
                doCPI();
                if (!TheRegisters.ZeroFlag && TheRegisters.Main.BC != 0)
                    TheRegisters.PC -= 2;
                TheRegisters.ParityFlag = TheRegisters.Main.BC != 0;
                return TheRegisters.Main.BC != 0 && TheRegisters.Main.A != MainMemory.Peek(TheRegisters.Main.HL) ? 21 : 16;
            }
            case Z80Instructions.InstructionID.ADC_A_n:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_A:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.A, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_B:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.B, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_C:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.C, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_D:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.D, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_E:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.E, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_H:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.H, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_L:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.L, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_addrHL:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.Main.HL), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_addrIXplus_d:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_addrIYplus_d:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_n:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(valueAddress), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_A:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.A, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_B:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.B, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_C:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.C, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_D:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.D, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_E:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.E, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_H:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.H, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_L:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.L, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_addrHL:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.Main.HL), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_addrIXplus_d:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_addrIYplus_d:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_A:
                TheRegisters.Main.A = TheAlu.DecAndSetFlags(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_B:
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_C:
                TheRegisters.Main.C = TheAlu.DecAndSetFlags(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_D:
                TheRegisters.Main.D = TheAlu.DecAndSetFlags(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_E:
                TheRegisters.Main.E = TheAlu.DecAndSetFlags(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_H:
                TheRegisters.Main.H = TheAlu.DecAndSetFlags(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_L:
                TheRegisters.Main.L = TheAlu.DecAndSetFlags(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.DecAndSetFlags(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_addrIXplus_d:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheAlu.DecAndSetFlags(MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_addrIYplus_d:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheAlu.DecAndSetFlags(MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_A:
                TheRegisters.Main.A = TheAlu.IncAndSetFlags(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_B:
                TheRegisters.Main.B = TheAlu.IncAndSetFlags(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_C:
                TheRegisters.Main.C = TheAlu.IncAndSetFlags(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_D:
                TheRegisters.Main.D = TheAlu.IncAndSetFlags(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_E:
                TheRegisters.Main.E = TheAlu.IncAndSetFlags(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_H:
                TheRegisters.Main.H = TheAlu.IncAndSetFlags(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_L:
                TheRegisters.Main.L = TheAlu.IncAndSetFlags(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.IncAndSetFlags(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_addrIXplus_d:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), TheAlu.IncAndSetFlags(MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_addrIYplus_d:
                MainMemory.Poke(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)), TheAlu.IncAndSetFlags(MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_n:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_A:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.A, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_B:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.B, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_C:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.C, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_D:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.D, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_E:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.E, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_H:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.H, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_L:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.L, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_addrHL:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.Main.HL), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_addrIXplus_d:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_addrIYplus_d:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_n:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(valueAddress), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_A:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.A, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_B:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.B, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_C:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.C, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_D:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.D, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_E:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.E, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_H:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.H, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_L:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.L, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_addrHL:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.Main.HL), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_addrIXplus_d:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_addrIYplus_d:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_BC:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.BC, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_DE:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.DE, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_HL:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.HL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_SP:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.SP, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_BC:
                TheRegisters.Main.HL = TheAlu.SubtractAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.BC, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_DE:
                TheRegisters.Main.HL = TheAlu.SubtractAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.DE, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_HL:
                TheRegisters.Main.HL = TheAlu.SubtractAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.HL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_SP:
                TheRegisters.Main.HL = TheAlu.SubtractAndSetFlags(TheRegisters.Main.HL, TheRegisters.SP, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_BC:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.BC, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_DE:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.DE, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_HL:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.Main.HL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_SP:
                TheRegisters.Main.HL = TheAlu.AddAndSetFlags(TheRegisters.Main.HL, TheRegisters.SP, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_BC:
                TheRegisters.IX = TheAlu.AddAndSetFlags(TheRegisters.IX, TheRegisters.Main.BC, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_DE:
                TheRegisters.IX = TheAlu.AddAndSetFlags(TheRegisters.IX, TheRegisters.Main.DE, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_IX:
                TheRegisters.IX = TheAlu.AddAndSetFlags(TheRegisters.IX, TheRegisters.IX, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_SP:
                TheRegisters.IX = TheAlu.AddAndSetFlags(TheRegisters.IX, TheRegisters.SP, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_BC:
                TheRegisters.IY = TheAlu.AddAndSetFlags(TheRegisters.IY, TheRegisters.Main.BC, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_DE:
                TheRegisters.IY = TheAlu.AddAndSetFlags(TheRegisters.IY, TheRegisters.Main.DE, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_IY:
                TheRegisters.IY = TheAlu.AddAndSetFlags(TheRegisters.IY, TheRegisters.IY, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_SP:
                TheRegisters.IY = TheAlu.AddAndSetFlags(TheRegisters.IY, TheRegisters.SP, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_BC:
                TheRegisters.Main.BC = (ushort)(TheRegisters.Main.BC - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_DE:
                TheRegisters.Main.DE = (ushort)(TheRegisters.Main.DE - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_HL:
                TheRegisters.Main.HL = (ushort)(TheRegisters.Main.HL - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_SP:
                TheRegisters.SP = (ushort)(TheRegisters.SP - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IX:
                TheRegisters.IX--;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IY:
                TheRegisters.IY--;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_BC:
                TheRegisters.Main.BC++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_DE:
                TheRegisters.Main.DE++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_HL:
                TheRegisters.Main.HL++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_SP:
                TheRegisters.SP++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IX:
                TheRegisters.IX++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IY:
                TheRegisters.IY++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_n:
                TheAlu.And(MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_A:
                TheAlu.And(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_B:
                TheAlu.And(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_C:
                TheAlu.And(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_D:
                TheAlu.And(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_E:
                TheAlu.And(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_H:
                TheAlu.And(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_L:
                TheAlu.And(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_addrHL:
                TheAlu.And(MainMemory.Peek(TheRegisters.Main.HL));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_addrIX_plus_d:
                TheAlu.And(MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_addrIY_plus_d:
                TheAlu.And(MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_n:
            {
                var b = MainMemory.Peek(valueAddress);
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, b, false);
                TheRegisters.SetFlags53From(b);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.CP_A:
            {
                var a = TheRegisters.Main.A;
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.A, false);
                TheRegisters.SetFlags53From(a);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.CP_B:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.B, false);
                TheRegisters.SetFlags53From(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_C:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.C, false);
                TheRegisters.SetFlags53From(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_D:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.D, false);
                TheRegisters.SetFlags53From(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_E:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.E, false);
                TheRegisters.SetFlags53From(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_H:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.H, false);
                TheRegisters.SetFlags53From(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_L:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.Main.L, false);
                TheRegisters.SetFlags53From(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_addrHL:
            {
                var b = MainMemory.Peek(TheRegisters.Main.HL);
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, b, false);
                TheRegisters.SetFlags53From(b);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.CP_addrIX_plus_d:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_addrIY_plus_d:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_n:
                TheAlu.Or(MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_A:
                TheAlu.Or(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_B:
                TheAlu.Or(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_C:
                TheAlu.Or(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_D:
                TheAlu.Or(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_E:
                TheAlu.Or(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_H:
                TheAlu.Or(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_L:
                TheAlu.Or(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_addrHL:
                TheAlu.Or(MainMemory.Peek(TheRegisters.Main.HL));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_addrIX_plus_d:
                TheAlu.Or(MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_addrIY_plus_d:
                TheAlu.Or(MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_n:
                TheAlu.Xor(MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_A:
                TheAlu.Xor(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_B:
                TheAlu.Xor(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_C:
                TheAlu.Xor(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_D:
                TheAlu.Xor(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_E:
                TheAlu.Xor(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_H:
                TheAlu.Xor(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_L:
                TheAlu.Xor(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_addrHL:
                TheAlu.Xor(MainMemory.Peek(TheRegisters.Main.HL));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_addrIX_plus_d:
                TheAlu.Xor(MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_addrIY_plus_d:
                TheAlu.Xor(MainMemory.Peek(TheRegisters.IYPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CCF:
                TheRegisters.HalfCarryFlag = TheRegisters.CarryFlag;
                TheRegisters.CarryFlag = !TheRegisters.CarryFlag;
                TheRegisters.SubtractFlag = false;
                TheRegisters.SetFlags53From(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CPL:
                TheRegisters.HalfCarryFlag = true;
                TheRegisters.SubtractFlag = true;
                TheRegisters.Main.A = (byte)~TheRegisters.Main.A;
                TheRegisters.SetFlags53From(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DAA:
                TheAlu.AdjustAccumulatorToBcd();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.EI:
                TheRegisters.IFF1 = TheRegisters.IFF2 = true;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DI:
                TheRegisters.IFF1 = TheRegisters.IFF2 = false;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.HALT:
                IsHalted = true;
                TheRegisters.PC--;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IM0:
                TheRegisters.IM = 0;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IM1:
                TheRegisters.IM = 1;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IM2:
                TheRegisters.IM = 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.NEG:
            {
                var newA = TheAlu.SubtractAndSetFlags((byte)0x00, TheRegisters.Main.A, false);
                TheRegisters.ParityFlag = TheRegisters.Main.A == 0x80;
                TheRegisters.CarryFlag = TheRegisters.Main.A != 0x00;
                TheRegisters.Main.A = newA;
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.SCF:
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.CarryFlag = true;
                TheRegisters.SubtractFlag = false;
                TheRegisters.SetFlags53FromA();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_A:
                TheRegisters.Main.A = TheAlu.RotateLeft(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_B:
                TheRegisters.Main.B = TheAlu.RotateLeft(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_C:
                TheRegisters.Main.C = TheAlu.RotateLeft(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_D:
                TheRegisters.Main.D = TheAlu.RotateLeft(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_E:
                TheRegisters.Main.E = TheAlu.RotateLeft(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_H:
                TheRegisters.Main.H = TheAlu.RotateLeft(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_L:
                TheRegisters.Main.L = TheAlu.RotateLeft(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.RotateLeft(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLA:
            {
                var b = TheRegisters.Main.A;
                var oldHighBit = (byte)(b & 0x80);
                var oldCarry = (byte)(TheAlu.TheRegisters.CarryFlag ? 1 : 0);
                b <<= 1;
                b |= oldCarry;
                TheAlu.TheRegisters.HalfCarryFlag = false;
                TheAlu.TheRegisters.SubtractFlag = false;
                TheAlu.TheRegisters.CarryFlag = oldHighBit != 0x00;
                TheRegisters.Main.A = b;
                TheRegisters.SetFlags53From(b);

                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.RLC_A:
                TheRegisters.Main.A = TheAlu.RotateLeftCircular(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_B:
                TheRegisters.Main.B = TheAlu.RotateLeftCircular(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_C:
                TheRegisters.Main.C = TheAlu.RotateLeftCircular(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_D:
                TheRegisters.Main.D = TheAlu.RotateLeftCircular(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_E:
                TheRegisters.Main.E = TheAlu.RotateLeftCircular(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_H:
                TheRegisters.Main.H = TheAlu.RotateLeftCircular(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_L:
                TheRegisters.Main.L = TheAlu.RotateLeftCircular(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLCA:
                TheRegisters.CarryFlag = (TheRegisters.Main.A & 0x80) != 0x00;
                TheRegisters.Main.A = (byte)((TheRegisters.Main.A << 1) + (TheRegisters.Main.A >> 7));
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.SubtractFlag = false;
                TheRegisters.SetFlags53From(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.RotateLeftCircular(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_A:
                TheRegisters.Main.A = TheAlu.RotateRight(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_B:
                TheRegisters.Main.B = TheAlu.RotateRight(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_C:
                TheRegisters.Main.C = TheAlu.RotateRight(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_D:
                TheRegisters.Main.D = TheAlu.RotateRight(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_E:
                TheRegisters.Main.E = TheAlu.RotateRight(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_H:
                TheRegisters.Main.H = TheAlu.RotateRight(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_L:
                TheRegisters.Main.L = TheAlu.RotateRight(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.RotateRight(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRA:
            {
                var oldCarry = (byte)(TheRegisters.CarryFlag ? 1 : 0);
                TheRegisters.CarryFlag = (TheRegisters.Main.A & 0x01) != 0x00;
                TheRegisters.Main.A = (byte)((TheRegisters.Main.A >> 1) + (oldCarry << 7));
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.SubtractFlag = false;
                TheRegisters.SetFlags53From(TheRegisters.Main.A);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.RRC_A:
                TheRegisters.Main.A = TheAlu.RotateRightCircular(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_B:
                TheRegisters.Main.B = TheAlu.RotateRightCircular(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_C:
                TheRegisters.Main.C = TheAlu.RotateRightCircular(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_D:
                TheRegisters.Main.D = TheAlu.RotateRightCircular(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_E:
                TheRegisters.Main.E = TheAlu.RotateRightCircular(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_H:
                TheRegisters.Main.H = TheAlu.RotateRightCircular(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_L:
                TheRegisters.Main.L = TheAlu.RotateRightCircular(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.RotateRightCircular(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRCA:
            {
                TheRegisters.CarryFlag = (TheRegisters.Main.A & 0x01) != 0x00;
                var v = (byte)((TheRegisters.Main.A >> 1) + ((TheRegisters.Main.A & 0x01) << 7));
                TheRegisters.Main.A = v;
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.SubtractFlag = false;
                TheRegisters.SetFlags53From(v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.SLA_A:
                TheRegisters.Main.A = TheAlu.ShiftLeft(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_B:
                TheRegisters.Main.B = TheAlu.ShiftLeft(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_C:
                TheRegisters.Main.C = TheAlu.ShiftLeft(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_D:
                TheRegisters.Main.D = TheAlu.ShiftLeft(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_E:
                TheRegisters.Main.E = TheAlu.ShiftLeft(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_H:
                TheRegisters.Main.H = TheAlu.ShiftLeft(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_L:
                TheRegisters.Main.L = TheAlu.ShiftLeft(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.ShiftLeft(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_A:
                TheRegisters.Main.A = TheAlu.ShiftLeft(TheRegisters.Main.A, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_B:
                TheRegisters.Main.B = TheAlu.ShiftLeft(TheRegisters.Main.B, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_C:
                TheRegisters.Main.C = TheAlu.ShiftLeft(TheRegisters.Main.C, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_D:
                TheRegisters.Main.D = TheAlu.ShiftLeft(TheRegisters.Main.D, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_E:
                TheRegisters.Main.E = TheAlu.ShiftLeft(TheRegisters.Main.E, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_H:
                TheRegisters.Main.H = TheAlu.ShiftLeft(TheRegisters.Main.H, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_L:
                TheRegisters.Main.L = TheAlu.ShiftLeft(TheRegisters.Main.L, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.ShiftLeft(MainMemory.Peek(TheRegisters.Main.HL), 1));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_A:
                TheRegisters.Main.A = TheAlu.ShiftRightArithmetic(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_B:
                TheRegisters.Main.B = TheAlu.ShiftRightArithmetic(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_C:
                TheRegisters.Main.C = TheAlu.ShiftRightArithmetic(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_D:
                TheRegisters.Main.D = TheAlu.ShiftRightArithmetic(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_E:
                TheRegisters.Main.E = TheAlu.ShiftRightArithmetic(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_H:
                TheRegisters.Main.H = TheAlu.ShiftRightArithmetic(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_L:
                TheRegisters.Main.L = TheAlu.ShiftRightArithmetic(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.ShiftRightArithmetic(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_A:
                TheRegisters.Main.A = TheAlu.ShiftRightLogical(TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_B:
                TheRegisters.Main.B = TheAlu.ShiftRightLogical(TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_C:
                TheRegisters.Main.C = TheAlu.ShiftRightLogical(TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_D:
                TheRegisters.Main.D = TheAlu.ShiftRightLogical(TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_E:
                TheRegisters.Main.E = TheAlu.ShiftRightLogical(TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_H:
                TheRegisters.Main.H = TheAlu.ShiftRightLogical(TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_L:
                TheRegisters.Main.L = TheAlu.ShiftRightLogical(TheRegisters.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, TheAlu.ShiftRightLogical(MainMemory.Peek(TheRegisters.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_A:
                doBitTest(TheRegisters.Main.A, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_A:
                doBitTest(TheRegisters.Main.A, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_A:
                doBitTest(TheRegisters.Main.A, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_A:
                doBitTest(TheRegisters.Main.A, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_A:
                doBitTest(TheRegisters.Main.A, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_A:
                doBitTest(TheRegisters.Main.A, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_A:
                doBitTest(TheRegisters.Main.A, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_A:
                doBitTest(TheRegisters.Main.A, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_B:
                doBitTest(TheRegisters.Main.B, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_B:
                doBitTest(TheRegisters.Main.B, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_B:
                doBitTest(TheRegisters.Main.B, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_B:
                doBitTest(TheRegisters.Main.B, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_B:
                doBitTest(TheRegisters.Main.B, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_B:
                doBitTest(TheRegisters.Main.B, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_B:
                doBitTest(TheRegisters.Main.B, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_B:
                doBitTest(TheRegisters.Main.B, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_C:
                doBitTest(TheRegisters.Main.C, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_C:
                doBitTest(TheRegisters.Main.C, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_C:
                doBitTest(TheRegisters.Main.C, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_C:
                doBitTest(TheRegisters.Main.C, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_C:
                doBitTest(TheRegisters.Main.C, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_C:
                doBitTest(TheRegisters.Main.C, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_C:
                doBitTest(TheRegisters.Main.C, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_C:
                doBitTest(TheRegisters.Main.C, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_D:
                doBitTest(TheRegisters.Main.D, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_D:
                doBitTest(TheRegisters.Main.D, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_D:
                doBitTest(TheRegisters.Main.D, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_D:
                doBitTest(TheRegisters.Main.D, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_D:
                doBitTest(TheRegisters.Main.D, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_D:
                doBitTest(TheRegisters.Main.D, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_D:
                doBitTest(TheRegisters.Main.D, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_D:
                doBitTest(TheRegisters.Main.D, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_E:
                doBitTest(TheRegisters.Main.E, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_E:
                doBitTest(TheRegisters.Main.E, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_E:
                doBitTest(TheRegisters.Main.E, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_E:
                doBitTest(TheRegisters.Main.E, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_E:
                doBitTest(TheRegisters.Main.E, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_E:
                doBitTest(TheRegisters.Main.E, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_E:
                doBitTest(TheRegisters.Main.E, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_E:
                doBitTest(TheRegisters.Main.E, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_H:
                doBitTest(TheRegisters.Main.H, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_H:
                doBitTest(TheRegisters.Main.H, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_H:
                doBitTest(TheRegisters.Main.H, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_H:
                doBitTest(TheRegisters.Main.H, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_H:
                doBitTest(TheRegisters.Main.H, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_H:
                doBitTest(TheRegisters.Main.H, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_H:
                doBitTest(TheRegisters.Main.H, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_H:
                doBitTest(TheRegisters.Main.H, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_L:
                doBitTest(TheRegisters.Main.L, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_L:
                doBitTest(TheRegisters.Main.L, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_L:
                doBitTest(TheRegisters.Main.L, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_L:
                doBitTest(TheRegisters.Main.L, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_L:
                doBitTest(TheRegisters.Main.L, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_L:
                doBitTest(TheRegisters.Main.L, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_L:
                doBitTest(TheRegisters.Main.L, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_L:
                doBitTest(TheRegisters.Main.L, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_addrHL:
                doBitTest(MainMemory.Peek(TheRegisters.Main.HL), 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_A:
                TheRegisters.Main.A = TheRegisters.Main.A.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_B:
                TheRegisters.Main.B = TheRegisters.Main.B.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_C:
                TheRegisters.Main.C = TheRegisters.Main.C.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_D:
                TheRegisters.Main.D = TheRegisters.Main.D.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_E:
                TheRegisters.Main.E = TheRegisters.Main.E.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_H:
                TheRegisters.Main.H = TheRegisters.Main.H.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_L:
                TheRegisters.Main.L = TheRegisters.Main.L.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(0));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(1));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(2));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(3));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(4));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(5));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(6));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).ResetBit(7));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_n_addrIX_plus_d:
                MainMemory.Poke(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress)), MainMemory.Peek(TheRegisters.IXPlusD(MainMemory.Peek(valueAddress))).ResetBit(0));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_A:
                TheRegisters.Main.A = TheRegisters.Main.A.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_B:
                TheRegisters.Main.B = TheRegisters.Main.B.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_C:
                TheRegisters.Main.C = TheRegisters.Main.C.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_D:
                TheRegisters.Main.D = TheRegisters.Main.D.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_E:
                TheRegisters.Main.E = TheRegisters.Main.E.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_H:
                TheRegisters.Main.H = TheRegisters.Main.H.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_L:
                TheRegisters.Main.L = TheRegisters.Main.L.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(0));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(1));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(2));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(3));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(4));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(5));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(6));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_addrHL:
                MainMemory.Poke(TheRegisters.Main.HL, MainMemory.Peek(TheRegisters.Main.HL).SetBit(7));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CALL_nn:
                CallIfTrue(MainMemory.PeekWord(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CALL_NZ_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.ZeroFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_Z_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.ZeroFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_NC_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.CarryFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_C_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.CarryFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_PO_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.ParityFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_PE_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.ParityFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_P_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.SignFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_M_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.SignFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_DJNZ_n:
                TheRegisters.Main.B--;
                if (TheRegisters.Main.B == 0)
                    return 8;
                TheRegisters.PC = (ushort)(TheRegisters.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress)));
                return 13;
            case Z80Instructions.InstructionID.JP_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_NZ_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.ZeroFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_Z_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.ZeroFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_NC_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.CarryFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_C_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.CarryFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_PO_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.ParityFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_PE_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.ParityFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_P_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !TheRegisters.SignFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_M_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), TheRegisters.SignFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_addrHL:
                JumpIfTrue(TheRegisters.Main.HL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_addr_IX:
                JumpIfTrue(TheRegisters.IX, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_addr_IY:
                JumpIfTrue(TheRegisters.IY, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JR_n:
                JumpIfTrue((ushort)(TheRegisters.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JR_NZ_n:
                return JumpIfTrue((ushort)(TheRegisters.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), !TheRegisters.ZeroFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.JR_Z_n:
                return JumpIfTrue((ushort)(TheRegisters.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), TheRegisters.ZeroFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.JR_NC_n:
                return JumpIfTrue((ushort)(TheRegisters.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), !TheRegisters.CarryFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.JR_C_n:
                return JumpIfTrue((ushort)(TheRegisters.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), TheRegisters.CarryFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.RET:
                doRet();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RET_NZ:
                if (!TheRegisters.ZeroFlag)
                {
                    doRet();
                    return 11;
                }

                return 5;
            case Z80Instructions.InstructionID.RET_Z:
                if (!TheRegisters.ZeroFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_NC:
                if (TheRegisters.CarryFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_C:
                if (!TheRegisters.CarryFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_PO:
                if (TheRegisters.ParityFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_PE:
                if (!TheRegisters.ParityFlag)
                    return 5;
                doRet();
                return 11;

            case Z80Instructions.InstructionID.RET_P:
                if (TheRegisters.SignFlag)
                    return 5;
                doRet();
                return 11;

            case Z80Instructions.InstructionID.RET_M:
                if (!TheRegisters.SignFlag)
                    return 5;
                doRet();
                return 11;

            case Z80Instructions.InstructionID.RETI:
                doRet();
                TheRegisters.IFF1 = TheRegisters.IFF2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RETN:
                RETN();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_00:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0000;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_08:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0008;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_10:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0010;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_18:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0018;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_20:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0020;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_28:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0028;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_30:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0030;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_38:
                TheRegisters.SP -= 2;
                MainMemory.Poke(TheRegisters.SP, TheRegisters.PC);
                TheRegisters.PC = 0x0038;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLD:
            {
                int valueAtHL = MainMemory.Peek(TheRegisters.Main.HL);
                var newValueAtHL = ((valueAtHL & 0x0f) << 4) + (TheRegisters.Main.A & 0x0f);
                var newA = (TheRegisters.Main.A & 0xf0) + ((valueAtHL & 0xf0) >> 4);
                TheRegisters.Main.A = (byte)newA;
                MainMemory.Poke(TheRegisters.Main.HL, (byte)newValueAtHL);

                TheRegisters.SignFlag = !Alu.IsBytePositive(TheRegisters.Main.A);
                TheRegisters.ZeroFlag = TheRegisters.Main.A == 0;
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.ParityFlag = Alu.IsEvenParity(TheRegisters.Main.A);
                TheRegisters.SubtractFlag = false;
            }
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRD:
            {
                int valueAtHL = MainMemory.Peek(TheRegisters.Main.HL);
                var newValueAtHL = ((TheRegisters.Main.A & 0x0f) << 4) + ((valueAtHL & 0xf0) >> 4);
                var newA = (TheRegisters.Main.A & 0xf0) + (valueAtHL & 0x0f);
                TheRegisters.Main.A = (byte)newA;
                MainMemory.Poke(TheRegisters.Main.HL, (byte)newValueAtHL);

                TheRegisters.SignFlag = !Alu.IsBytePositive(TheRegisters.Main.A);
                TheRegisters.ZeroFlag = TheRegisters.Main.A == 0;
                TheRegisters.HalfCarryFlag = false;
                TheRegisters.ParityFlag = Alu.IsEvenParity(TheRegisters.Main.A);
                TheRegisters.SubtractFlag = false;
            }
                return instruction.TStateCount;

            case Z80Instructions.InstructionID.OUT_addr_C_0:
                ThePortHandler?.Out(MainMemory.Peek(valueAddress), 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_addr_n_A:
                ThePortHandler?.Out(MainMemory.Peek(valueAddress), TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_A_addr_C:
                ThePortHandler?.Out(TheRegisters.Main.C, TheRegisters.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_B_addr_C:
                ThePortHandler?.Out(TheRegisters.Main.C, TheRegisters.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_C_addr_C:
                ThePortHandler?.Out(TheRegisters.Main.C, TheRegisters.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_D_addr_C:
                ThePortHandler?.Out(TheRegisters.Main.C, TheRegisters.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_E_addr_C:
                ThePortHandler?.Out(TheRegisters.Main.C, TheRegisters.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_H_addr_C:
                ThePortHandler?.Out(TheRegisters.Main.C, TheRegisters.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_L_addr_C:
                ThePortHandler?.Out(TheRegisters.Main.C, TheRegisters.Main.L);
                return instruction.TStateCount;

            case Z80Instructions.InstructionID.IN_A_addr_n:
                if (ThePortHandler != null)
                {
                    var portAddress = (TheRegisters.Main.A << 8) + MainMemory.Peek(valueAddress);
                    TheRegisters.Main.A = ThePortHandler.In((ushort)portAddress);
                }
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_A_addr_C:
                TheRegisters.Main.A = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_B_addr_C:
                TheRegisters.Main.B = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_C_addr_C:
                TheRegisters.Main.C = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_D_addr_C:
                TheRegisters.Main.D = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_E_addr_C:
                TheRegisters.Main.E = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_H_addr_C:
                TheRegisters.Main.H = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_L_addr_C:
                TheRegisters.Main.L = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_addr_C:
                doIN_addrC();
                return instruction.TStateCount;

            case Z80Instructions.InstructionID.ADC_A_IXH:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IXH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_IXL:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IXL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_IYH:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IYH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_IYL:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IYL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IXH:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IXH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IXL:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IXL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IYH:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IYH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IYL:
                TheRegisters.Main.A = TheAlu.AddAndSetFlags(TheRegisters.Main.A, TheRegisters.IYL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IXH:
                TheAlu.And(TheRegisters.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IXL:
                TheAlu.And(TheRegisters.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IYH:
                TheAlu.And(TheRegisters.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IYL:
                TheAlu.And(TheRegisters.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IXH:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IXH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IXL:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IXL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IYH:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IYH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IYL:
                TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IYL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IXL:
                TheRegisters.IXL = TheAlu.DecAndSetFlags(TheRegisters.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IXH:
                TheRegisters.IXH = TheAlu.DecAndSetFlags(TheRegisters.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IYL:
                TheRegisters.IYL = TheAlu.DecAndSetFlags(TheRegisters.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IYH:
                TheRegisters.IYH = TheAlu.DecAndSetFlags(TheRegisters.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IXL:
                TheRegisters.IXL = TheAlu.IncAndSetFlags(TheRegisters.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IYL:
                TheRegisters.IYL = TheAlu.IncAndSetFlags(TheRegisters.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IXH:
                TheRegisters.IXH = TheAlu.IncAndSetFlags(TheRegisters.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IYH:
                TheRegisters.IYH = TheAlu.IncAndSetFlags(TheRegisters.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IXH:
                TheRegisters.Main.A = TheRegisters.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IXL:
                TheRegisters.Main.A = TheRegisters.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IYH:
                TheRegisters.Main.A = TheRegisters.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IYL:
                TheRegisters.Main.A = TheRegisters.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IXH:
                TheRegisters.Main.B = TheRegisters.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IXL:
                TheRegisters.Main.B = TheRegisters.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IYH:
                TheRegisters.Main.B = TheRegisters.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IYL:
                TheRegisters.Main.B = TheRegisters.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IXH:
                TheRegisters.Main.C = TheRegisters.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IXL:
                TheRegisters.Main.C = TheRegisters.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IYH:
                TheRegisters.Main.C = TheRegisters.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IYL:
                TheRegisters.Main.C = TheRegisters.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IXH:
                TheRegisters.Main.D = TheRegisters.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IXL:
                TheRegisters.Main.D = TheRegisters.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IYH:
                TheRegisters.Main.D = TheRegisters.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IYL:
                TheRegisters.Main.D = TheRegisters.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IXH:
                TheRegisters.Main.E = TheRegisters.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IXL:
                TheRegisters.Main.E = TheRegisters.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IYH:
                TheRegisters.Main.E = TheRegisters.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IYL:
                TheRegisters.Main.E = TheRegisters.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_A:
                TheRegisters.IXH = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_B:
                TheRegisters.IXH = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_C:
                TheRegisters.IXH = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_D:
                TheRegisters.IXH = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_E:
                TheRegisters.IXH = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_IXH:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_IXL:
                TheRegisters.IXH = TheRegisters.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_n:
                TheRegisters.IXH = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_A:
                TheRegisters.IXL = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_B:
                TheRegisters.IXL = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_C:
                TheRegisters.IXL = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_D:
                TheRegisters.IXL = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_E:
                TheRegisters.IXL = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_IXH:
                TheRegisters.IXL = TheRegisters.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_IXL:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_n:
                TheRegisters.IXL = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_A:
                TheRegisters.IYH = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_B:
                TheRegisters.IYH = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_C:
                TheRegisters.IYH = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_D:
                TheRegisters.IYH = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_E:
                TheRegisters.IYH = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_IYH:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_IYL:
                TheRegisters.IYH = TheRegisters.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_n:
                TheRegisters.IYH = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_A:
                TheRegisters.IYL = TheRegisters.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_B:
                TheRegisters.IYL = TheRegisters.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_C:
                TheRegisters.IYL = TheRegisters.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_D:
                TheRegisters.IYL = TheRegisters.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_E:
                TheRegisters.IYL = TheRegisters.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_IYH:
                TheRegisters.IYL = TheRegisters.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_IYL:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_n:
                TheRegisters.IYL = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IXH:
                TheAlu.Or(TheRegisters.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IXL:
                TheAlu.Or(TheRegisters.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IYH:
                TheAlu.Or(TheRegisters.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IYL:
                TheAlu.Or(TheRegisters.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IXH:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IXH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IXL:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IXL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IYH:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IYH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IYL:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IYL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IXH:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IXH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IXL:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IXL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IYH:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IYH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IYL:
                TheRegisters.Main.A = TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, TheRegisters.IYL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IXH:
                TheAlu.Xor(TheRegisters.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IXL:
                TheAlu.Xor(TheRegisters.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IYH:
                TheAlu.Xor(TheRegisters.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IYL:
                TheAlu.Xor(TheRegisters.IYL);
                return instruction.TStateCount;

            case Z80Instructions.InstructionID.INI:
            {
                var hlMem = MainMemory.Poke(TheRegisters.Main.HL, ThePortHandler.In(TheRegisters.Main.BC));
                TheRegisters.Main.HL++;
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);

                // 'Undocumented'.
                var incC = (TheRegisters.Main.C + 1) & 255;
                TheRegisters.HalfCarryFlag = hlMem + incC > 255;
                TheRegisters.CarryFlag = hlMem + incC > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + incC) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.INIR:
            {
                // Looping version of INI.
                var hlMem = MainMemory.Poke(TheRegisters.Main.HL, ThePortHandler.In(TheRegisters.Main.BC));
                TheRegisters.Main.HL++;
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);
                if (TheRegisters.Main.B != 0)
                    TheRegisters.PC -= 2; // Repeat.

                // 'Undocumented'.
                var incC = (TheRegisters.Main.C + 1) & 255;
                TheRegisters.HalfCarryFlag = hlMem + incC > 255;
                TheRegisters.CarryFlag = hlMem + incC > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + incC) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;

                return TheRegisters.Main.B == 0 ? 16 : 21;
            }

            case Z80Instructions.InstructionID.IND:
            {
                var hlMem = MainMemory.Poke(TheRegisters.Main.HL, ThePortHandler.In(TheRegisters.Main.BC));
                TheRegisters.Main.HL--;
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);

                // 'Undocumented'.
                var decC = (TheRegisters.Main.C - 1) & 255;
                TheRegisters.HalfCarryFlag = hlMem + decC > 255;
                TheRegisters.CarryFlag = hlMem + decC > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + decC) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;
                
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.INDR:
            {
                // Looping version if IND.
                var hlMem = MainMemory.Poke(TheRegisters.Main.HL, ThePortHandler.In(TheRegisters.Main.BC));
                TheRegisters.Main.HL--;
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);
                if (TheRegisters.Main.B != 0)
                    TheRegisters.PC -= 2; // Repeat.

                // 'Undocumented'.
                var decC = (TheRegisters.Main.C - 1) & 255;
                TheRegisters.HalfCarryFlag = hlMem + decC > 255;
                TheRegisters.CarryFlag = hlMem + decC > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + decC) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;

                return TheRegisters.Main.B == 0 ? 16 : 21;
            }

            case Z80Instructions.InstructionID.OUTI:
            {
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);
                var hlMem = MainMemory.Peek(TheRegisters.Main.HL);
                ThePortHandler.Out(TheRegisters.Main.C, hlMem);
                TheRegisters.Main.HL++;

                // 'Undocumented'.
                TheRegisters.HalfCarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.CarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + TheRegisters.Main.L) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.OTIR:
            {
                // Looping version of OUTI.
                var hlMem = MainMemory.Peek(TheRegisters.Main.HL);
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);
                ThePortHandler.Out(TheRegisters.Main.C, hlMem);
                TheRegisters.Main.HL++;
                if (TheRegisters.Main.B != 0)
                    TheRegisters.PC -= 2; // Repeat.

                // 'Undocumented'.
                TheRegisters.HalfCarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.CarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + TheRegisters.Main.L) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;

                return TheRegisters.Main.B == 0 ? 16 : 21;
            }

            case Z80Instructions.InstructionID.OUTD:
            {
                var hlMem = MainMemory.Peek(TheRegisters.Main.HL);
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);
                ThePortHandler.Out(TheRegisters.Main.C, hlMem);
                TheRegisters.Main.HL--;

                // 'Undocumented'.
                TheRegisters.HalfCarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.CarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + TheRegisters.Main.L) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;
                
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.OTDR:
            {
                // Looping version of OUTD.
                var hlMem = MainMemory.Peek(TheRegisters.Main.HL);
                TheRegisters.Main.B = TheAlu.DecAndSetFlags(TheRegisters.Main.B);
                ThePortHandler.Out(TheRegisters.Main.C, hlMem);
                TheRegisters.Main.HL--;
                if (TheRegisters.Main.B != 0)
                    TheRegisters.PC -= 2; // Repeat.

                // 'Undocumented'.
                TheRegisters.HalfCarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.CarryFlag = hlMem + TheRegisters.Main.L > 255;
                TheRegisters.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + TheRegisters.Main.L) & 7) ^ TheRegisters.Main.B));
                TheRegisters.SubtractFlag = (hlMem & 0x80) != 0;

                return TheRegisters.Main.B == 0 ? 16 : 21;
            }

            default:
                if (instruction.Run == null)
                    throw new UnsupportedInstruction(this, instruction);
                
                // Dynamic instruction.
                return instruction.Run(MainMemory, TheRegisters, TheAlu, valueAddress);
        }
    }
    
    /// <summary>
    /// Each tick increments the R register, but doesn't change the high bit.
    /// </summary>
    private void IncrementR() =>
        TheRegisters.R = (byte)((TheRegisters.R + 1 & 0x7F) | (TheRegisters.R & 0x80));
}
