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

using System.Text;
using CSharp.Core;

namespace Speculator.Core.Tape;

/// <summary>
/// Represents the sound/tone data in a single tape block.
/// </summary>
public class TapeBlock
{
    private const int TStatesPerBitZero = 855;
    private const int TStatesPerBitOne = 1710;

    private readonly byte[] m_blockBytes;
    private SoundBuffer m_soundBuffer;
    private long m_tStatesAtBlockStart;
    private readonly SoundBuffer.Comparer m_bufferComparer = new SoundBuffer.Comparer();

    public TapeBlock(byte[] tapeBytes, int blockStartIndex, int blockSize)
    {
        m_blockBytes = new byte[blockSize];
        Array.Copy(tapeBytes, blockStartIndex, m_blockBytes, 0, blockSize);
    }
    
    public TapeBlock PopulateTones(long tStatesFromCpuStart)
    {
        if (m_soundBuffer != null)
            return this; // Already populated.

        m_tStatesAtBlockStart = tStatesFromCpuStart;
        Logger.Instance.Info($"Loading tape block: {this}");
        m_soundBuffer = new SoundBuffer();
        
        // Is this header or data?
        var isHeader = m_blockBytes.Length == 19;
        
        // Create leading tones.
        var signalState = true;
        int pulses;
        if (isHeader)
        {
            // Header block.
            pulses = 8063;
        }
        else
        {
            // Data block.
            pulses = 3223;
        }

        for (var i = 0; i < pulses; i++)
        {
            m_soundBuffer.Add(signalState, 2168);
            signalState = !signalState;
        }

        if (!signalState)
            m_soundBuffer.Add(false, 2168);
        m_soundBuffer.Add(true, 667);
        m_soundBuffer.Add(false, 735);

        // Create tape tones.
        m_soundBuffer.ReserveExtra(m_blockBytes.Length * 8);
        foreach (var blockByte in m_blockBytes)
        {
            for (var mask = 0x80; mask > 0; mask >>= 1)
            {
                var isBitSet = (blockByte & mask) != 0;
                var pulseLength = isBitSet ? TStatesPerBitOne : TStatesPerBitZero;
                
                m_soundBuffer.Add(true, pulseLength);
                m_soundBuffer.Add(false, pulseLength);
            }
        }
        
        // Pause.
        m_soundBuffer.Add(true, 954);

        return this;
    }

    /// <summary>
    /// Find the low/high signal value at the specified T-state time.
    /// </summary>
    public bool? GetSignal(long tStatesFromCpuStart)
    {
        var tStatesFromBlockStart = tStatesFromCpuStart - m_tStatesAtBlockStart;

        var searchItem = (false, 0L, tStatesFromBlockStart);
        var index = m_soundBuffer.Levels.BinarySearch(searchItem, m_bufferComparer);

        if (index < 0)
        {
            index = ~index; // Get the index of the first element greater than tStatesFromBlockStart.
            if (index < m_soundBuffer.Levels.Count)
            {
                return m_soundBuffer.Levels[index].level;
            }
        }
        else
        {
            // Exact match found, return the level of the current index.
            return m_soundBuffer.Levels[index].level;
        }

        // No more tones in this block.
        m_soundBuffer = null;
        return null;
    }

    public override string ToString()
    {
        if (m_blockBytes.Length == 19)
        {
            var blockName = Encoding.ASCII.GetString(m_blockBytes, 2, 10).TrimEnd();
            var dataLength = m_blockBytes[12] + (m_blockBytes[13] << 8);
            switch (m_blockBytes[1])
            {
                case 0: return $"Program header: {blockName}";
                case 1: return $"Numerics header: {blockName}";
                case 2: return $"Alpha-numerics header: {blockName}";
                case 3: return dataLength == 6912 ? $"SCREEN$ header: {blockName}" : $"Byte header: {blockName}";
            }
        }
        
        return $"Data: {m_blockBytes.Length} bytes";
    }
}