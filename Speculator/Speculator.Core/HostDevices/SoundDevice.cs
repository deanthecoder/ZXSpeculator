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
using OpenTK.Audio.OpenAL;

namespace Speculator.Core.HostDevices;

/// <summary>
/// A sound device to interface with the host machine's sound card.
/// </summary>
public class SoundDevice
{
    private readonly int m_source;
    private readonly int[] m_buffers;
    private readonly int m_sampleRate;
    private bool m_isSoundEnabled = true;
    private byte m_lastWrittenSample;

    /// <summary>
    /// Data received from the CPU, copied into m_transferBuffer for transfer to the sound card.
    /// </summary>
    private readonly List<byte> m_cpuBuffer = new List<byte>(4096);

    /// <summary>
    /// The number of device buffers we can queue.
    /// </summary>
    private const int BufferCount = 3;
    
    /// <summary>
    /// Fixed buffer for interop with the sound card. Used to fill the device buffers.
    /// </summary>
    private readonly byte[] m_transferBuffer;
    
    public SoundDevice(int sampleHz)
    {
        m_sampleRate = sampleHz;

        // Initialize OpenAL.
        var device = ALC.OpenDevice(null);
        var context = ALC.CreateContext(device, (int[])null);
        ALC.MakeContextCurrent(context);

        // Generate buffers and a source.
        m_buffers = AL.GenBuffers(BufferCount);
        m_source = AL.GenSource();

        // Enough data for 0.1 seconds of play, split between all buffers.
        var bufferSize = (int)(m_sampleRate * 0.1 / BufferCount);
        m_transferBuffer = new byte[bufferSize];
    }

    public void SoundLoop(Func<bool> isCancelled)
    {
        // Wait for 'real' sound data to appear.
        while (!isCancelled() && m_cpuBuffer.Count < m_transferBuffer.Length * BufferCount)
            Thread.Sleep(10);
        
        // Pre-fill all buffers with initial data.
        foreach (var bufferId in m_buffers)
            UpdateBufferData(bufferId);

        // Start playback (Muted, to avoid the 'pop').
        Mute();
        AL.SourcePlay(m_source);
        CheckSoundError();

        var gain = 0.0f;
        while (!isCancelled())
        {
            Thread.Sleep(10);

            if (gain < 0.2)
            {
                gain += 0.005f;
                AL.Source(m_source, ALSourcef.Gain, gain);
            }

            // Fill any unused device buffers with more CPU data.
            AL.GetSource(m_source, ALGetSourcei.BuffersProcessed, out var buffersProcessed);
            CheckSoundError();
            while (buffersProcessed-- > 0)
            {
                var bufferId = AL.SourceUnqueueBuffer(m_source);
                CheckSoundError();
                UpdateBufferData(bufferId);
            }

            // Restart playback if it has stopped and there are device buffers queued.
            AL.GetSource(m_source, ALGetSourcei.SourceState, out var state);
            CheckSoundError();
            if ((ALSourceState)state == ALSourceState.Playing)
                continue; // Device is playing - All good.

            AL.GetSource(m_source, ALGetSourcei.BuffersQueued, out var buffersQueued);
            CheckSoundError();
            if (buffersQueued <= 0)
                continue; // No buffers ready for playback, yet.

            gain = 0.0f;
            AL.Source(m_source, ALSourcef.Gain, gain);
            AL.SourcePlay(m_source);
            CheckSoundError();
            ClearCpuBuffer();
        }

        Mute();
        AL.SourceStop(m_source);
    }

    public void Mute() =>
        AL.Source(m_source, ALSourcef.Gain, 0.0f);

    private static void CheckSoundError()
    {
        var err = AL.GetError();
        if (err != ALError.NoError)
            Logger.Instance.Error($"Sound device error: {err}");
    }

    private void UpdateBufferData(int bufferId)
    {
        var dstIndex = 0;
        lock (m_cpuBuffer)
        {
            // Compensate if sound data is generated faster than we can consume it.
            var excessBytes = m_cpuBuffer.Count - BufferCount * m_transferBuffer.Length;
            excessBytes = Math.Max(0, excessBytes);
            var speedUp = 1.0 + 0.4 * excessBytes / m_transferBuffer.Length;

            // Move the CPU sound buffer data to the device's buffer.
            var srcIndex = 0.0;
            while (srcIndex < m_cpuBuffer.Count && dstIndex < m_transferBuffer.Length)
            {
                m_transferBuffer[dstIndex++] = m_cpuBuffer[(int)srcIndex];
                srcIndex += speedUp;
            }
            
            m_cpuBuffer.RemoveRange(0, (int)srcIndex);
        }

        // Pad transfer buffer if necessary.
        for (var i = dstIndex; i < m_transferBuffer.Length; i++)
            m_transferBuffer[i] = m_lastWrittenSample;

        // Load the device buffer with data.
        AL.BufferData(bufferId, ALFormat.Mono8, m_transferBuffer, m_sampleRate);
        CheckSoundError();

        // Queue the device buffer for playback.
        AL.SourceQueueBuffer(m_source, bufferId);
        CheckSoundError();
    }
    
    public void AddSample(double sampleValue)
    {
        m_lastWrittenSample = (byte)(m_isSoundEnabled ? sampleValue * byte.MaxValue : 0);
        lock (m_cpuBuffer)
            m_cpuBuffer.Add(m_lastWrittenSample);
    }

    public void SetEnabled(bool isSoundEnabled)
    {
        if (m_isSoundEnabled == isSoundEnabled)
            return;
        m_isSoundEnabled = isSoundEnabled;
        ClearCpuBuffer();
    }
    
    private void ClearCpuBuffer()
    {
        lock (m_cpuBuffer)
            m_cpuBuffer.Clear();
        m_lastWrittenSample = 0;
    }
}
