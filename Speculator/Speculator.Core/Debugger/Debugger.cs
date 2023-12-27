using System.Collections.ObjectModel;
using CSharp.Utils.Validators;
using CSharp.Utils.ViewModels;
using Speculator.Core.Extensions;

namespace Speculator.Core.Debugger;

public class Debugger : ViewModelBase
{
    public CPU TheCpu { get; }
    private string m_breakpointAddr;
    private bool m_isStepping;
    private bool m_isVisible;
    private bool m_recordHistory;

    public event EventHandler IsSteppingChanged;

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
            if (!SetField(ref m_isStepping, value))
                return;
            IsSteppingChanged?.Invoke(this, EventArgs.Empty);
            TheCpu.IsDebugStepping = value;
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
                TheCpu.Ticked -= OnCpuTick;
            m_recordHistory = value;
            if (m_recordHistory)
            {
                TheCpu.Ticked += OnCpuTick;
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

    public bool IsValid => new HexStringAttribute().IsValid(BreakpointAddr);

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

    private void OnCpuTick(object sender, EventArgs e)
    {
        // Record the CPU history.
        var unused = string.Empty;
        TheCpu.Disassemble(TheCpu.TheRegisters.PC, ref unused, out var mnemonics);
        mnemonics = $"*{mnemonics}*";
        History.Add($"{TheCpu.TheRegisters.PC:X04}: {mnemonics}");
        while (History.Count > 1000)
            History.RemoveAt(0);

        OnPropertyChanged(nameof(History));
    }

    public void StartDebugging() => IsStepping = true;
    public void StopDebugging() => IsStepping = false;
    public void Show() => IsVisible = true;
    public void Hide() => IsVisible = false;
}