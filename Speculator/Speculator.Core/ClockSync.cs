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
    private bool m_isSpeedLimited = true;

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
    public IDisposable CreatePauser() => new Pauser(m_realTime);

    /// <summary>
    /// Call to set whether the emulator is running at 100% emulated speed,
    /// or 'full throttle'.
    /// </summary>
    public void SetLimitSpeed(bool enableSpeedLimit)
    {
        lock (m_realTime)
        {
            if (m_isSpeedLimited == enableSpeedLimit)
                return;
            m_isSpeedLimited = enableSpeedLimit;
            if (!m_isSpeedLimited)
                return;
            
            // Reset the timing variables when re-enabling 100% emulated peed.
            m_tStateCountAtStart = m_ticksSinceCpuStart();
            m_realTime.Restart();
        }
    }

    public void SyncWithRealTime()
    {
        if (!m_isSpeedLimited)
            return;
        
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

    private class Pauser : IDisposable
    {
        private readonly Stopwatch m_stopwatch;
        
        public Pauser(Stopwatch stopwatch)
        {
            m_stopwatch = stopwatch;
            stopwatch.Stop();
        }

        public void Dispose() => m_stopwatch.Start();
    }
}
