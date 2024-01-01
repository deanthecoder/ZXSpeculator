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

namespace Speculator.Core.Tape;

/// <summary>
/// Stores an array of hi/lo pulses representing the sound of a single
/// .tap tape block.
/// </summary>
public class SoundBuffer
{
    public List<(bool level, long tStateLength, long tStateStart)> Levels { get; } = new();

    public void Add(bool level, long tStateLength)
    {
        long tStateStart;
        if (Levels.Any())
        {
            var prev = Levels.Last();
            tStateStart = prev.tStateStart + prev.tStateLength;
        }
        else
            tStateStart = 0;
        
        Levels.Add((level, tStateLength, tStateStart));
    }

    /// <summary>
    /// Comparer for binary-searching. 
    /// </summary>
    public class Comparer : IComparer<(bool level, long tStateLength, long tStateStart)>
    {
        public int Compare((bool level, long tStateLength, long tStateStart) x, (bool level, long tStateLength, long tStateStart) y)
        {
            return x.tStateStart.CompareTo(y.tStateStart);
        }
    }
}