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

/// <summary>
/// The main emulation entry point object.
/// </summary>
public class ZxSpectrum : IDisposable
{
    // ReSharper disable once InconsistentNaming
    private const int TStatesPerRenderFrame = 69888;

    private SoundHandler m_soundHandler;

    public ZxSpectrum(ZxDisplay display)
    {
        TheDisplay = display;
        TheCpu = new CPU(new Memory(64 * 1024), PortHandler, SoundHandler)
        {
            TStatesPerInterrupt = TStatesPerRenderFrame
        };

        TheCpu.RenderCallbackEvent += TheCPU_RenderCallbackEvent;
    }

    private SoundHandler SoundHandler => m_soundHandler ??= new SoundHandler();

    public CPU TheCpu { get; }
    private ZxDisplay TheDisplay { get; }

    private ZxPortHandler m_portHandler;
    public ZxPortHandler PortHandler => m_portHandler ??= new ZxPortHandler(SoundHandler);

    public ZxSpectrum LoadBasicRom()
    {
        var systemRom = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "ROMs", "48.rom");
        TheCpu.MainMemory.LoadBasicROM(systemRom);
        return this;
    }

    private void TheCPU_RenderCallbackEvent(CPU sender) => TheDisplay.UpdateScreen(sender);

    public void Dispose()
    {
        m_soundHandler?.Dispose();
        TheCpu?.PowerOffAsync();
    }
    
    public ZxSpectrum PowerOnAsync()
    {
        TheCpu.PowerOnAsync();
        return this;
    }
    
    public ZxSpectrum LoadRom(FileInfo romFile)
    {
        ZxFileFormats.LoadFile(TheCpu, romFile);
        return this;
    }
}