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
/// Creates a Task which triggers an action at regular intervals. 
/// </summary>
public class PeriodicAction : IDisposable
{
    private readonly TimeSpan m_period;
    private readonly Action m_action;
    private readonly CancellationTokenSource m_tokenSource;
    private Task m_task;

    /// <summary>
    /// Construct the object in a 'stopped' state.
    /// </summary>
    public PeriodicAction(TimeSpan period, Action action)
    {
        m_period = period;
        m_action = action;
        m_tokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Construct the object in a 'started' state.
    /// </summary>
    public static PeriodicAction Start(TimeSpan period, Action action) =>
        new PeriodicAction(period, action).Start();
    
    public PeriodicAction Start()
    {
        m_task ??= Task.Run(() =>
        {
            try
            {
                while (true)
                {
                    m_action();
                    Thread.Sleep((int)m_period.TotalMilliseconds);
                }
            }
            catch (TaskCanceledException)
            {
                // This is ok.
            }
        }, m_tokenSource.Token);
        return this;
    }

    public void Stop()
    {
        m_tokenSource.Cancel();
        m_task?.Dispose();
        m_task = null;
    }
    
    public void Dispose()
    {
        Stop();
        m_tokenSource?.Dispose();
    }
}