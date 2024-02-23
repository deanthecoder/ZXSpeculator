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
    private readonly Func<long> m_resetCpuTicks;
    private Speed m_speed = Speed.Actual;

    /// <summary>
    /// Number of T states when this stopwatch was started.
    /// </summary>
    private long m_tStateCountAtStart;
    
    public enum Speed { Actual, Fast, Maximum, Pause }

    public ClockSync(double emulatedCpuMHz, Func<long> ticksSinceCpuStart, Func<long> resetCpuTicks)
    {
        m_realTime = Stopwatch.StartNew();
        m_emulatedTicksPerSecond = emulatedCpuMHz;
        m_ticksSinceCpuStart = ticksSinceCpuStart;
        m_resetCpuTicks = resetCpuTicks;
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
    public void SetSpeed(Speed speed)
    {
        lock (m_realTime)
        {
            if (m_speed == speed)
                return;
            m_speed = speed;
            
            // Reset the timing variables when re-enabling 100% emulated peed.
            m_tStateCountAtStart = m_ticksSinceCpuStart();
            m_realTime.Restart();
        }
    }

    public void SyncWithRealTime()
    {
        lock (m_realTime)
        {
            var emulatedUptimeSecs = (m_ticksSinceCpuStart() - m_tStateCountAtStart) / m_emulatedTicksPerSecond;

            switch (m_speed)
            {
                case Speed.Actual:
                    // No change required.
                    break;
                case Speed.Fast:
                    emulatedUptimeSecs *= 0.66;
                    break;
                case Speed.Maximum:
                    // Don't delay.
                    return;
                case Speed.Pause:
                    // Not quite paused, but veeeery slow.
                    Thread.Sleep(50);
                    return;
            }

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
            m_tStateCountAtStart = 0;
            m_resetCpuTicks();
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
