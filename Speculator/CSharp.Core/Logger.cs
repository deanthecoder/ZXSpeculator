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

public class Logger
{
    public enum Severity
    {
        Info,
        Warning,
        Error
    }
    
    public static Logger Instance { get; } = new Logger();

    public event EventHandler<(Severity, string Message)> Logged; 

    public void Info(Func<string> message) => Info(message());
    public void Warn(Func<string> message) => Warn(message());
    public void Error(Func<string> message) => Error(message());

    public void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Info: ");
        Console.ResetColor();
        Console.WriteLine(message);
        Logged?.Invoke(this, (Severity.Info, message));
    }
    
    public void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("Warn: ");
        Console.ResetColor();
        Console.WriteLine(message);
        Logged?.Invoke(this, (Severity.Warning, message));
    }
    
    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("Error: ");
        Console.ResetColor();
        Console.WriteLine(message);
        Logged?.Invoke(this, (Severity.Error, message));
    }
    
    public void Exception(string message, Exception exception) =>
        Error($"{message} ({exception.Message})");
}
