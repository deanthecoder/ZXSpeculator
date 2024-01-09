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

using NUnit.Framework;
using Speculator.Core;

namespace UnitTests.FuseUtils;

/// <summary>
/// Represents a single Fuse test result.
/// </summary>
public class FuseResult
{
    private readonly ushort[] m_registers;
    private readonly ushort[] m_state;
    private readonly string m_memory;
    private readonly ushort m_tStates;

    public string TestId { get; }

    public ushort ExpectedPC => m_registers[11];

    public FuseResult(string testId, string registers, string state, string memory)
    {
        m_registers = registers.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(o => Convert.ToUInt16(o, 16)).ToArray();
        m_state = state.Split(' ', StringSplitOptions.RemoveEmptyEntries).SkipLast(1).Select(o => Convert.ToUInt16(o, 16)).ToArray();
        m_tStates = Convert.ToUInt16(state.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last());
        m_memory = memory;
        TestId = testId;
    }
    
    public void Verify(CPU cpu, bool relaxFlagChecks, bool relaxTStateChecks)
    {
        VerifyRegisters(cpu, relaxFlagChecks);
        VerifyCpuState(cpu, relaxTStateChecks);
        VerifyMemory(cpu);
    }

    private void VerifyMemory(CPU cpu)
    {
        Assert.Multiple(() =>
        {
            foreach (var memoryLine in m_memory.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var memory =
                    memoryLine
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .TakeWhile(o => o != "-1")
                        .Select(o => Convert.ToUInt16(o, 16)).ToArray();
                var addr = memory[0];
                foreach (var value in memory.Skip(1))
                    Assert.That(cpu.MainMemory.Data[addr++], Is.EqualTo(value));
            }
        });
    }

    private void VerifyCpuState(CPU cpu, bool relaxTStateChecks)
    {
        Assert.Multiple(() =>
        {
            Assert.That(cpu.TheRegisters.I, Is.EqualTo(m_state[0]), "Register mismatch: I");
            Assert.That(cpu.TheRegisters.R, Is.EqualTo(m_state[1]), "Register mismatch: R");
            Assert.That(cpu.TheRegisters.IFF1, Is.EqualTo(m_state[2] != 0), "Register mismatch: IFF1");
            Assert.That(cpu.TheRegisters.IFF2, Is.EqualTo(m_state[3] != 0), "Register mismatch: IFF2");
            Assert.That(cpu.TheRegisters.IM, Is.EqualTo(m_state[4]), "Register mismatch: IM");
        });

        if (!relaxTStateChecks)
            Assert.That(cpu.TStatesSinceCpuStart, Is.EqualTo(m_tStates));
    }
    
    private void VerifyRegisters(CPU cpu, bool relaxFlagChecks)
    {
        var registers = m_registers;
        Assert.Multiple(() =>
        {
            Assert.That(cpu.TheRegisters.Main.A, Is.EqualTo((registers[0] & 0xff00) >> 8), "Register mismatch: A");
            
            var mainF = cpu.TheRegisters.Main.F;
            var expected = registers[0] & 0x00ff;
            if (relaxFlagChecks)
            {
                // Ignore bit 3 and 5.
                mainF &= 0b11010111;
                expected &= 0b11010111;
            }
            Assert.That(mainF, Is.EqualTo(expected), "Flags (F): SZ5H3PNC\n" +
                                                     $"Actual:    {Convert.ToString(mainF, 2).PadLeft(8, '0')}\n" +
                                                     $"Expected:  {Convert.ToString(expected, 2).PadLeft(8, '0')}");
            
            Assert.That(cpu.TheRegisters.Main.BC, Is.EqualTo(registers[1]), "Register mismatch: BC");
            Assert.That(cpu.TheRegisters.Main.DE, Is.EqualTo(registers[2]), "Register mismatch: DE");
            Assert.That(cpu.TheRegisters.Main.HL, Is.EqualTo(registers[3]), "Register mismatch: HL");
            Assert.That(cpu.TheRegisters.Alt.A, Is.EqualTo((registers[4] & 0xff00) >> 8), "Register mismatch: A'");

            var altF = cpu.TheRegisters.Alt.F;
            expected = registers[4] & 0x00ff;
            if (relaxFlagChecks)
            {
                // Ignore bit 3 and 5.
                altF &= 0b11010111;
                expected &= 0b11010111;
            }
            Assert.That(altF, Is.EqualTo(expected), "Flags (F'): SZ5H3PNC\n" +
                                                    $"Actual:     {Convert.ToString(altF, 2).PadLeft(8, '0')}\n" +
                                                    $"Expected:   {Convert.ToString(expected, 2).PadLeft(8, '0')}");
            
            Assert.That(cpu.TheRegisters.Alt.BC, Is.EqualTo(registers[5]), "Register mismatch: BC'");
            Assert.That(cpu.TheRegisters.Alt.DE, Is.EqualTo(registers[6]), "Register mismatch: DE'");
            Assert.That(cpu.TheRegisters.Alt.HL, Is.EqualTo(registers[7]), "Register mismatch: HL'");
            Assert.That(cpu.TheRegisters.IX, Is.EqualTo(registers[8]), "Register mismatch: IX");
            Assert.That(cpu.TheRegisters.IY, Is.EqualTo(registers[9]), "Register mismatch: IY");
            Assert.That(cpu.TheRegisters.SP, Is.EqualTo(registers[10]), "Register mismatch: SP");
            Assert.That(cpu.TheRegisters.PC, Is.EqualTo(registers[11]), "Register mismatch: PC");
        });
    }
}