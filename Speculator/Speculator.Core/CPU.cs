// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If modified, please retain this copyright header, and consider telling us
// about your changes.  We're always glad to see how people use our code!
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// We do not accept any liability for damage caused by executing
// or modifying this code.

// todo - fix all the header comments.

using System.Diagnostics;
using System.Text;
using CSharp.Utils.ViewModels;

// ReSharper disable InconsistentNaming
namespace Speculator.Core;

public partial class CPU : ViewModelBase
{
    private readonly SoundHandler m_soundHandler;
    private Z80Instructions InstructionSet { get; }
    public Registers TheRegisters { get; }
    public Memory MainMemory { get; }
    private Alu TheAlu { get; }
    private IPortHandler ThePortHandler { get; }
    private Thread m_cpuThread;

    public CPU(Memory mainMemory, IPortHandler portHandler = null, SoundHandler soundHandler = null)
    {
        m_soundHandler = soundHandler;
        MainMemory = mainMemory;
        InstructionSet = new Z80Instructions();
        TheRegisters = new Registers();
        TheAlu = new Alu(TheRegisters);
        ThePortHandler = portHandler;
        ClockSync = new ClockSync(TStatesPerSecond);
    }

    public bool FullThrottle
    {
        get => m_fullThrottle;
        set
        {
            if (SetField(ref m_fullThrottle, value))
                ClockSync.Reset(m_TStatesSinceCpuStart);
        }
    }

    public void PowerOnAsync()
    {
        TheRegisters.Clear();
        m_cpuThread = new Thread(RunLoop) { Name = "Z80 CPU" };
        m_cpuThread.Start();
    }

    public void PowerOffAsync()
    {
        m_shutdownRequested = true;
    }

    public void ResetAsync()
    {
        m_resetRequested = true;
        PowerOffAsync();
    }

    /// <summary>
    /// Triggers the event which allows the CPU to 'tick' to the next instruction.
    /// </summary>
    public void DebuggerStep()
    {
        m_debuggerTickEvent.Set();
        RaiseAllPropertyChanged();
    }

    /// <summary>
    /// We pause the CPU when loading a new ROM/snapshot.
    /// </summary>
    public bool IsPaused { get; set; }

    public bool IsDebugging
    {
        get => m_isDebugging;
        set
        {
            if (!SetField(ref m_isDebugging, value))
                return;
            ClockSync.Reset(m_TStatesSinceCpuStart);
            RaiseAllPropertyChanged();
        }
    }

    private bool m_isHalted;

    private readonly AutoResetEvent m_debuggerTickEvent = new AutoResetEvent(false);
    private bool m_shutdownRequested;
    private bool m_resetRequested;

    private void RunLoop()
    {
        m_TStatesSinceInterrupt = 0;

        m_shutdownRequested = false;
        m_resetRequested = false;
        m_isHalted = false;
        IsPaused = false;

        while (!m_shutdownRequested)
        {
            if (IsPaused)
            {
                Thread.Sleep(100);
                continue;
            }
            
            // Allow debugger to stall execution.
            if (IsDebugging)
            {
                if (!m_debuggerTickEvent.WaitOne(TimeSpan.FromMilliseconds(100)))
                    continue; // Timed out.
            }
            else
            {
                // Sync the clock speed.
                if (!FullThrottle)
                    ClockSync.SyncWithRealTime(() => m_TStatesSinceCpuStart);
            }
            
            // Execute instruction.
            var TStates = ExecuteAtPC();
            m_TStatesSinceCpuStart += TStates;
            
            // Record speaker state.
            m_soundHandler.SampleSpeakerState(m_TStatesSinceCpuStart);

            // Handle interrupts.
            m_TStatesSinceInterrupt += TStates;
            if (TStatesPerInterrupt == 0 || m_TStatesSinceInterrupt < TStatesPerInterrupt)
                continue;
            if (m_isHalted)
            {
                m_isHalted = false;
                TheRegisters.PC++;
            }

            // Screen refresh.
            m_TStatesSinceInterrupt -= TStatesPerInterrupt;
            RenderCallbackEvent?.Invoke(this);

            // Handle MI interrupts.
            if (TheRegisters.IFF1)
            {
                TheRegisters.IFF1 = TheRegisters.IFF2 = false;

                switch (TheRegisters.IM)
                {
                    case 0:
                    case 1:
                        CallIfTrue(0x0038, true);
                        break;
                    case 2:
                        //Debug.Fail("IM2 currently untested."); // todo
                        CallIfTrue(MainMemory.PeekWord((ushort)((TheRegisters.I << 8) | 0xff)), true);
                        break;
                    default:
                        Debug.Fail("Invalid interrupt mode.");
                        break;
                }
            }
        }

        if (m_resetRequested)
        {
            m_resetRequested = false;
            PowerOnAsync();
        }

        m_shutdownRequested = false;
    }

