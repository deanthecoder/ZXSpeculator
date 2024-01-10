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

namespace CSharp.Utils;

/// <summary>
/// The ActionConsolidator class is designed to manage and throttle the execution of an Action delegate.
/// It ensures that the action is not invoked too frequently. This is particularly useful for scenarios
/// where an action might be triggered multiple times in a short period (e.g., handling UI events),
/// but executing the action every time is unnecessary or inefficient.
/// </summary>
/// <remarks>
/// When the Invoke method is called, the class checks the time elapsed since the last invocation.
/// - If more than 0.1 seconds have passed, the action is invoked immediately.
/// - If less than 0.1 seconds have passed, the invocation is delayed until 0.2 seconds after the
///   last invocation request. This is managed through a timer to ensure that the action is eventually
///   executed if no further requests are received.
/// This class is thread-safe, ensuring proper behavior even when accessed from multiple threads.
/// </remarks>
public class ActionConsolidator : IDisposable
{
    private readonly Action m_action;
    private readonly Timer m_timer;
    private readonly object m_lock = new object();
    private DateTime? m_lastInvokeTime;

    public ActionConsolidator(Action action)
    {
        m_action = action;
        m_timer = new Timer(_ => InvokeNow(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Invoke()
    {
        lock (m_lock)
        {
            if (m_lastInvokeTime == null || DateTime.Now - m_lastInvokeTime > TimeSpan.FromSeconds(0.1))
            {
                // Directly invoke if enough time has elapsed or this is the first request.
                InvokeNow();
            }
            else
            {
                // Reset the timer for the last invoke.
                m_timer.Change(TimeSpan.FromSeconds(0.2), Timeout.InfiniteTimeSpan);
            }
        }
    }

    private void InvokeNow()
    {
        lock (m_lock)
        {
            m_action.Invoke();
            m_lastInvokeTime = DateTime.Now;
            m_timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer
        }
    }
        
    public void Dispose() => m_timer?.Dispose();
}