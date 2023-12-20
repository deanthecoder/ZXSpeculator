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

using System.Diagnostics;

namespace Speculator.Core;

public class ClockSync
{
    private readonly Stopwatch m_realTime;
    private readonly double m_emulatedTicksPerSecond;
    
    /// <summary>
    /// Number of T states when this stopwatch was started.
    /// </summary>
    private long m_tStateCountAtStart;

    public ClockSync(double emulatedCpuMHz)
    {
        m_realTime = Stopwatch.StartNew();
        m_emulatedTicksPerSecond = emulatedCpuMHz;
    }

    /// <summary>
    /// Operations external to emulation (such as loading a ROM) should pause
    /// emulated machine whilst they're 'busy'.
    /// </summary>
    public Pauser CreatePauser() => new Pauser(m_realTime);

    public void SyncWithRealTime(Func<long> ticksSinceCpuStart)
    {
        lock (m_realTime)
        {
            var emulatedUptimeSecs = (ticksSinceCpuStart() - m_tStateCountAtStart) / m_emulatedTicksPerSecond;
            var targetRealElapsedTicks = Stopwatch.Frequency * emulatedUptimeSecs;

            while (m_realTime.ElapsedTicks < targetRealElapsedTicks)
            {
                // Absolutely nothing.
                Thread.Sleep(0);
            }
        }
    }

    public void Reset(long ticksSinceCpuStart)
    {
        lock (m_realTime)
        {
            m_realTime.Restart();
            m_tStateCountAtStart = ticksSinceCpuStart;
        }
    }

    public class Pauser : IDisposable
    {
        private readonly Stopwatch m_stopwatch;
        
        public Pauser(Stopwatch stopwatch) // todo - Pause stopwatch, and set m_tStateCountAtStart?
        {
            m_stopwatch = stopwatch;
            stopwatch.Stop();
        }

        public void Dispose() => m_stopwatch.Start();
    }
}