    private int m_TStatesSinceInterrupt;
    private long m_TStatesSinceCpuStart;

    public const double TStatesPerSecond = 3494400;

    public int TStatesPerInterrupt { private get; set; }
    public delegate void RenderCallbackEventHandler(CPU sender);

    public event RenderCallbackEventHandler RenderCallbackEvent;

    private readonly List<string> m_recentInstructionList = new List<string>();
    private bool m_fullThrottle;
    private bool m_isDebugging;
    public ClockSync ClockSync { get; }

    private byte doIN_addrC()
    {
        var b = ThePortHandler?.In((TheRegisters.Main.B << 8) + TheRegisters.Main.C) ?? 0x00;
        TheRegisters.SignFlag = !Alu.IsBytePositive(b);
        TheRegisters.ZeroFlag = b == 0;
        TheRegisters.HalfCarryFlag = false;
        TheRegisters.ParityFlag = Alu.IsEvenParity(b);
        TheRegisters.SubtractFlag = false;
        return b;
    }

    public void RETN()
    {
        doRet();
        TheRegisters.IFF1 = TheRegisters.IFF2;
    }

    private void doRet()
    {
        TheRegisters.PC = MainMemory.PeekWord(TheRegisters.SP);
        TheRegisters.SP += 2;
    }

    private bool JumpIfTrue(ushort addr, bool b)
    {
        if (b)
            TheRegisters.PC = addr;

        return b;
    }

    private bool CallIfTrue(ushort addr, bool b)
    {
        if (b)
        {
            TheRegisters.SP -= 2;
            MainMemory.PokeWord(TheRegisters.SP, TheRegisters.PC);
            TheRegisters.PC = addr;
        }

        return b;
    }

    private static byte doSetBit(byte b, int i)
    {
        Debug.Assert(i >= 0 && i <= 7, "Index out of range.");
        return (byte) (b | (1 << i));
    }

    private static byte doResetBit(byte b, int i)
    {
        Debug.Assert(i >= 0 && i <= 7, "Index out of range.");
        var mask = (byte)~(1 << i);
        return (byte) (b & mask);
    }

    private void doBitTest(byte b, int i)
    {
        Debug.Assert(i >= 0 && i <= 7, "Index out of range.");
        TheRegisters.ZeroFlag = (b & (1 << i)) == 0;
        TheRegisters.SubtractFlag = false;
        TheRegisters.HalfCarryFlag = true;

        // From 'undocumented' docs.
        TheRegisters.ParityFlag = TheRegisters.ZeroFlag;
        TheRegisters.SignFlag = i == 7 && !Alu.IsBytePositive(b);
    }

    private ushort IXPlusD(byte d)
    {
        return (ushort)(TheRegisters.IX + Alu.FromTwosCompliment(d));
    }

    private ushort IYPlusD(byte d)
    {
        return (ushort)(TheRegisters.IY + Alu.FromTwosCompliment(d));
    }

