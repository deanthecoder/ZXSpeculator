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

using System.Diagnostics;

namespace Speculator.Core;

public class ClockSync
{
    private readonly Stopwatch m_realTime;
    private readonly double m_emulatedTicksPerSecond;
    private readonly Func<long> m_ticksSinceCpuStart;

    /// <summary>
    /// Number of T states when this stopwatch was started.
    /// </summary>
    private long m_tStateCountAtStart;

    public ClockSync(double emulatedCpuMHz, Func<long> ticksSinceCpuStart)
    {
        m_realTime = Stopwatch.StartNew();
        m_emulatedTicksPerSecond = emulatedCpuMHz;
        m_ticksSinceCpuStart = ticksSinceCpuStart;
    }

    /// <summary>
    /// Operations external to emulation (such as loading a ROM) should pause
    /// emulated machine whilst they're 'busy'.
    /// </summary>
    public Pauser CreatePauser() => new Pauser(m_realTime);

    public void SyncWithRealTime()
    {
        lock (m_realTime)
        {
            var emulatedUptimeSecs = (m_ticksSinceCpuStart() - m_tStateCountAtStart) / m_emulatedTicksPerSecond;
            var targetRealElapsedTicks = Stopwatch.Frequency * emulatedUptimeSecs;

            while (m_realTime.ElapsedTicks < targetRealElapsedTicks)
            {
                // Absolutely nothing.
                Thread.Sleep(0);
            }
        }
    }

    public void Reset()
    {
        lock (m_realTime)
        {
            m_realTime.Restart();
            m_tStateCountAtStart = m_ticksSinceCpuStart();
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
