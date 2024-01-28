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

using Avalonia.Media.Imaging;
using CSharp.Utils;
using CSharp.Utils.Extensions;
using CSharp.Utils.ViewModels;

namespace Speculator.Core;

/// <summary>
/// Periodically snapshot the machine state to build a history that can be cycled through.
/// </summary>
public class CpuHistory : ViewModelBase, IDisposable
{
    private const int SamplesPerSecond = 2;
    private readonly ZxFileIo m_zxFileIo;
    private const int MaxSamples = 120;
    private readonly long m_ticksPerSample;
    private readonly List<FileInfo> m_snapshots = new List<FileInfo>();
    private long m_ticksToNextSample;
    private long m_lastTStateCount;
    private int m_indexToRestore;

    public CPU TheCpu { get; }
    public int LastSampleIndex => m_snapshots.Count - 1;
    public bool CanRestore => LastSampleIndex >= 0 && IndexToRestore < LastSampleIndex;
    
    public int IndexToRestore
    {
        get => m_indexToRestore;
        set
        {
            if (!SetField(ref m_indexToRestore, value))
                return;
            OnPropertyChanged(nameof(CanRestore));
            OnPropertyChanged(nameof(ScreenPreview));
        }
    }

    public Bitmap ScreenPreview
    {
        get
        {
            if (m_snapshots.Count == 0)
                return null;
            
            var memory = new Memory();
            ZxFileIo.LoadSna(m_snapshots[m_indexToRestore], new CPU(memory), out var borderAttr);
            return ZxDisplay.CaptureScreenFromMemory(memory, borderAttr);
        }
    }

    public CpuHistory(CPU theCpu, ZxFileIo zxFileIo)
    {
        m_zxFileIo = zxFileIo;
        TheCpu = theCpu;
        theCpu.Ticked += OnCpuTicked;
        m_ticksPerSample = (long)(CPU.TStatesPerSecond / SamplesPerSecond); // Two samples per second.
        m_ticksToNextSample = m_ticksPerSample * 5; // Start 5 seconds after machine start.
        m_lastTStateCount = 0;
    }

    private void OnCpuTicked(object sender, (ushort prevPC, ushort currentPC) e)
    {
        var advance = TheCpu.TStatesSinceCpuStart - m_lastTStateCount;
        m_lastTStateCount = TheCpu.TStatesSinceCpuStart;

        m_ticksToNextSample -= advance;

        if (m_ticksToNextSample > 0)
            return; // Not yet time for an action to be triggered.
        m_ticksToNextSample += m_ticksPerSample;

        // Sample CPU state.
        var snapshot = new TempFile(".sna");
        m_zxFileIo.SaveSna(snapshot);
        m_snapshots.Add(snapshot);
        
        // Trim the total number of snapshots.
        while (m_snapshots.Count > MaxSamples)
            RemoveSnapshot(m_snapshots[0]);
        
        OnPropertyChanged(nameof(LastSampleIndex));
        IndexToRestore = LastSampleIndex;
        OnPropertyChanged(nameof(CanRestore));
    }

    public void Rollback()
    {
        // Restore snapshot.
        m_zxFileIo.LoadFile(m_snapshots[IndexToRestore]);
        
        // Delete all snapshots from this point.
        while (m_snapshots.Count > IndexToRestore)
            RemoveSnapshot(m_snapshots.Last());
    }

    public void RollbackByTime(int goBackSecs)
    {
        if (m_snapshots.Count == 0)
            return;
        
        var snapshotCount = goBackSecs * SamplesPerSecond;
        IndexToRestore = Math.Max(0, m_snapshots.Count - snapshotCount);
        Rollback();
    }
    
    private void RemoveSnapshot(FileInfo toRemove)
    {
        toRemove.TryDelete();
        m_snapshots.Remove(toRemove);
    }

    public void Dispose()
    {
        while (m_snapshots.Any())
            RemoveSnapshot(m_snapshots[0]);
    }
}