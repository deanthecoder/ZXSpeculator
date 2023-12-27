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

    public event EventHandler IsSteppingChanged;

    public MemoryDumpViewModel MemoryDump { get; }

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

    public void StartDebugging() => IsStepping = true;
    public void StopDebugging() => IsStepping = false;
    public void Show() => IsVisible = true;
    public void Hide() => IsVisible = false;
}