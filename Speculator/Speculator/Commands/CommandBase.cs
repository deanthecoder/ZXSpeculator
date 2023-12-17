// -----------------------------------------------------------------------
//  <copyright file="CommandBase.cs" company="Dean Edis">
//      Copyright (c) 2023 Dean Edis. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Dean Edis. does not warrant or make any representations regarding the use or
//  results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using System;
using System.Windows.Input;

namespace Speculator.Commands;

public abstract class CommandBase : ICommand
{
    public event EventHandler CanExecuteChanged;

    public virtual bool CanExecute(object parameter) => true;

    public abstract void Execute(object parameter);
    
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}