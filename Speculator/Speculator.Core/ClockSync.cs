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

    public ClockSync(double emulatedCpuMHz)
    {
        m_realTime = Stopwatch.StartNew();
        m_emulatedTicksPerSecond = emulatedCpuMHz;
    }

    public void SyncWithRealTime(long ticksSinceCpuStart)
    {
        var emulatedUptimeSecs = ticksSinceCpuStart / m_emulatedTicksPerSecond;
        var targetRealElapsedTicks = Stopwatch.Frequency * emulatedUptimeSecs;

        while (m_realTime.ElapsedTicks < targetRealElapsedTicks)
        {
            // Absolutely nothing.
            Thread.Sleep(0);
        }
    }
}