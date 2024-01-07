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

using CSharp.Utils;
using CSharp.Utils.ViewModels;
using Speculator.Core.HostDevices;

namespace Speculator.Core;

/// <summary>
/// Emulated sound support, recording virtual speaker movements.
/// Uses a SoundDevice to send sound to the host device.
/// </summary>
public class SoundHandler : ViewModelBase, IDisposable
{
    private byte m_soundLevel;
    private const int SampleHz = 11025;
    private const int TicksPerSample = (int)(CPU.TStatesPerSecond / SampleHz);
    private long m_lastTStateCount;
    private readonly int[] m_soundLevels = new int[4];
    private readonly SoundDevice m_soundDevice;
    private bool m_isDisposed;
    private readonly Thread m_thread;
    private bool m_isEnabled = true;

    public SoundHandler()
    {
        try
        {
            m_soundDevice = new SoundDevice(SampleHz);
            m_thread = new Thread(() => m_soundDevice.SoundLoop(() => m_isDisposed))
            {
                Name = "Sound Device",
                Priority = ThreadPriority.AboveNormal
            };
        }
        catch (Exception e)
        {
            Logger.Instance.Error($"Failed to initialize host sound device: {e.Message}");
        }
    }

    public bool IsEnabled
    {
        get => m_isEnabled;
        set
        {
            if (SetField(ref m_isEnabled, value))
                m_soundDevice.SetEnabled(value);
        }
    }

    public void Start()
    {
        if (m_thread?.IsAlive != true)
            m_thread?.Start();
    }

    /// <summary>
    /// Called whenever the CPU's speaker state changes.
    /// </summary>
    public void SetSpeakerState(byte soundLevel) =>
        m_soundLevel = soundLevel;

    public void Dispose()
    {
        // Wait for the sound thread to exit.
        m_isDisposed = true;
        m_thread?.Join();
    }

    /// <summary>
    /// Called every CPU tick to build a collection of speaker samples.
    /// Passed to the sound device's buffer when enough are collected.
    /// </summary>
    public void SampleSpeakerState(long tStateCount)
    {
        // Update the count of on/off speaker states.
        m_soundLevels[m_soundLevel]++;

        var elapsedTicks = tStateCount - m_lastTStateCount;
        if (elapsedTicks < TicksPerSample)
            return; // Not enough time elapsed - Keep collecting speaker states.

        // We've collected enough samples for averaging to occur.
        m_lastTStateCount += TicksPerSample;
        var sampleValue = 0.0;
        var sampleCount = 0.0;
        for (var i = 0; i < m_soundLevels.Length; i++)
        {
            sampleValue += i * m_soundLevels[i];
            sampleCount += m_soundLevels[i];
            m_soundLevels[i] = 0;
        }
        
        // Append to the sample buffer.
        var value = sampleCount > 0.0 ? sampleValue * 0.25 / sampleCount : 0.0;
        m_soundDevice?.AddSample(value);
    }
}
