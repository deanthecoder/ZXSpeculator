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

using Speculator.Core.HostDevices;

namespace Speculator.Core;

/// <summary>
/// Emulated sound support, recording virtual speaker movements.
/// Uses a SoundDevice to send sound to the host device.
/// </summary>
public class SoundHandler : IDisposable
{
    private bool m_speakerState;
    private const int SampleHz = 11025;
    private const int TicksPerSample = (int)(CPU.TStatesPerSecond / SampleHz);
    private long m_lastTStateCount;
    private readonly int[] m_speakerStates = new int[2];
    private readonly SoundDevice m_soundDevice;
    private bool m_isDisposed;
    private readonly Thread m_thread;
    private bool m_isSoundEnabled = true;

    public SoundHandler()
    {
        try
        {
            m_soundDevice = new SoundDevice(SampleHz);
            m_thread = new Thread(() => m_soundDevice.SoundLoop(() => m_isDisposed))
            {
                Name = "Sound thread"
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to initialize host sound device: {e.Message}");
        }
    }
    
    public void Start()
    {
        if (m_thread?.IsAlive != true)
            m_thread?.Start();
    }

    public void SetSpeakerState(bool soundBit)
    {
        m_speakerState = soundBit;
    }

    public void Dispose()
    {
        // Wait for the sound thread to exit.
        m_isDisposed = true;
        m_thread?.Join();
    }

    public void SampleSpeakerState(long tStateCount)
    {
        if (!m_isSoundEnabled)
            return;
        
        // Update the count of on/off speaker states.
        m_speakerStates[m_speakerState ? 1 : 0]++;

        var elapsedTicks = tStateCount - m_lastTStateCount;
        if (elapsedTicks < TicksPerSample)
            return; // Not enough time elapsed - Keep collecting speaker states.

        // We've collected enough samples for averaging to occur.
        m_lastTStateCount = tStateCount;
        var sampleValue = m_speakerStates[1] / ((double)m_speakerStates[0] + m_speakerStates[1]);
        m_speakerStates[0] = 0;
        m_speakerStates[1] = 0;

        // Append to the sample buffer.
        m_soundDevice.AddSample(m_isSoundEnabled ? sampleValue : 0.0);
    }

    public void SetSoundEnabled(bool isSoundEnabled)
    {
        m_isSoundEnabled = isSoundEnabled;
        m_soundDevice.SetEnabled(isSoundEnabled);
    }
}