    private void doCPI()
    {
        var oldCarry = TheRegisters.CarryFlag;
        TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.Main.HL), false);
        TheRegisters.Main.HL++;
        TheRegisters.Main.BC--;
        TheRegisters.ParityFlag = TheRegisters.Main.BC != 0;
        TheRegisters.SubtractFlag = true;
        TheRegisters.CarryFlag = oldCarry;
    }
        
    private void doCPD()
    {
        var oldCarry = TheRegisters.CarryFlag;
        TheAlu.SubtractAndSetFlags(TheRegisters.Main.A, MainMemory.Peek(TheRegisters.Main.HL), false);
        TheRegisters.Main.HL--;
        TheRegisters.Main.BC--;
        TheRegisters.ParityFlag = TheRegisters.Main.BC != 0;
        TheRegisters.SubtractFlag = true;
        TheRegisters.CarryFlag = oldCarry;
    }

    private void doLDI()
    {
        MainMemory.Poke(TheRegisters.Main.DE, MainMemory.Peek(TheRegisters.Main.HL));
        TheRegisters.Main.HL++;
        TheRegisters.Main.DE++;
        TheRegisters.Main.BC--;
        TheRegisters.HalfCarryFlag = false;
        TheRegisters.ParityFlag = TheRegisters.Main.BC != 0;
        TheRegisters.SubtractFlag = false;
    }

    private void doLDD()
    {
        MainMemory.Poke(TheRegisters.Main.DE, MainMemory.Peek(TheRegisters.Main.HL));
        TheRegisters.Main.HL--;
        TheRegisters.Main.DE--;
        TheRegisters.Main.BC--;
        TheRegisters.HalfCarryFlag = false;
        TheRegisters.ParityFlag = TheRegisters.Main.BC != 0;
        TheRegisters.SubtractFlag = false;
    }

    private class PCHitMemoryLimit : Exception
    {
        public PCHitMemoryLimit() : base("Program counter exceeded the available memory.")
        {
        }
    }

    private class UnsupportedInstruction : Exception
    {
        public UnsupportedInstruction(CPU cpu)
            : base($"An invalid/unsupported emulated instruction was encountered: ({cpu.TheRegisters.PC:X4}: {cpu.MainMemory.ReadAsHexString(cpu.TheRegisters.PC, 4)}...)")
        {
            var message = base.Message;
            message = cpu.m_recentInstructionList.Aggregate(message, (current, s) => current + "\n  " + s);
            Debug.WriteLine(message);
        }
    }

    public string Disassembly
    {
        get
        {
            var sb = new StringBuilder();
            var pc = TheRegisters.PC;
            for (var i = 0; i < 6; i++)
            {
                var hexBytes = string.Empty;
                var pcOffset = Disassemble(pc, ref hexBytes, out var mnemonics);
                mnemonics = $"*{mnemonics}*";
                sb.AppendLine($"{pc:X04}: {mnemonics,-14}  {hexBytes,-11}");
                if (pcOffset == 0)
                    break;
                pc += pcOffset;
            }

            return sb.ToString();
        }
    }

    public double UpTime => m_TStatesSinceCpuStart / TStatesPerSecond;

    private ushort Disassemble(ushort addr, ref string hexBytes, out string mnemonics)
    {
        var instruction = InstructionSet.findInstructionAtMemoryLocation(MainMemory, addr);

        if (instruction == null)
        {
            hexBytes = MainMemory.ReadAsHexString(addr, 4) + "...";
            mnemonics = "??";
            return 0;
        }

        // Format the instruction as hex bytes.
        for (var i = 0; i < instruction.ByteCount; i++)
        {
            if (i > 0)
                hexBytes += " ";
            hexBytes += MainMemory.ReadAsHexString((ushort)(addr + i), 1);
        }

        // Format the instruction as opcodes.
        var hexParts = instruction.HexTemplate.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var hexValues = new List<string>();
        for (var i = 0; i < hexParts.Length; i++)
        {
            switch (hexParts[i])
            {
                case "n":
                    hexValues.Add(MainMemory.ReadAsHexString((ushort)(addr + i), 1));
                    break;
                case "d":
                    hexValues.Add(Alu.FromTwosCompliment(MainMemory.Peek((ushort)(addr + i))).ToString());
                    break;
            }
        }

        // LD hl,nn = LD hl,1234 = A3 n n => hexValues [34, 12]
        mnemonics = instruction.MnemonicTemplate;
        foreach (var hexValue in hexValues)
        {
            var index = Math.Max(mnemonics.LastIndexOf('n'), mnemonics.LastIndexOf('d'));
            Debug.Assert(index >= 0);
            mnemonics = mnemonics.Insert(index, hexValue).Remove(index + hexValue.Length, 1);
        }

        return instruction.ByteCount;
    }
}