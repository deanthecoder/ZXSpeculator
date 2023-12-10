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

using System.Reflection;

namespace Speculator.Core;

public class ZxSpectrum : IDisposable
{
    // ReSharper disable once InconsistentNaming
    private const int TStatesPerRenderFrame = 69888;

    public ZxSpectrum(ZXDisplay display)
    {
        TheDisplay = display;
        TheCpu = new CPU(new Memory(64 * 1024), PortHandler, SoundChip)
        {
            TStatesPerInterrupt = TStatesPerRenderFrame
        };

        TheCpu.RenderCallbackEvent += TheCPU_RenderCallbackEvent;
    }

    private SoundChip m_soundChip;
    private SoundChip SoundChip => m_soundChip ??= new SoundChip();

    public CPU TheCpu { get; }
    private ZXDisplay TheDisplay { get; }

    private ZXPortHandler m_portHandler;
    public ZXPortHandler PortHandler
    {
        get { return m_portHandler ??= new ZXPortHandler(SoundChip); }
    }

    public ZxSpectrum LoadBasicRom()
    {
        var systemRom = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ROMs", "48.rom");
        TheCpu.MainMemory.LoadBasicROM(systemRom);
        return this;
    }

    private void TheCPU_RenderCallbackEvent(CPU sender, CPU.RenderCallbackArgs args)
    {
        // todo - Invoke on UI thread.
        TheDisplay.UpdateScreen(sender);
    }

    public void Dispose()
    {
        m_soundChip?.Dispose();
        TheCpu?.PowerOffAsync();
    }
    
    public ZxSpectrum PowerOnAsync()
    {
        TheCpu.PowerOnAsync();
        return this;
    }
    
    public ZxSpectrum LoadRom(FileInfo romFile)
    {
        ZXFileFormats.LoadFile(TheCpu, romFile);
        return this;
    }
}