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