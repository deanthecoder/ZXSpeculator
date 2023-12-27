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
                m_theCpu.Ticked -= OnTicked;
            m_isEnabled = value;
            if (m_isEnabled)
                m_theCpu.Ticked += OnTicked;
        }
    }

    public SingleBreakpoint(CPU theCpu, ushort addr)
    {
        m_theCpu = theCpu;
        Addr = addr;
        IsEnabled = true;
    }
    
    private void OnTicked(object sender, EventArgs e)
    {
        if (m_theCpu.TheRegisters.PC != Addr)
            return; // Breakpoint not hit yet...
        
        m_theCpu.IsDebugStepping = true;
        BreakpointHit?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() => Addr.ToString("X04");
}