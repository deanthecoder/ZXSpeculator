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

namespace CSharp.Core;

/// <summary>
/// Contains a collection of objects that will be disposed at
/// app shutdown (Temp files, etc).
/// </summary>
public class LazyDisposer : IDisposable
{
    private readonly List<IDisposable> m_objects = new List<IDisposable>();
    
    public static LazyDisposer Instance { get; } = new LazyDisposer();

    public void Dispose()
    {
        lock (m_objects)
        {
            m_objects.ForEach(o =>
            {
                try
                {
                    o.Dispose();
                }
                catch
                {
                    // Continue.
                }
            });
        }
    }
    
    public void Add(IDisposable o)
    {
        lock (m_objects)
            m_objects.Add(o);
    }
}