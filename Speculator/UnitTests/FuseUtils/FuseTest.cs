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

using Speculator.Core;

namespace UnitTests.FuseUtils;

/// <summary>
/// Represents a single Fuse test.
/// </summary>
public class FuseTest
{
    private readonly string m_registers;
    private readonly string m_state;
    private readonly string m_memory;

    public string TestId { get; }

    public FuseTest(string testId, string registers, string state, string memory)
    {
        TestId = testId;
        m_registers = registers;
        m_state = state;
        m_memory = memory;
    }
    
    public void InitCpu(CPU cpu)
    {
        var registers = m_registers.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(o => Convert.ToUInt16(o, 16)).ToArray();
        cpu.TheRegisters.Main.AF = registers[0];
        cpu.TheRegisters.Main.BC = registers[1];
        cpu.TheRegisters.Main.DE = registers[2];
        cpu.TheRegisters.Main.HL = registers[3];
        cpu.TheRegisters.Alt.AF = registers[4];
        cpu.TheRegisters.Alt.BC = registers[5];
        cpu.TheRegisters.Alt.DE = registers[6];
        cpu.TheRegisters.Alt.HL = registers[7];
        cpu.TheRegisters.IX = registers[8];
        cpu.TheRegisters.IY = registers[9];
        cpu.TheRegisters.SP = registers[10];
        cpu.TheRegisters.PC = registers[11];

        var state = m_state.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(o => Convert.ToUInt16(o, 16)).ToArray();
        cpu.TheRegisters.I = (byte)state[0];
        cpu.TheRegisters.R = (byte)state[1];
        cpu.TheRegisters.IFF1 = state[2] != 0;
        cpu.TheRegisters.IFF2 = state[3] != 0;
        cpu.TheRegisters.IM = (byte)state[4];
        var isHalted = state[5] != 0;
        if (isHalted)
            throw new NotSupportedException();

        foreach (var memoryLine in m_memory.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var memory =
                memoryLine
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .TakeWhile(o => o != "-1")
                    .Select(o => Convert.ToUInt16(o, 16)).ToArray();
            var addr = memory[0];
            foreach (var value in memory.Skip(1))
                cpu.MainMemory.Data[addr++] = (byte)value;
        }
    }

    public override string ToString() => TestId;
    
    public bool Run(CPU cpu, ushort expectedPc)
    {
        var ticks = 0;
        do
        {
            cpu.Step();
        }
        while (cpu.TheRegisters.PC < expectedPc && ticks++ < 65536);

        return ticks < 65536;
    }
}