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
using CSharp.Core;
using CSharp.Core.Extensions;
using CSharp.Core.ViewModels;

namespace Speculator.Core;

/// <summary>
/// Periodically snapshot the machine state to build a history that can be cycled through.
/// </summary>
public class CpuHistory : ViewModelBase
{
    private readonly ZxFileIo m_zxFileIo;
    private const int MaxSamples = 240;
    private const int TicksPerSample = (int)CPU.TStatesPerSecond;
    private readonly CompressedDataStore<long> m_snapshots = new CompressedDataStore<long>();
    private long m_ticksToNextSample;
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

            using var tempSna = new TempFile(".sna").WriteAllBytes(m_snapshots.At(m_indexToRestore));
            var memory = new Memory();
            ZxFileIo.LoadSna(tempSna, new CPU(memory), out var borderAttr);
            return ZxDisplay.CaptureScreenFromMemory(memory, borderAttr);
        }
    }

    public CpuHistory(CPU theCpu, ZxFileIo zxFileIo)
    {
        m_zxFileIo = zxFileIo;
        TheCpu = theCpu;
        theCpu.Ticked += OnCpuTicked;
        m_ticksToNextSample = TicksPerSample * 2; // Start 2 seconds after machine start.

        zxFileIo.RomLoaded += (_, romType) =>
        {
            if (romType == ZxFileIo.RomType.Game)
                return;
            m_snapshots.Clear();
            IndexToRestore = 0;
            m_ticksToNextSample = TicksPerSample;
        };
    }

    private void OnCpuTicked(object sender, (int elapsedTicks, ushort prevPC, ushort currentPC) args)
    {
        m_ticksToNextSample -= args.elapsedTicks;
        if (m_ticksToNextSample > 0)
            return; // Not yet time for an action to be triggered.
        m_ticksToNextSample += TicksPerSample;
        
        // Sample CPU state.
        using var snapshot = new MemoryStream(49179);
        m_zxFileIo.WriteSnaToStream(snapshot);
        m_snapshots.Add(TheCpu.TStatesSinceCpuStart, snapshot.GetBuffer());
        
        // Trim the total number of snapshots.
        while (m_snapshots.Count > MaxSamples)
            m_snapshots.RemoveAt(0);
        
        OnPropertyChanged(nameof(LastSampleIndex));
        IndexToRestore = LastSampleIndex;
        OnPropertyChanged(nameof(CanRestore));
    }

    public void Rollback()
    {
        // Restore snapshot.
        using (var snaFile = new TempFile(".sna").WriteAllBytes(m_snapshots.At(IndexToRestore)))
            m_zxFileIo.LoadFile(snaFile);
        TheCpu.SetTStatesSinceCpuStart(m_snapshots.Keys.ElementAt(IndexToRestore));
        m_ticksToNextSample = TicksPerSample;

        // Delete all snapshots from this point.
        while (m_snapshots.Count > IndexToRestore)
            m_snapshots.RemoveAt(m_snapshots.Count - 1);
    }

    public void RollbackByTime(int goBackSecs)
    {
        if (m_snapshots.Count == 0)
            return;

        IndexToRestore = Math.Max(0, m_snapshots.Count - goBackSecs);
        Rollback();
    }
}