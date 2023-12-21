namespace Speculator.Core;

public class UnsupportedInstruction : Exception
{
    public UnsupportedInstruction(CPU cpu, Instruction instruction)
        : base($"Unsupported Z80 instruction '{instruction}' at 0x{cpu.TheRegisters.PC:X4}")
    {
    }
}