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

#if WINDOWS
using System.Collections.Concurrent;
using NAudio.Wave;

namespace Speculator.Core.HostDevices;

public class SoundDevice
{
    private readonly WaveOutEvent m_waveOut;
    private readonly BufferedWaveProvider m_bufferedWaveProvider;
    private readonly ConcurrentQueue<float> m_soundBuffer = new ConcurrentQueue<float>();
    private bool m_isSoundEnabled = true;

    public SoundDevice(int sampleHz)
    {
        m_waveOut = new WaveOutEvent { DesiredLatency = 200, NumberOfBuffers = 2 };
        m_bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(sampleHz, 8, 1))
        {
            BufferLength = sampleHz
        };
        m_waveOut.Init(m_bufferedWaveProvider);
    }

    public void SoundLoop(Func<bool> isCancelled)
    {
        m_waveOut.Play();

        const int sampleLength = 200;
        var buffer = new byte[sampleLength];
        while (!isCancelled())
        {
            if (!m_isSoundEnabled)
                continue;

            var bufferIndex = 0;
            for (var i = 0; i < sampleLength && m_bufferedWaveProvider.BufferedBytes + i < m_bufferedWaveProvider.BufferLength && m_soundBuffer.TryDequeue(out var sample); i++)
                buffer[bufferIndex++] = (byte)(sample * 255);

            m_bufferedWaveProvider.AddSamples(buffer, 0, bufferIndex);
        }

        m_waveOut.Stop();
    }

    public void AddSample(double sampleValue)
    {
        if (m_isSoundEnabled)
            m_soundBuffer.Enqueue((float)sampleValue);
    }

    public void SetEnabled(bool isSoundEnabled)
    {
        m_isSoundEnabled = isSoundEnabled;

        if (!isSoundEnabled)
        {
            m_waveOut.Stop();
            m_soundBuffer.Clear();
            m_waveOut.Play();
        }
    }
}
#endif
