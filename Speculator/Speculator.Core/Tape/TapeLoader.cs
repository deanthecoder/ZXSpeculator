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

using CSharp.Utils.Extensions;

namespace Speculator.Core.Tape;

public class TapeLoader
{
    private int m_currentTapeBlockIndex;
    private List<TapeBlock> m_tapeBlocks;
    private FileInfo m_tapeFile;

    public CPU TheCpu { get; set; }

    public void Load(FileInfo tapeFile)
    {
        m_tapeBlocks = null;
        m_tapeFile = tapeFile;
    }
    
    public bool? GetTapeSignal()
    {
        if (m_tapeFile?.Exists != true)
        {
            // No tape.
            m_tapeBlocks = null;
            return null;
        }

        // Load tape data.
        if (m_tapeBlocks == null)
        {
            var tapeBytes = m_tapeFile.ReadAllBytes();
            m_tapeBlocks = new List<TapeBlock>();
            var i = 0;
            while (i < tapeBytes.Length)
            {
                var blockSize = tapeBytes[i++] + (tapeBytes[i++] << 8);
                m_tapeBlocks.Add(new TapeBlock(tapeBytes, i, blockSize));

                i += blockSize;
            }

            m_currentTapeBlockIndex = 0;
        }

        // Get current tape block and sure its 'tones' are populated.
        bool? signal = null;
        while (!signal.HasValue && m_tapeFile != null)
        {
            signal = m_tapeBlocks[m_currentTapeBlockIndex]
                .PopulateTones(TheCpu.TStatesSinceCpuStart)
                .GetSignal(TheCpu.TStatesSinceCpuStart);
            
            if (signal != null)
                continue; // We have a signal.
            
            // End of block reached - Move to the next.
            m_currentTapeBlockIndex++;
            if (m_currentTapeBlockIndex < m_tapeBlocks.Count)
                continue;
            
            // No more tape!
            m_tapeFile = null;
            return false;
        }

        return signal;
    }
}