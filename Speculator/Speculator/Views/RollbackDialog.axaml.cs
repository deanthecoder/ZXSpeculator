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

using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Speculator.Core;

namespace Speculator.Views;

public partial class RollbackDialog : UserControl
{
    private IDisposable m_cpuPauser;
    private CpuHistory m_cpuHistory;

    public RollbackDialog()
    {
        InitializeComponent();
        
        PropertyChanged += (_, args) =>
        {
            if (args.Property.Name != nameof(DataContext))
                return;
            
            if (DataContext != null)
            {
                // Dialog opened.
                m_cpuHistory = (CpuHistory)DataContext;
                if (m_cpuHistory == null)
                    return; // We're in the designer.
                    
                m_cpuPauser = m_cpuHistory.TheCpu.ClockSync.CreatePauser();
                Monitor.Enter(m_cpuHistory.TheCpu.CpuStepLock);
            }
            else
            {
                if (m_cpuHistory == null)
                    return; // We're in the designer.
                
                // Dialog closed.
                m_cpuPauser.Dispose();
                Monitor.Exit(m_cpuHistory.TheCpu.CpuStepLock);
            }
        };
    }
    
    private void OnRollback(object sender, RoutedEventArgs e) =>
        m_cpuHistory?.Rollback();
}