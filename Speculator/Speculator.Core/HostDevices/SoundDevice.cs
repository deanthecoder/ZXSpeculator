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

    /// <summary>
    /// Data received from the CPU, copied into m_transferBuffer for transfer to the sound card.
    /// </summary>
    private readonly List<byte> m_cpuBuffer = new List<byte>(8192);

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

        m_transferBuffer = new byte[m_sampleRate / (20 * BufferCount)];
    }

    public void SoundLoop(Func<bool> isCancelled)
    {
        // Wait for the first batch of sample data from the CPU.
        while (!isCancelled() && !IsEnoughDataFromCpuBuffered(BufferCount))
            Thread.Sleep(10);

        // Pre-fill all buffers with initial data.
        foreach (var bufferId in m_buffers)
            UpdateBufferData(bufferId);

        // Start playback.
        AL.SourcePlay(m_source);
        CheckSoundError();
        
        while (!isCancelled())
        {
            Thread.Sleep(10);
            
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
            
            AL.SourcePlay(m_source);
            CheckSoundError();
            ClearCpuBuffer();
        }

        AL.SourceStop(m_source);
    }
    
    private static void CheckSoundError()
    {
        var err = AL.GetError();
        if (err != ALError.NoError)
            Logger.Instance.Error($"Sound device error: {err}");
    }
    
    private bool IsEnoughDataFromCpuBuffered(int bufferCountToFill = 1) =>
        m_cpuBuffer.Count >= m_transferBuffer.Length * bufferCountToFill;

    private void UpdateBufferData(int bufferId)
    {
        lock (m_cpuBuffer)
        {
            // Pad CPU sound buffer if not filled to capacity.
            while (!IsEnoughDataFromCpuBuffered())
                m_cpuBuffer.Add((byte)(m_cpuBuffer.Count > 0 ? m_cpuBuffer[^1] : 0));
            
            // Move the CPU sound buffer data to the device's buffer.
            m_cpuBuffer.CopyTo(0, m_transferBuffer, 0, m_transferBuffer.Length);
            m_cpuBuffer.RemoveRange(0, m_transferBuffer.Length);
        }

        // Fill the device buffer with data.
        AL.BufferData(bufferId, ALFormat.Mono8, m_transferBuffer, m_sampleRate);
        CheckSoundError();

        // Queue the device buffer for playback.
        AL.SourceQueueBuffer(m_source, bufferId);
        CheckSoundError();
    }

    public void AddSample(double sampleValue)
    {
        var sample = (byte)(m_isSoundEnabled ? sampleValue * byte.MaxValue : 0);
        lock (m_cpuBuffer)
            m_cpuBuffer.Add(sample);
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
    }
}
