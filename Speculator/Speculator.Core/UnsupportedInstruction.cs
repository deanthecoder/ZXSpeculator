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

public class UnsupportedInstruction : Exception
{
    public UnsupportedInstruction(CPU cpu, Instruction instruction)
        : base($"Unsupported Z80 instruction '{instruction}' at 0x{cpu.TheRegisters.PC:X4}")
    {
    }

    public UnsupportedInstruction(CPU cpu)
        : base($"Unsupported Z80 opcode sequence '{cpu.MainMemory.ReadAsHexString(cpu.TheRegisters.PC, 4, true)}' at 0x{cpu.TheRegisters.PC:X4}")
    {
    }
}
