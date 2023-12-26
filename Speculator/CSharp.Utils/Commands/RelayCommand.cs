// -----------------------------------------------------------------------
//  <copyright file="RelayCommand.cs" company="Dean Edis">
//      Copyright (c) 2023 Dean Edis. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Dean Edis. does not warrant or make any representations regarding the use or
//  results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

namespace CSharp.Utils.Commands;

public class RelayCommand : CommandBase
{
    private readonly Action<object> m_execute;
    private readonly Func<bool> m_canExecute;

    public RelayCommand(Action<object> execute, Func<bool> canExecute = null)
    {
        m_execute = execute ?? throw new ArgumentNullException(nameof(execute));
        m_canExecute = canExecute;
    }
    
    public override bool CanExecute(object parameter) => m_canExecute == null || m_canExecute();

    public override void Execute(object parameter) => m_execute(parameter);
}