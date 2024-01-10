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

using Speculator.Core.Tape;

namespace Speculator.Core;

/// <summary>
/// The main emulation entry point object.
/// </summary>
public class ZxSpectrum : IDisposable
{
    // ReSharper disable once InconsistentNaming
    private const int TStatesPerRenderFrame = 69888;

    private SoundHandler m_soundHandler;
    private ZxDisplay TheDisplay { get; }
    public Debugger.Debugger TheDebugger { get; }

    public CPU TheCpu { get; }
    public ZxPortHandler PortHandler { get; }
    public SoundHandler SoundHandler => m_soundHandler ??= new SoundHandler();
    public TapeLoader TheTapeLoader { get; } = new TapeLoader();

    public ZxSpectrum(ZxDisplay display)
    {
        TheDisplay = display;
        PortHandler = new ZxPortHandler(SoundHandler, TheDisplay, TheTapeLoader);
        TheCpu = new CPU(new Memory(), PortHandler, SoundHandler)
        {
            TStatesPerInterrupt = TStatesPerRenderFrame
        };
        TheTapeLoader.SetCpu(TheCpu);
        TheDebugger = new Debugger.Debugger(TheCpu);

        TheCpu.RenderScanline += TheDisplay.OnRenderScanline;

        TheDebugger.IsSteppingChanged += (_, _) =>
        {
            if (TheDebugger.IsStepping)
                SoundHandler.IsEnabled = false;
        };
    }
    
    public ZxSpectrum LoadBasicRom()
    {
        var systemRom = Path.Combine(AppContext.BaseDirectory, "ROMs", "48.rom");
        TheCpu.MainMemory.LoadRom(systemRom);
        return this;
    }

    public void Dispose()
    {
        m_soundHandler?.Dispose();
        PortHandler?.Dispose();
        TheCpu?.PowerOffAsync();
    }
    
    public ZxSpectrum PowerOnAsync()
    {
        TheCpu.PowerOnAsync();
        return this;
    }
    
    public ZxSpectrum LoadRom(FileInfo romFile)
    {
        using var _ = PortHandler.CreateKeyBlocker();
        new ZxFileIo(TheCpu, TheDisplay, TheTapeLoader).LoadFile(romFile);
        return this;
    }
}
