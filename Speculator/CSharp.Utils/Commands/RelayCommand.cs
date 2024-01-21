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

namespace CSharp.Utils.Commands;

public class RelayCommand : CommandBase
{
    private readonly Action<object> m_execute;
    private readonly Func<object, bool> m_canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        m_execute = execute ?? throw new ArgumentNullException(nameof(execute));
        m_canExecute = canExecute;
    }
    
    public override bool CanExecute(object parameter) =>
        m_canExecute?.Invoke(parameter) != false;

    public override void Execute(object parameter) =>
        m_execute(parameter);
}
