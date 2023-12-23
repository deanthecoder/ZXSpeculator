#if !WINDOWS
using System.Collections.Concurrent;
using SoundIOSharp;

namespace Speculator.Core.HostDevices;

/// <summary>
/// A sound device to interface with the host machine's sound card.
/// </summary>
public class SoundDevice
{
    private readonly int m_sampleHz;
    private Action<IntPtr, double> m_writeSampleFunc;
    private readonly ConcurrentQueue<double> m_soundBuffer = new ConcurrentQueue<double>();
    private double m_lastSample;
    private int m_channelCount;
    private bool m_isSoundEnabled = true;

    public SoundDevice(int sampleHz)
    {
        m_sampleHz = sampleHz;
    }

    public void SoundLoop(Func<bool> isCancelled)
    {
        using var api = new SoundIO();
        api.Connect();
        api.FlushEvents();

        var device = api.GetOutputDevice(api.DefaultOutputDeviceIndex);
        if (device == null)
        {
            Console.Error.WriteLine("Output sound device not found.");
            return;
        }

        if (device.ProbeError != 0)
        {
            Console.Error.WriteLine("Cannot probe sound device.");
            return;
        }

        var outStream = device.CreateOutStream();

        outStream.WriteCallback = (min, max) => WriteCallback(outStream, min, max);
        outStream.SampleRate = m_sampleHz;
        outStream.ErrorCallback += () => Console.WriteLine("Sound error.");
        outStream.UnderflowCallback += () => Console.WriteLine("Sound buffer underflow.");

        if (device.SupportsFormat(SoundIODevice.Float32NE))
        {
            outStream.Format = SoundIODevice.Float32NE;
            m_writeSampleFunc = write_sample_float32ne;
        }
        else if (device.SupportsFormat(SoundIODevice.Float64NE))
        {
            outStream.Format = SoundIODevice.Float64NE;
            m_writeSampleFunc = write_sample_float64ne;
        }
        else if (device.SupportsFormat(SoundIODevice.S32NE))
        {
            outStream.Format = SoundIODevice.S32NE;
            m_writeSampleFunc = write_sample_s32ne;
        }
        else if (device.SupportsFormat(SoundIODevice.S16NE))
        {
            outStream.Format = SoundIODevice.S16NE;
            m_writeSampleFunc = write_sample_s16ne;
        }
        else
        {
            Console.Error.WriteLine("No suitable sound format available.");
            return;
        }

        outStream.Open();
            
        if (outStream.LayoutErrorMessage != null)
            Console.Error.WriteLine("Unable to set sound channel layout: " + outStream.LayoutErrorMessage);

        outStream.Start();
        m_channelCount = outStream.Layout.ChannelCount;

        while (!isCancelled())
            api.FlushEvents();

        outStream.Dispose();
        device.RemoveReference();
    }
    
    /// <summary>
    /// Called periodically by the sound library to send more data to the sound card.
    /// </summary>
    private void WriteCallback(SoundIOOutStream outStream, int frameCountMin, int frameCountMax)
    {
        var toWrite = frameCountMin;

        if (!m_isSoundEnabled)
            m_soundBuffer.Clear();

        if (m_soundBuffer.Count > toWrite)
            toWrite = Math.Min(m_soundBuffer.Count, frameCountMax);

        // Prepare to stream the buffer samples.
        var results = outStream.BeginWrite(ref toWrite);

        // Write real sample data.
        var toWriteFromBuffer = Math.Min(toWrite, m_soundBuffer.Count);
        int written;
        for (written = 0; written < toWriteFromBuffer; written++)
        {
            if (m_soundBuffer.TryDequeue(out var sample))
                WriteSample(results, sample);
        }

        // If there's a shortfall, pad by reusing the last known sample.
        while (written < toWrite)
        {
            WriteSample(results, m_lastSample);
            written++;
        }

        outStream.EndWrite();
    }
    
    private void WriteSample(SoundIOChannelAreas channelAreas, double sample)
    {
        for (var channel = 0; channel < m_channelCount; channel++)
        {
            m_lastSample = sample;
            var area = channelAreas.GetArea(channel);
            m_writeSampleFunc(area.Pointer, m_lastSample);
            area.Pointer += area.Step;
        }
    }

    private unsafe static void write_sample_s16ne(IntPtr ptr, double sample)
    {
        var buf = (short*)ptr;
        var range = short.MaxValue - (double)short.MinValue;
        var val = sample * range / 2.0;
        *buf = (short)val;
    }

    private unsafe static void write_sample_s32ne(IntPtr ptr, double sample)
    {
        var buf = (int*)ptr;
        var range = int.MaxValue - (double)int.MinValue;
        var val = sample * range / 2.0;
        *buf = (int)val;
    }

    private unsafe static void write_sample_float32ne(IntPtr ptr, double sample)
    {
        var buf = (float*)ptr;
        *buf = (float)sample;
    }

    private unsafe static void write_sample_float64ne(IntPtr ptr, double sample)
    {
        var buf = (double*)ptr;
        *buf = sample;
    }
    
    /// <summary>
    /// Append a sample (0.0 - 1.0) to the buffer.
    /// </summary>
    public void AddSample(double sampleValue)
    {
        m_soundBuffer.Enqueue(sampleValue);
    }
    
    public void SetEnabled(bool isSoundEnabled)
    {
        m_isSoundEnabled = isSoundEnabled;
    }
}
#endif