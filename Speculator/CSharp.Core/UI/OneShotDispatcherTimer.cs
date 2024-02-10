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

using Avalonia.Threading;

namespace CSharp.Core.UI;

/// <summary>
/// Trigger a single delayed method callback using the Dispatcher.
/// </summary>
/// <remarks>
/// Constructing the class will not start the timer. You must either call Start()
/// or OneShotDispatcherTimer.CreateAndStart().
/// </remarks>
public class OneShotDispatcherTimer
{
    private readonly DispatcherTimer m_timer;

    public static OneShotDispatcherTimer CreateAndStart(TimeSpan delay, Action action) =>
        new OneShotDispatcherTimer(delay, action).Start();

    public OneShotDispatcherTimer(TimeSpan delay, Action action)
    {
        m_timer = new DispatcherTimer
        {
            Interval = delay
        };

        m_timer.Tick += (_, _) =>
        {
            m_timer.Stop();
            action.Invoke();
        };
    }

    public OneShotDispatcherTimer Start()
    {
        m_timer.Start();
        return this;
    }
}