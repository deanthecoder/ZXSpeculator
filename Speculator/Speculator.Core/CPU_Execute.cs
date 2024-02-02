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
        var regs = TheRegisters;
        var instructionAddress = regs.PC;
        regs.PC += instruction.ByteCount;
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
                regs.Main.A = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_n:
                regs.Main.B = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_n:
                regs.Main.C = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_n:
                regs.Main.D = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_n:
                regs.Main.E = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_n:
                regs.Main.H = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_n:
                regs.Main.L = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addr:
                regs.Main.A = MainMemory.Peek(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_A:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_B:
                regs.Main.A = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_C:
                regs.Main.A = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_D:
                regs.Main.A = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_E:
                regs.Main.A = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_H:
                regs.Main.A = regs.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_L:
                regs.Main.A = regs.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_A:
                regs.Main.B = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_B:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_C:
                regs.Main.B = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_D:
                regs.Main.B = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_E:
                regs.Main.B = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_H:
                regs.Main.B = regs.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_L:
                regs.Main.B = regs.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_A:
                regs.Main.C = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_B:
                regs.Main.C = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_C:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_D:
                regs.Main.C = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_E:
                regs.Main.C = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_H:
                regs.Main.C = regs.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_L:
                regs.Main.C = regs.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_A:
                regs.Main.D = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_B:
                regs.Main.D = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_C:
                regs.Main.D = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_D:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_E:
                regs.Main.D = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_H:
                regs.Main.D = regs.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_L:
                regs.Main.D = regs.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_A:
                regs.Main.E = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_B:
                regs.Main.E = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_C:
                regs.Main.E = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_D:
                regs.Main.E = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_E:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_H:
                regs.Main.E = regs.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_L:
                regs.Main.E = regs.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_A:
                regs.Main.H = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_B:
                regs.Main.H = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_C:
                regs.Main.H = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_D:
                regs.Main.H = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_E:
                regs.Main.H = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_H:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_L:
                regs.Main.H = regs.Main.L;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_A:
                regs.Main.L = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_B:
                regs.Main.L = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_C:
                regs.Main.L = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_D:
                regs.Main.L = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_E:
                regs.Main.L = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_H:
                regs.Main.L = regs.Main.H;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_L:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrHL:
                regs.Main.A = MainMemory.Peek(regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_addrHL:
                regs.Main.B = MainMemory.Peek(regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_addrHL:
                regs.Main.C = MainMemory.Peek(regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_addrHL:
                regs.Main.D = MainMemory.Peek(regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_addrHL:
                regs.Main.E = MainMemory.Peek(regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_addrHL:
                regs.Main.H = MainMemory.Peek(regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_addrHL:
                regs.Main.L = MainMemory.Peek(regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrIXplus_d:
                regs.Main.A =
                    MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrIYplus_d:
                regs.Main.A =
                    MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_addrIXplus_d:
                regs.Main.B =
                    MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_addrIYplus_d:
                regs.Main.B =
                    MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_addrIXplus_d:
                regs.Main.C =
                    MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_addrIYplus_d:
                regs.Main.C =
                    MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_addrIXplus_d:
                regs.Main.D =
                    MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_addrIYplus_d:
                regs.Main.D =
                    MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_addrIXplus_d:
                regs.Main.E =
                    MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_addrIYplus_d:
                regs.Main.E =
                    MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_addrIXplus_d:
                regs.Main.H =
                    MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_H_addrIYplus_d:
                regs.Main.H =
                    MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_addrIXplus_d:
                regs.Main.L =
                    MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_L_addrIYplus_d:
                regs.Main.L =
                    MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IX_nn:
                regs.IX = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IY_nn:
                regs.IY = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IX_addr:
                regs.IX = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IY_addr:
                regs.IY = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_n:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_A:
                MainMemory.Poke(regs.Main.HL, regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_B:
                MainMemory.Poke(regs.Main.HL, regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_C:
                MainMemory.Poke(regs.Main.HL, regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_D:
                MainMemory.Poke(regs.Main.HL, regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_E:
                MainMemory.Poke(regs.Main.HL, regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_H:
                MainMemory.Poke(regs.Main.HL, regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL_L:
                MainMemory.Poke(regs.Main.HL, regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_n:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), MainMemory.Peek((ushort)(valueAddress + 1)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_n:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), MainMemory.Peek((ushort)(valueAddress + 1)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_A:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_B:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_C:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_D:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_E:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_H:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIXplus_d_L:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_A:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_B:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_C:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_D:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_E:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_H:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrIYplus_d_L:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrBC:
                regs.Main.A = MainMemory.Peek(regs.Main.BC);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_addrDE:
                regs.Main.A = MainMemory.Peek(regs.Main.DE);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrBC_A:
                MainMemory.Poke(regs.Main.BC, regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrDE_A:
                MainMemory.Poke(regs.Main.DE, regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_A:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_I:
                regs.Main.A = regs.I;
                regs.SignFlag = !Alu.IsBytePositive(regs.I);
                regs.ZeroFlag = regs.I == 0;
                regs.HalfCarryFlag = false;
                regs.ParityFlag = regs.IFF2;
                regs.SubtractFlag = false;
                regs.SetFlags53FromA();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_R:
                regs.Main.A = regs.R;
                regs.SignFlag = !Alu.IsBytePositive(regs.R);
                regs.ZeroFlag = regs.R == 0;
                regs.HalfCarryFlag = false;
                regs.ParityFlag = regs.IFF2;
                regs.SubtractFlag = false;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_I_A:
                regs.I = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_R_A:
                regs.R = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_BC_nn:
                regs.Main.BC = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_DE_nn:
                regs.Main.DE = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_HL_nn:
                regs.Main.HL = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_nn:
                regs.SP = MainMemory.PeekWord(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_BC_addr:
                regs.Main.BC = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_DE_addr:
                regs.Main.DE = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_HL_addr:
                regs.Main.HL = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_addr:
                regs.SP = MainMemory.PeekWord(MainMemory.PeekWord(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_BC:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), regs.Main.BC);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_DE:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), regs.Main.DE);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addrHL:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_SP:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), regs.SP);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_IX:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), regs.IX);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_addr_IY:
                MainMemory.Poke(MainMemory.PeekWord(valueAddress), regs.IY);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_HL:
                regs.SP = regs.Main.HL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_IX:
                regs.SP = regs.IX;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_SP_IY:
                regs.SP = regs.IY;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_AF:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.Main.AF);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_BC:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.Main.BC);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_DE:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.Main.DE);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_HL:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.Main.HL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_IX:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.IX);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.PUSH_IY:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.IY);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_AF:
                regs.Main.AF = MainMemory.PeekWord(regs.SP);
                regs.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_BC:
                regs.Main.BC = MainMemory.PeekWord(regs.SP);
                regs.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_DE:
                regs.Main.DE = MainMemory.PeekWord(regs.SP);
                regs.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_HL:
                regs.Main.HL = MainMemory.PeekWord(regs.SP);
                regs.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_IX:
                regs.IX = MainMemory.PeekWord(regs.SP);
                regs.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.POP_IY:
                regs.IY = MainMemory.PeekWord(regs.SP);
                regs.SP += 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.EX_DE_HL:
            {
                (regs.Main.DE, regs.Main.HL) = (regs.Main.HL, regs.Main.DE);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EX_AF_altAF:
            {
                (regs.Main.AF, regs.Alt.AF) = (regs.Alt.AF, regs.Main.AF);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EXX:
                regs.ExchangeRegisterSet();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.EX_addrSP_HL:
            {
                var v = regs.Main.HL;
                regs.Main.HL = MainMemory.PeekWord(regs.SP);
                MainMemory.Poke(regs.SP, v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EX_addrSP_IX:
            {
                var v = regs.IX;
                regs.IX = MainMemory.PeekWord(regs.SP);
                MainMemory.Poke(regs.SP, v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.EX_addrSP_IY:
            {
                var v = regs.IY;
                regs.IY = MainMemory.PeekWord(regs.SP);
                MainMemory.Poke(regs.SP, v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.LDD:
                doLDD();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LDDR:
            {
                doLDD();
                if (regs.Main.BC != 0)
                    regs.PC -= 2;
                return regs.Main.BC != 0 ? 21 : 16;
            }
            case Z80Instructions.InstructionID.LDI:
                doLDI();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LDIR:
            {
                doLDI();
                if (regs.Main.BC != 0)
                    regs.PC -= 2;
                return regs.Main.BC != 0 ? 21 : 16;
            }
            case Z80Instructions.InstructionID.CPD:
                doCPD();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CPDR:
            {
                doCPD();
                if (!regs.ZeroFlag && regs.Main.BC != 0)
                    regs.PC -= 2;
                regs.ParityFlag = regs.Main.BC != 0;
                return regs.Main.BC != 0 ? 21 : 16;
            }
            case Z80Instructions.InstructionID.CPI:
                doCPI();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CPIR:
            {
                doCPI();
                if (!regs.ZeroFlag && regs.Main.BC != 0)
                    regs.PC -= 2;
                regs.ParityFlag = regs.Main.BC != 0;
                return regs.Main.BC != 0 && regs.Main.A != MainMemory.Peek(regs.Main.HL) ? 21 : 16;
            }
            case Z80Instructions.InstructionID.ADC_A_n:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_A:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.A, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_B:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.B, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_C:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.C, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_D:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.D, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_E:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.E, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_H:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.H, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_L:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.L, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_addrHL:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(regs.Main.HL), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_addrIXplus_d:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_addrIYplus_d:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_n:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(valueAddress), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_A:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.A, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_B:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.B, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_C:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.C, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_D:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.D, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_E:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.E, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_H:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.H, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_L:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.Main.L, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_addrHL:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(regs.Main.HL), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_addrIXplus_d:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_addrIYplus_d:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_A:
                regs.Main.A = TheAlu.DecAndSetFlags(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_B:
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_C:
                regs.Main.C = TheAlu.DecAndSetFlags(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_D:
                regs.Main.D = TheAlu.DecAndSetFlags(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_E:
                regs.Main.E = TheAlu.DecAndSetFlags(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_H:
                regs.Main.H = TheAlu.DecAndSetFlags(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_L:
                regs.Main.L = TheAlu.DecAndSetFlags(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.DecAndSetFlags(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_addrIXplus_d:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), TheAlu.DecAndSetFlags(MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_addrIYplus_d:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), TheAlu.DecAndSetFlags(MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_A:
                regs.Main.A = TheAlu.IncAndSetFlags(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_B:
                regs.Main.B = TheAlu.IncAndSetFlags(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_C:
                regs.Main.C = TheAlu.IncAndSetFlags(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_D:
                regs.Main.D = TheAlu.IncAndSetFlags(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_E:
                regs.Main.E = TheAlu.IncAndSetFlags(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_H:
                regs.Main.H = TheAlu.IncAndSetFlags(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_L:
                regs.Main.L = TheAlu.IncAndSetFlags(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.IncAndSetFlags(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_addrIXplus_d:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), TheAlu.IncAndSetFlags(MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_addrIYplus_d:
                MainMemory.Poke(regs.IYPlusD(MainMemory.Peek(valueAddress)), TheAlu.IncAndSetFlags(MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress)))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_n:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_A:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.A, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_B:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.B, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_C:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.C, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_D:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.D, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_E:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.E, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_H:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.H, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_L:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.L, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_addrHL:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.Main.HL), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_addrIXplus_d:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_addrIYplus_d:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_n:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(valueAddress), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_A:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.A, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_B:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.B, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_C:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.C, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_D:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.D, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_E:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.E, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_H:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.H, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_L:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.L, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_addrHL:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.Main.HL), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_addrIXplus_d:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_addrIYplus_d:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))), false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_BC:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.Main.BC, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_DE:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.Main.DE, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_HL:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.Main.HL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_HL_SP:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.SP, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_BC:
                regs.Main.HL = TheAlu.SubtractAndSetFlags(regs.Main.HL, regs.Main.BC, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_DE:
                regs.Main.HL = TheAlu.SubtractAndSetFlags(regs.Main.HL, regs.Main.DE, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_HL:
                regs.Main.HL = TheAlu.SubtractAndSetFlags(regs.Main.HL, regs.Main.HL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_HL_SP:
                regs.Main.HL = TheAlu.SubtractAndSetFlags(regs.Main.HL, regs.SP, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_BC:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.Main.BC, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_DE:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.Main.DE, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_HL:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.Main.HL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_HL_SP:
                regs.Main.HL = TheAlu.AddAndSetFlags(regs.Main.HL, regs.SP, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_BC:
                regs.IX = TheAlu.AddAndSetFlags(regs.IX, regs.Main.BC, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_DE:
                regs.IX = TheAlu.AddAndSetFlags(regs.IX, regs.Main.DE, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_IX:
                regs.IX = TheAlu.AddAndSetFlags(regs.IX, regs.IX, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IX_SP:
                regs.IX = TheAlu.AddAndSetFlags(regs.IX, regs.SP, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_BC:
                regs.IY = TheAlu.AddAndSetFlags(regs.IY, regs.Main.BC, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_DE:
                regs.IY = TheAlu.AddAndSetFlags(regs.IY, regs.Main.DE, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_IY:
                regs.IY = TheAlu.AddAndSetFlags(regs.IY, regs.IY, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_IY_SP:
                regs.IY = TheAlu.AddAndSetFlags(regs.IY, regs.SP, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_BC:
                regs.Main.BC = (ushort)(regs.Main.BC - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_DE:
                regs.Main.DE = (ushort)(regs.Main.DE - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_HL:
                regs.Main.HL = (ushort)(regs.Main.HL - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_SP:
                regs.SP = (ushort)(regs.SP - 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IX:
                regs.IX--;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IY:
                regs.IY--;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_BC:
                regs.Main.BC++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_DE:
                regs.Main.DE++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_HL:
                regs.Main.HL++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_SP:
                regs.SP++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IX:
                regs.IX++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IY:
                regs.IY++;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_n:
                TheAlu.And(MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_A:
                TheAlu.And(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_B:
                TheAlu.And(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_C:
                TheAlu.And(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_D:
                TheAlu.And(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_E:
                TheAlu.And(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_H:
                TheAlu.And(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_L:
                TheAlu.And(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_addrHL:
                TheAlu.And(MainMemory.Peek(regs.Main.HL));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_addrIX_plus_d:
                TheAlu.And(MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_addrIY_plus_d:
                TheAlu.And(MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_n:
            {
                var b = MainMemory.Peek(valueAddress);
                TheAlu.SubtractAndSetFlags(regs.Main.A, b, false);
                regs.SetFlags53From(b);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.CP_A:
            {
                var a = regs.Main.A;
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.A, false);
                regs.SetFlags53From(a);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.CP_B:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.B, false);
                regs.SetFlags53From(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_C:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.C, false);
                regs.SetFlags53From(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_D:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.D, false);
                regs.SetFlags53From(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_E:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.E, false);
                regs.SetFlags53From(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_H:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.H, false);
                regs.SetFlags53From(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_L:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.Main.L, false);
                regs.SetFlags53From(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_addrHL:
            {
                var b = MainMemory.Peek(regs.Main.HL);
                TheAlu.SubtractAndSetFlags(regs.Main.A, b, false);
                regs.SetFlags53From(b);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.CP_addrIX_plus_d:
                TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))), false);
                regs.SetFlags53From(MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_addrIY_plus_d:
                TheAlu.SubtractAndSetFlags(regs.Main.A, MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))), false);
                regs.SetFlags53From(MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_n:
                TheAlu.Or(MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_A:
                TheAlu.Or(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_B:
                TheAlu.Or(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_C:
                TheAlu.Or(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_D:
                TheAlu.Or(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_E:
                TheAlu.Or(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_H:
                TheAlu.Or(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_L:
                TheAlu.Or(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_addrHL:
                TheAlu.Or(MainMemory.Peek(regs.Main.HL));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_addrIX_plus_d:
                TheAlu.Or(MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_addrIY_plus_d:
                TheAlu.Or(MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_n:
                TheAlu.Xor(MainMemory.Peek(valueAddress));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_A:
                TheAlu.Xor(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_B:
                TheAlu.Xor(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_C:
                TheAlu.Xor(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_D:
                TheAlu.Xor(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_E:
                TheAlu.Xor(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_H:
                TheAlu.Xor(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_L:
                TheAlu.Xor(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_addrHL:
                TheAlu.Xor(MainMemory.Peek(regs.Main.HL));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_addrIX_plus_d:
                TheAlu.Xor(MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_addrIY_plus_d:
                TheAlu.Xor(MainMemory.Peek(regs.IYPlusD(MainMemory.Peek(valueAddress))));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CCF:
                regs.HalfCarryFlag = regs.CarryFlag;
                regs.CarryFlag = !regs.CarryFlag;
                regs.SubtractFlag = false;
                regs.SetFlags53From(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CPL:
                regs.HalfCarryFlag = true;
                regs.SubtractFlag = true;
                regs.Main.A = (byte)~regs.Main.A;
                regs.SetFlags53From(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DAA:
                TheAlu.AdjustAccumulatorToBcd();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.EI:
                regs.IFF1 = regs.IFF2 = true;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DI:
                regs.IFF1 = regs.IFF2 = false;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.HALT:
                IsHalted = true;
                regs.PC--;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IM0:
                regs.IM = 0;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IM1:
                regs.IM = 1;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IM2:
                regs.IM = 2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.NEG:
            {
                var newA = TheAlu.SubtractAndSetFlags((byte)0x00, regs.Main.A, false);
                regs.ParityFlag = regs.Main.A == 0x80;
                regs.CarryFlag = regs.Main.A != 0x00;
                regs.Main.A = newA;
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.SCF:
                regs.HalfCarryFlag = false;
                regs.CarryFlag = true;
                regs.SubtractFlag = false;
                regs.SetFlags53FromA();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_A:
                regs.Main.A = TheAlu.RotateLeft(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_B:
                regs.Main.B = TheAlu.RotateLeft(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_C:
                regs.Main.C = TheAlu.RotateLeft(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_D:
                regs.Main.D = TheAlu.RotateLeft(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_E:
                regs.Main.E = TheAlu.RotateLeft(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_H:
                regs.Main.H = TheAlu.RotateLeft(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_L:
                regs.Main.L = TheAlu.RotateLeft(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RL_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.RotateLeft(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLA:
            {
                var b = regs.Main.A;
                var oldHighBit = (byte)(b & 0x80);
                var oldCarry = (byte)(TheAlu.TheRegisters.CarryFlag ? 1 : 0);
                b <<= 1;
                b |= oldCarry;
                TheAlu.TheRegisters.HalfCarryFlag = false;
                TheAlu.TheRegisters.SubtractFlag = false;
                TheAlu.TheRegisters.CarryFlag = oldHighBit != 0x00;
                regs.Main.A = b;
                regs.SetFlags53From(b);

                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.RLC_A:
                regs.Main.A = TheAlu.RotateLeftCircular(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_B:
                regs.Main.B = TheAlu.RotateLeftCircular(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_C:
                regs.Main.C = TheAlu.RotateLeftCircular(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_D:
                regs.Main.D = TheAlu.RotateLeftCircular(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_E:
                regs.Main.E = TheAlu.RotateLeftCircular(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_H:
                regs.Main.H = TheAlu.RotateLeftCircular(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_L:
                regs.Main.L = TheAlu.RotateLeftCircular(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLCA:
                regs.CarryFlag = (regs.Main.A & 0x80) != 0x00;
                regs.Main.A = (byte)((regs.Main.A << 1) + (regs.Main.A >> 7));
                regs.HalfCarryFlag = false;
                regs.SubtractFlag = false;
                regs.SetFlags53From(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLC_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.RotateLeftCircular(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_A:
                regs.Main.A = TheAlu.RotateRight(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_B:
                regs.Main.B = TheAlu.RotateRight(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_C:
                regs.Main.C = TheAlu.RotateRight(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_D:
                regs.Main.D = TheAlu.RotateRight(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_E:
                regs.Main.E = TheAlu.RotateRight(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_H:
                regs.Main.H = TheAlu.RotateRight(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_L:
                regs.Main.L = TheAlu.RotateRight(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RR_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.RotateRight(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRA:
            {
                var oldCarry = (byte)(regs.CarryFlag ? 1 : 0);
                regs.CarryFlag = (regs.Main.A & 0x01) != 0x00;
                regs.Main.A = (byte)((regs.Main.A >> 1) + (oldCarry << 7));
                regs.HalfCarryFlag = false;
                regs.SubtractFlag = false;
                regs.SetFlags53From(regs.Main.A);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.RRC_A:
                regs.Main.A = TheAlu.RotateRightCircular(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_B:
                regs.Main.B = TheAlu.RotateRightCircular(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_C:
                regs.Main.C = TheAlu.RotateRightCircular(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_D:
                regs.Main.D = TheAlu.RotateRightCircular(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_E:
                regs.Main.E = TheAlu.RotateRightCircular(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_H:
                regs.Main.H = TheAlu.RotateRightCircular(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_L:
                regs.Main.L = TheAlu.RotateRightCircular(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRC_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.RotateRightCircular(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RRCA:
            {
                regs.CarryFlag = (regs.Main.A & 0x01) != 0x00;
                var v = (byte)((regs.Main.A >> 1) + ((regs.Main.A & 0x01) << 7));
                regs.Main.A = v;
                regs.HalfCarryFlag = false;
                regs.SubtractFlag = false;
                regs.SetFlags53From(v);
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.SLA_A:
                regs.Main.A = TheAlu.ShiftLeft(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_B:
                regs.Main.B = TheAlu.ShiftLeft(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_C:
                regs.Main.C = TheAlu.ShiftLeft(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_D:
                regs.Main.D = TheAlu.ShiftLeft(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_E:
                regs.Main.E = TheAlu.ShiftLeft(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_H:
                regs.Main.H = TheAlu.ShiftLeft(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_L:
                regs.Main.L = TheAlu.ShiftLeft(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLA_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.ShiftLeft(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_A:
                regs.Main.A = TheAlu.ShiftLeft(regs.Main.A, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_B:
                regs.Main.B = TheAlu.ShiftLeft(regs.Main.B, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_C:
                regs.Main.C = TheAlu.ShiftLeft(regs.Main.C, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_D:
                regs.Main.D = TheAlu.ShiftLeft(regs.Main.D, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_E:
                regs.Main.E = TheAlu.ShiftLeft(regs.Main.E, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_H:
                regs.Main.H = TheAlu.ShiftLeft(regs.Main.H, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_L:
                regs.Main.L = TheAlu.ShiftLeft(regs.Main.L, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SLL_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.ShiftLeft(MainMemory.Peek(regs.Main.HL), 1));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_A:
                regs.Main.A = TheAlu.ShiftRightArithmetic(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_B:
                regs.Main.B = TheAlu.ShiftRightArithmetic(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_C:
                regs.Main.C = TheAlu.ShiftRightArithmetic(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_D:
                regs.Main.D = TheAlu.ShiftRightArithmetic(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_E:
                regs.Main.E = TheAlu.ShiftRightArithmetic(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_H:
                regs.Main.H = TheAlu.ShiftRightArithmetic(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_L:
                regs.Main.L = TheAlu.ShiftRightArithmetic(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRA_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.ShiftRightArithmetic(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_A:
                regs.Main.A = TheAlu.ShiftRightLogical(regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_B:
                regs.Main.B = TheAlu.ShiftRightLogical(regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_C:
                regs.Main.C = TheAlu.ShiftRightLogical(regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_D:
                regs.Main.D = TheAlu.ShiftRightLogical(regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_E:
                regs.Main.E = TheAlu.ShiftRightLogical(regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_H:
                regs.Main.H = TheAlu.ShiftRightLogical(regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_L:
                regs.Main.L = TheAlu.ShiftRightLogical(regs.Main.L);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SRL_addrHL:
                MainMemory.Poke(regs.Main.HL, TheAlu.ShiftRightLogical(MainMemory.Peek(regs.Main.HL)));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_A:
                doBitTest(regs.Main.A, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_A:
                doBitTest(regs.Main.A, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_A:
                doBitTest(regs.Main.A, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_A:
                doBitTest(regs.Main.A, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_A:
                doBitTest(regs.Main.A, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_A:
                doBitTest(regs.Main.A, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_A:
                doBitTest(regs.Main.A, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_A:
                doBitTest(regs.Main.A, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_B:
                doBitTest(regs.Main.B, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_B:
                doBitTest(regs.Main.B, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_B:
                doBitTest(regs.Main.B, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_B:
                doBitTest(regs.Main.B, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_B:
                doBitTest(regs.Main.B, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_B:
                doBitTest(regs.Main.B, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_B:
                doBitTest(regs.Main.B, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_B:
                doBitTest(regs.Main.B, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_C:
                doBitTest(regs.Main.C, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_C:
                doBitTest(regs.Main.C, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_C:
                doBitTest(regs.Main.C, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_C:
                doBitTest(regs.Main.C, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_C:
                doBitTest(regs.Main.C, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_C:
                doBitTest(regs.Main.C, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_C:
                doBitTest(regs.Main.C, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_C:
                doBitTest(regs.Main.C, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_D:
                doBitTest(regs.Main.D, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_D:
                doBitTest(regs.Main.D, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_D:
                doBitTest(regs.Main.D, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_D:
                doBitTest(regs.Main.D, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_D:
                doBitTest(regs.Main.D, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_D:
                doBitTest(regs.Main.D, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_D:
                doBitTest(regs.Main.D, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_D:
                doBitTest(regs.Main.D, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_E:
                doBitTest(regs.Main.E, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_E:
                doBitTest(regs.Main.E, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_E:
                doBitTest(regs.Main.E, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_E:
                doBitTest(regs.Main.E, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_E:
                doBitTest(regs.Main.E, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_E:
                doBitTest(regs.Main.E, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_E:
                doBitTest(regs.Main.E, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_E:
                doBitTest(regs.Main.E, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_H:
                doBitTest(regs.Main.H, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_H:
                doBitTest(regs.Main.H, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_H:
                doBitTest(regs.Main.H, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_H:
                doBitTest(regs.Main.H, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_H:
                doBitTest(regs.Main.H, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_H:
                doBitTest(regs.Main.H, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_H:
                doBitTest(regs.Main.H, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_H:
                doBitTest(regs.Main.H, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_L:
                doBitTest(regs.Main.L, 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_L:
                doBitTest(regs.Main.L, 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_L:
                doBitTest(regs.Main.L, 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_L:
                doBitTest(regs.Main.L, 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_L:
                doBitTest(regs.Main.L, 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_L:
                doBitTest(regs.Main.L, 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_L:
                doBitTest(regs.Main.L, 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_L:
                doBitTest(regs.Main.L, 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_0_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_1_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_2_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_3_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_4_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_5_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_6_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.BIT_7_addrHL:
                doBitTest(MainMemory.Peek(regs.Main.HL), 7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_A:
                regs.Main.A = regs.Main.A.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_A:
                regs.Main.A = regs.Main.A.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_A:
                regs.Main.A = regs.Main.A.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_A:
                regs.Main.A = regs.Main.A.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_A:
                regs.Main.A = regs.Main.A.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_A:
                regs.Main.A = regs.Main.A.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_A:
                regs.Main.A = regs.Main.A.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_A:
                regs.Main.A = regs.Main.A.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_B:
                regs.Main.B = regs.Main.B.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_B:
                regs.Main.B = regs.Main.B.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_B:
                regs.Main.B = regs.Main.B.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_B:
                regs.Main.B = regs.Main.B.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_B:
                regs.Main.B = regs.Main.B.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_B:
                regs.Main.B = regs.Main.B.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_B:
                regs.Main.B = regs.Main.B.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_B:
                regs.Main.B = regs.Main.B.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_C:
                regs.Main.C = regs.Main.C.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_C:
                regs.Main.C = regs.Main.C.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_C:
                regs.Main.C = regs.Main.C.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_C:
                regs.Main.C = regs.Main.C.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_C:
                regs.Main.C = regs.Main.C.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_C:
                regs.Main.C = regs.Main.C.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_C:
                regs.Main.C = regs.Main.C.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_C:
                regs.Main.C = regs.Main.C.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_D:
                regs.Main.D = regs.Main.D.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_D:
                regs.Main.D = regs.Main.D.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_D:
                regs.Main.D = regs.Main.D.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_D:
                regs.Main.D = regs.Main.D.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_D:
                regs.Main.D = regs.Main.D.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_D:
                regs.Main.D = regs.Main.D.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_D:
                regs.Main.D = regs.Main.D.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_D:
                regs.Main.D = regs.Main.D.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_E:
                regs.Main.E = regs.Main.E.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_E:
                regs.Main.E = regs.Main.E.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_E:
                regs.Main.E = regs.Main.E.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_E:
                regs.Main.E = regs.Main.E.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_E:
                regs.Main.E = regs.Main.E.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_E:
                regs.Main.E = regs.Main.E.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_E:
                regs.Main.E = regs.Main.E.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_E:
                regs.Main.E = regs.Main.E.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_H:
                regs.Main.H = regs.Main.H.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_H:
                regs.Main.H = regs.Main.H.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_H:
                regs.Main.H = regs.Main.H.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_H:
                regs.Main.H = regs.Main.H.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_H:
                regs.Main.H = regs.Main.H.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_H:
                regs.Main.H = regs.Main.H.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_H:
                regs.Main.H = regs.Main.H.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_H:
                regs.Main.H = regs.Main.H.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_L:
                regs.Main.L = regs.Main.L.ResetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_L:
                regs.Main.L = regs.Main.L.ResetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_L:
                regs.Main.L = regs.Main.L.ResetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_L:
                regs.Main.L = regs.Main.L.ResetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_L:
                regs.Main.L = regs.Main.L.ResetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_L:
                regs.Main.L = regs.Main.L.ResetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_L:
                regs.Main.L = regs.Main.L.ResetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_L:
                regs.Main.L = regs.Main.L.ResetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_0_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(0));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_1_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(1));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_2_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(2));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_3_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(3));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_4_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(4));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_5_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(5));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_6_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(6));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_7_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).ResetBit(7));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RES_n_addrIX_plus_d:
                MainMemory.Poke(regs.IXPlusD(MainMemory.Peek(valueAddress)), MainMemory.Peek(regs.IXPlusD(MainMemory.Peek(valueAddress))).ResetBit(0));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_A:
                regs.Main.A = regs.Main.A.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_A:
                regs.Main.A = regs.Main.A.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_A:
                regs.Main.A = regs.Main.A.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_A:
                regs.Main.A = regs.Main.A.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_A:
                regs.Main.A = regs.Main.A.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_A:
                regs.Main.A = regs.Main.A.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_A:
                regs.Main.A = regs.Main.A.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_A:
                regs.Main.A = regs.Main.A.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_B:
                regs.Main.B = regs.Main.B.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_B:
                regs.Main.B = regs.Main.B.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_B:
                regs.Main.B = regs.Main.B.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_B:
                regs.Main.B = regs.Main.B.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_B:
                regs.Main.B = regs.Main.B.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_B:
                regs.Main.B = regs.Main.B.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_B:
                regs.Main.B = regs.Main.B.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_B:
                regs.Main.B = regs.Main.B.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_C:
                regs.Main.C = regs.Main.C.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_C:
                regs.Main.C = regs.Main.C.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_C:
                regs.Main.C = regs.Main.C.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_C:
                regs.Main.C = regs.Main.C.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_C:
                regs.Main.C = regs.Main.C.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_C:
                regs.Main.C = regs.Main.C.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_C:
                regs.Main.C = regs.Main.C.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_C:
                regs.Main.C = regs.Main.C.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_D:
                regs.Main.D = regs.Main.D.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_D:
                regs.Main.D = regs.Main.D.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_D:
                regs.Main.D = regs.Main.D.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_D:
                regs.Main.D = regs.Main.D.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_D:
                regs.Main.D = regs.Main.D.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_D:
                regs.Main.D = regs.Main.D.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_D:
                regs.Main.D = regs.Main.D.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_D:
                regs.Main.D = regs.Main.D.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_E:
                regs.Main.E = regs.Main.E.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_E:
                regs.Main.E = regs.Main.E.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_E:
                regs.Main.E = regs.Main.E.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_E:
                regs.Main.E = regs.Main.E.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_E:
                regs.Main.E = regs.Main.E.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_E:
                regs.Main.E = regs.Main.E.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_E:
                regs.Main.E = regs.Main.E.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_E:
                regs.Main.E = regs.Main.E.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_H:
                regs.Main.H = regs.Main.H.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_H:
                regs.Main.H = regs.Main.H.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_H:
                regs.Main.H = regs.Main.H.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_H:
                regs.Main.H = regs.Main.H.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_H:
                regs.Main.H = regs.Main.H.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_H:
                regs.Main.H = regs.Main.H.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_H:
                regs.Main.H = regs.Main.H.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_H:
                regs.Main.H = regs.Main.H.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_L:
                regs.Main.L = regs.Main.L.SetBit(0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_L:
                regs.Main.L = regs.Main.L.SetBit(1);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_L:
                regs.Main.L = regs.Main.L.SetBit(2);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_L:
                regs.Main.L = regs.Main.L.SetBit(3);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_L:
                regs.Main.L = regs.Main.L.SetBit(4);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_L:
                regs.Main.L = regs.Main.L.SetBit(5);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_L:
                regs.Main.L = regs.Main.L.SetBit(6);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_L:
                regs.Main.L = regs.Main.L.SetBit(7);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_0_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(0));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_1_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(1));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_2_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(2));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_3_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(3));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_4_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(4));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_5_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(5));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_6_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(6));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SET_7_addrHL:
                MainMemory.Poke(regs.Main.HL, MainMemory.Peek(regs.Main.HL).SetBit(7));
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CALL_nn:
                CallIfTrue(MainMemory.PeekWord(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CALL_NZ_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !regs.ZeroFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_Z_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), regs.ZeroFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_NC_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !regs.CarryFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_C_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), regs.CarryFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_PO_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !regs.ParityFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_PE_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), regs.ParityFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_P_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), !regs.SignFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_M_nn:
                return CallIfTrue(MainMemory.PeekWord(valueAddress), regs.SignFlag) ? 17 : 10;
            case Z80Instructions.InstructionID.CALL_DJNZ_n:
                regs.Main.B--;
                if (regs.Main.B == 0)
                    return 8;
                regs.PC = (ushort)(regs.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress)));
                return 13;
            case Z80Instructions.InstructionID.JP_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_NZ_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !regs.ZeroFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_Z_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), regs.ZeroFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_NC_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !regs.CarryFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_C_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), regs.CarryFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_PO_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !regs.ParityFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_PE_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), regs.ParityFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_P_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), !regs.SignFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_M_nn:
                JumpIfTrue(MainMemory.PeekWord(valueAddress), regs.SignFlag);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_addrHL:
                JumpIfTrue(regs.Main.HL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_addr_IX:
                JumpIfTrue(regs.IX, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JP_addr_IY:
                JumpIfTrue(regs.IY, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JR_n:
                JumpIfTrue((ushort)(regs.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.JR_NZ_n:
                return JumpIfTrue((ushort)(regs.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), !regs.ZeroFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.JR_Z_n:
                return JumpIfTrue((ushort)(regs.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), regs.ZeroFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.JR_NC_n:
                return JumpIfTrue((ushort)(regs.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), !regs.CarryFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.JR_C_n:
                return JumpIfTrue((ushort)(regs.PC + Alu.FromTwosCompliment(MainMemory.Peek(valueAddress))), regs.CarryFlag) ? 12 : 7;
            case Z80Instructions.InstructionID.RET:
                doRet();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RET_NZ:
                if (!regs.ZeroFlag)
                {
                    doRet();
                    return 11;
                }

                return 5;
            case Z80Instructions.InstructionID.RET_Z:
                if (!regs.ZeroFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_NC:
                if (regs.CarryFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_C:
                if (!regs.CarryFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_PO:
                if (regs.ParityFlag)
                    return 5;
                doRet();
                return 11;
            case Z80Instructions.InstructionID.RET_PE:
                if (!regs.ParityFlag)
                    return 5;
                doRet();
                return 11;

            case Z80Instructions.InstructionID.RET_P:
                if (regs.SignFlag)
                    return 5;
                doRet();
                return 11;

            case Z80Instructions.InstructionID.RET_M:
                if (!regs.SignFlag)
                    return 5;
                doRet();
                return 11;

            case Z80Instructions.InstructionID.RETI:
                doRet();
                regs.IFF1 = regs.IFF2;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RETN:
                RETN();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_00:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0000;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_08:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0008;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_10:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0010;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_18:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0018;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_20:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0020;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_28:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0028;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_30:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0030;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RST_38:
                regs.SP -= 2;
                MainMemory.Poke(regs.SP, regs.PC);
                regs.PC = 0x0038;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.RLD:
            {
                int valueAtHl = MainMemory.Peek(regs.Main.HL);
                var newValueAtHl = ((valueAtHl & 0x0f) << 4) + (regs.Main.A & 0x0f);
                var newA = (regs.Main.A & 0xf0) + ((valueAtHl & 0xf0) >> 4);
                regs.Main.A = (byte)newA;
                MainMemory.Poke(regs.Main.HL, (byte)newValueAtHl);

                regs.SignFlag = !Alu.IsBytePositive(regs.Main.A);
                regs.ZeroFlag = regs.Main.A == 0;
                regs.HalfCarryFlag = false;
                regs.ParityFlag = Alu.IsEvenParity(regs.Main.A);
                regs.SubtractFlag = false;
                regs.SetFlags53FromA();
                return instruction.TStateCount;
            }
            case Z80Instructions.InstructionID.RRD:
            {
                int valueAtHl = MainMemory.Peek(regs.Main.HL);
                var newValueAtHl = ((regs.Main.A & 0x0f) << 4) + ((valueAtHl & 0xf0) >> 4);
                var newA = (regs.Main.A & 0xf0) + (valueAtHl & 0x0f);
                regs.Main.A = (byte)newA;
                MainMemory.Poke(regs.Main.HL, (byte)newValueAtHl);

                regs.SignFlag = !Alu.IsBytePositive(regs.Main.A);
                regs.ZeroFlag = regs.Main.A == 0;
                regs.HalfCarryFlag = false;
                regs.ParityFlag = Alu.IsEvenParity(regs.Main.A);
                regs.SubtractFlag = false;
                regs.SetFlags53FromA();
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.OUT_addr_C_0:
                ThePortHandler?.Out(MainMemory.Peek(valueAddress), 0);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_addr_n_A:
                ThePortHandler?.Out(MainMemory.Peek(valueAddress), regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_A_addr_C:
                ThePortHandler?.Out(regs.Main.C, regs.Main.A);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_B_addr_C:
                ThePortHandler?.Out(regs.Main.C, regs.Main.B);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_C_addr_C:
                ThePortHandler?.Out(regs.Main.C, regs.Main.C);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_D_addr_C:
                ThePortHandler?.Out(regs.Main.C, regs.Main.D);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_E_addr_C:
                ThePortHandler?.Out(regs.Main.C, regs.Main.E);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_H_addr_C:
                ThePortHandler?.Out(regs.Main.C, regs.Main.H);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OUT_L_addr_C:
                ThePortHandler?.Out(regs.Main.C, regs.Main.L);
                return instruction.TStateCount;

            case Z80Instructions.InstructionID.IN_A_addr_n:
                if (ThePortHandler != null)
                {
                    var portAddress = (regs.Main.A << 8) + MainMemory.Peek(valueAddress);
                    regs.Main.A = ThePortHandler.In((ushort)portAddress);
                }
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_A_addr_C:
                regs.Main.A = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_B_addr_C:
                regs.Main.B = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_C_addr_C:
                regs.Main.C = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_D_addr_C:
                regs.Main.D = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_E_addr_C:
                regs.Main.E = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_H_addr_C:
                regs.Main.H = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_L_addr_C:
                regs.Main.L = doIN_addrC();
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.IN_addr_C:
                doIN_addrC();
                return instruction.TStateCount;

            case Z80Instructions.InstructionID.ADC_A_IXH:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IXH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_IXL:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IXL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_IYH:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IYH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADC_A_IYL:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IYL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IXH:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IXH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IXL:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IXL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IYH:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IYH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.ADD_A_IYL:
                regs.Main.A = TheAlu.AddAndSetFlags(regs.Main.A, regs.IYL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IXH:
                TheAlu.And(regs.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IXL:
                TheAlu.And(regs.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IYH:
                TheAlu.And(regs.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.AND_IYL:
                TheAlu.And(regs.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IXH:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IXH, false);
                regs.SetFlags53From(regs.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IXL:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IXL, false);
                regs.SetFlags53From(regs.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IYH:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IYH, false);
                regs.SetFlags53From(regs.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.CP_IYL:
                TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IYL, false);
                regs.SetFlags53From(regs.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IXL:
                regs.IXL = TheAlu.DecAndSetFlags(regs.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IXH:
                regs.IXH = TheAlu.DecAndSetFlags(regs.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IYL:
                regs.IYL = TheAlu.DecAndSetFlags(regs.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.DEC_IYH:
                regs.IYH = TheAlu.DecAndSetFlags(regs.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IXL:
                regs.IXL = TheAlu.IncAndSetFlags(regs.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IYL:
                regs.IYL = TheAlu.IncAndSetFlags(regs.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IXH:
                regs.IXH = TheAlu.IncAndSetFlags(regs.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.INC_IYH:
                regs.IYH = TheAlu.IncAndSetFlags(regs.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IXH:
                regs.Main.A = regs.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IXL:
                regs.Main.A = regs.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IYH:
                regs.Main.A = regs.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_A_IYL:
                regs.Main.A = regs.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IXH:
                regs.Main.B = regs.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IXL:
                regs.Main.B = regs.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IYH:
                regs.Main.B = regs.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_B_IYL:
                regs.Main.B = regs.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IXH:
                regs.Main.C = regs.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IXL:
                regs.Main.C = regs.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IYH:
                regs.Main.C = regs.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_C_IYL:
                regs.Main.C = regs.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IXH:
                regs.Main.D = regs.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IXL:
                regs.Main.D = regs.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IYH:
                regs.Main.D = regs.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_D_IYL:
                regs.Main.D = regs.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IXH:
                regs.Main.E = regs.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IXL:
                regs.Main.E = regs.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IYH:
                regs.Main.E = regs.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_E_IYL:
                regs.Main.E = regs.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_A:
                regs.IXH = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_B:
                regs.IXH = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_C:
                regs.IXH = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_D:
                regs.IXH = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_E:
                regs.IXH = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_IXH:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_IXL:
                regs.IXH = regs.IXL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXH_n:
                regs.IXH = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_A:
                regs.IXL = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_B:
                regs.IXL = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_C:
                regs.IXL = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_D:
                regs.IXL = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_E:
                regs.IXL = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_IXH:
                regs.IXL = regs.IXH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_IXL:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IXL_n:
                regs.IXL = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_A:
                regs.IYH = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_B:
                regs.IYH = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_C:
                regs.IYH = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_D:
                regs.IYH = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_E:
                regs.IYH = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_IYH:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_IYL:
                regs.IYH = regs.IYL;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYH_n:
                regs.IYH = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_A:
                regs.IYL = regs.Main.A;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_B:
                regs.IYL = regs.Main.B;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_C:
                regs.IYL = regs.Main.C;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_D:
                regs.IYL = regs.Main.D;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_E:
                regs.IYL = regs.Main.E;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_IYH:
                regs.IYL = regs.IYH;
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_IYL:
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.LD_IYL_n:
                regs.IYL = MainMemory.Peek(valueAddress);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IXH:
                TheAlu.Or(regs.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IXL:
                TheAlu.Or(regs.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IYH:
                TheAlu.Or(regs.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.OR_IYL:
                TheAlu.Or(regs.IYL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IXH:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IXH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IXL:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IXL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IYH:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IYH, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SBC_A_IYL:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IYL, true);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IXH:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IXH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IXL:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IXL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IYH:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IYH, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.SUB_IYL:
                regs.Main.A = TheAlu.SubtractAndSetFlags(regs.Main.A, regs.IYL, false);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IXH:
                TheAlu.Xor(regs.IXH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IXL:
                TheAlu.Xor(regs.IXL);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IYH:
                TheAlu.Xor(regs.IYH);
                return instruction.TStateCount;
            case Z80Instructions.InstructionID.XOR_IYL:
                TheAlu.Xor(regs.IYL);
                return instruction.TStateCount;

            case Z80Instructions.InstructionID.INI:
            {
                var hlMem = MainMemory.Poke(regs.Main.HL, ThePortHandler.In(regs.Main.BC));
                regs.Main.HL++;
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);

                // 'Undocumented'.
                var incC = (regs.Main.C + 1) & 255;
                regs.HalfCarryFlag = hlMem + incC > 255;
                regs.CarryFlag = hlMem + incC > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + incC) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.INIR:
            {
                // Looping version of INI.
                var hlMem = MainMemory.Poke(regs.Main.HL, ThePortHandler.In(regs.Main.BC));
                regs.Main.HL++;
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);
                if (regs.Main.B != 0)
                    regs.PC -= 2; // Repeat.

                // 'Undocumented'.
                var incC = (regs.Main.C + 1) & 255;
                regs.HalfCarryFlag = hlMem + incC > 255;
                regs.CarryFlag = hlMem + incC > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + incC) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;

                return regs.Main.B == 0 ? 16 : 21;
            }

            case Z80Instructions.InstructionID.IND:
            {
                var hlMem = MainMemory.Poke(regs.Main.HL, ThePortHandler.In(regs.Main.BC));
                regs.Main.HL--;
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);

                // 'Undocumented'.
                var decC = (regs.Main.C - 1) & 255;
                regs.HalfCarryFlag = hlMem + decC > 255;
                regs.CarryFlag = hlMem + decC > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + decC) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;
                
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.INDR:
            {
                // Looping version if IND.
                var hlMem = MainMemory.Poke(regs.Main.HL, ThePortHandler.In(regs.Main.BC));
                regs.Main.HL--;
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);
                if (regs.Main.B != 0)
                    regs.PC -= 2; // Repeat.

                // 'Undocumented'.
                var decC = (regs.Main.C - 1) & 255;
                regs.HalfCarryFlag = hlMem + decC > 255;
                regs.CarryFlag = hlMem + decC > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + decC) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;

                return regs.Main.B == 0 ? 16 : 21;
            }

            case Z80Instructions.InstructionID.OUTI:
            {
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);
                var hlMem = MainMemory.Peek(regs.Main.HL);
                ThePortHandler.Out(regs.Main.C, hlMem);
                regs.Main.HL++;

                // 'Undocumented'.
                regs.HalfCarryFlag = hlMem + regs.Main.L > 255;
                regs.CarryFlag = hlMem + regs.Main.L > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + regs.Main.L) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.OTIR:
            {
                // Looping version of OUTI.
                var hlMem = MainMemory.Peek(regs.Main.HL);
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);
                ThePortHandler.Out(regs.Main.C, hlMem);
                regs.Main.HL++;
                if (regs.Main.B != 0)
                    regs.PC -= 2; // Repeat.

                // 'Undocumented'.
                regs.HalfCarryFlag = hlMem + regs.Main.L > 255;
                regs.CarryFlag = hlMem + regs.Main.L > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + regs.Main.L) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;

                return regs.Main.B == 0 ? 16 : 21;
            }

            case Z80Instructions.InstructionID.OUTD:
            {
                var hlMem = MainMemory.Peek(regs.Main.HL);
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);
                ThePortHandler.Out(regs.Main.C, hlMem);
                regs.Main.HL--;

                // 'Undocumented'.
                regs.HalfCarryFlag = hlMem + regs.Main.L > 255;
                regs.CarryFlag = hlMem + regs.Main.L > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + regs.Main.L) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;
                
                return instruction.TStateCount;
            }

            case Z80Instructions.InstructionID.OTDR:
            {
                // Looping version of OUTD.
                var hlMem = MainMemory.Peek(regs.Main.HL);
                regs.Main.B = TheAlu.DecAndSetFlags(regs.Main.B);
                ThePortHandler.Out(regs.Main.C, hlMem);
                regs.Main.HL--;
                if (regs.Main.B != 0)
                    regs.PC -= 2; // Repeat.

                // 'Undocumented'.
                regs.HalfCarryFlag = hlMem + regs.Main.L > 255;
                regs.CarryFlag = hlMem + regs.Main.L > 255;
                regs.ParityFlag = Alu.IsEvenParity((byte)(((hlMem + regs.Main.L) & 7) ^ regs.Main.B));
                regs.SubtractFlag = (hlMem & 0x80) != 0;

                return regs.Main.B == 0 ? 16 : 21;
            }

            default:
                if (instruction.Run == null)
                    throw new UnsupportedInstruction(this, instruction);
                
                // Dynamic instruction.
                return instruction.Run(MainMemory, regs, TheAlu, valueAddress);
        }
    }
    
    /// <summary>
    /// Each tick increments the R register, but doesn't change the high bit.
    /// </summary>
    private void IncrementR() =>
        TheRegisters.R = (byte)((TheRegisters.R + 1 & 0x7F) | (TheRegisters.R & 0x80));
}
