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

namespace Speculator.Core.Debugger;

public class SingleBreakpoint
{
    private readonly CPU m_theCpu;
    private bool m_isEnabled;
    
    public ushort Addr { get; }
    public event EventHandler BreakpointHit;
    
    public bool IsEnabled
    {
        get => m_isEnabled;
        set
        {
            if (m_isEnabled == value)
                return;
            
            if (m_isEnabled)
                m_theCpu.Ticked -= OnCpuTicked;
            m_isEnabled = value;
            if (m_isEnabled)
                m_theCpu.Ticked += OnCpuTicked;
        }
    }

    public SingleBreakpoint(CPU theCpu, ushort addr)
    {
        m_theCpu = theCpu;
        Addr = addr;
        IsEnabled = true;
    }
    
    private void OnCpuTicked(object sender, (ushort prevPC, ushort currentPC) pcValues)
    {
        if (pcValues.currentPC != Addr)
            return; // Breakpoint not hit yet...
        
        m_theCpu.IsDebuggerActive = true;
        BreakpointHit?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() => Addr.ToString("X04");
}
