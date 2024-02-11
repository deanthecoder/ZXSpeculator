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

using System.Collections.ObjectModel;
using System.Text;
using CSharp.Core;
using CSharp.Core.Validators;
using CSharp.Core.ViewModels;
using Speculator.Core.Extensions;

namespace Speculator.Core.Debugger;

public class Debugger : ViewModelBase
{
    private const int MaxHistoryLength = 200;
    private readonly ActionConsolidator m_propertyEventRaiser;
    private string m_breakpointAddr;
    private bool m_isStepping;
    private bool m_isVisible;
    private bool m_recordHistory;
    private int m_cpuSubscriptions;

    public event EventHandler IsSteppingChanged;

    public CPU TheCpu { get; }
    public MemoryDumpViewModel MemoryDump { get; }
    public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

    /// <summary>
    /// Whether the UI is visible.
    /// </summary>
    public bool IsVisible
    {
        get => m_isVisible;
        private set => SetField(ref m_isVisible, value);
    }

    public bool IsStepping
    {
        get => m_isStepping;
        set
        {
            if (m_isStepping == value)
                return;
            
            if (m_isStepping)
                SubscribeToCpuEvents(false);

            m_isStepping = value;
            IsSteppingChanged?.Invoke(this, EventArgs.Empty);
            TheCpu.IsDebuggerActive = value;

            if (m_isStepping)
                SubscribeToCpuEvents(true);

            RaiseAllPropertiesChanged();
        }
    }

    public bool RecordHistory
    {
        get => m_recordHistory;
        set
        {
            if (m_recordHistory == value)
                return;

            if (m_recordHistory)
                SubscribeToCpuEvents(false);
            m_recordHistory = value;
            if (m_recordHistory)
            {
                SubscribeToCpuEvents(true);
                History.Clear();
            }
        }
    }

    [HexString]
    public string BreakpointAddr
    {
        get => m_breakpointAddr ??= "ED15"; // :D
        set
        {
            if (!SetField(ref m_breakpointAddr, value))
                return;
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(Instruction));
        }
    }
    
    /// <summary>
    /// True if BreakpointAddr is valid.
    /// </summary>
    public bool IsValid => new HexStringAttribute().IsValid(BreakpointAddr);

    /// <summary>
    /// Human readable instruction located at [BreakpointAddr].
    /// </summary>
    public string Instruction
    {
        get
        {
            if (!new HexStringAttribute().IsValid(BreakpointAddr))
                return "N/A";

            var addr = Convert.ToUInt16(BreakpointAddr, 16);
            var hexBytes = string.Empty;
            TheCpu.Disassemble(addr, ref hexBytes, out var mnemonics);

            return mnemonics;
        }
    }

    public ObservableCollection<SingleBreakpoint> Breakpoints { get; } = new ObservableCollection<SingleBreakpoint>();

    public Debugger(CPU theCpu = null)
    {
        m_propertyEventRaiser = new ActionConsolidator(RaiseAllPropertiesChanged);
        TheCpu = theCpu;
        MemoryDump = new MemoryDumpViewModel(TheCpu?.MainMemory ?? new Memory());
    }

    public void AddBreakpoint()
    {
        var addr = Convert.ToUInt16(BreakpointAddr, 16);
        if (Breakpoints.Any(o => o.Addr == addr))
            return; // Breakpoint already set.
        
        var breakpoint = new SingleBreakpoint(TheCpu, addr);
        breakpoint.BreakpointHit += (_, _) =>
        {
            IsStepping = true;
            IsVisible = true;
        };
        Breakpoints.Add(breakpoint);
    }

    private void OnCpuTicked(object sender, (int elapsedTicks, ushort prevPC, ushort currentPC) args)
    {
        if (RecordHistory)
        {
            // Record the CPU history.
            var unused = string.Empty;
            TheCpu.Disassemble(args.prevPC, ref unused, out var mnemonics);
            mnemonics = $"*{mnemonics}*";
            History.Add($"{args.prevPC:X04}: {mnemonics}");
            while (History.Count > MaxHistoryLength)
                History.RemoveAt(0);
        }

        if (IsStepping)
            m_propertyEventRaiser.Invoke();
    }

    /// <summary>
    /// Multi-line disassembly, starting at [PC].
    /// </summary>
    public string Disassembly
    {
        get
        {
            var sb = new StringBuilder();
            var pc = TheCpu.TheRegisters.PC;
            for (var i = 0; i < 6; i++)
            {
                var hexBytes = string.Empty;
                var pcOffset = TheCpu.Disassemble(pc, ref hexBytes, out var mnemonics);
                sb.AppendLine($"{pc:X04}: *{mnemonics,-14}*  {hexBytes,-11}");
                if (pcOffset == 0)
                    break;
                pc += pcOffset;
            }

            return sb.ToString();
        }
    }

    public void StartDebugging() => IsStepping = true;
    public void StopDebugging() => IsStepping = false;
    public void Show() => IsVisible = true;
    public void Hide() => IsVisible = false;

    public void RunToInterrupt()
    {
        TheCpu.InterruptFired += OnInterruptFired;
        StopDebugging();
        return;

        void OnInterruptFired(object sender, EventArgs e)
        {
            TheCpu.InterruptFired -= OnInterruptFired;
            StartDebugging();
        }
    }

    private void SubscribeToCpuEvents(bool b)
    {
        if (m_cpuSubscriptions == 0 && b)
            TheCpu.Ticked += OnCpuTicked;
        else if (m_cpuSubscriptions == 1 && !b)
            TheCpu.Ticked -= OnCpuTicked;

        m_cpuSubscriptions += b ? 1 : -1;
    }
}
