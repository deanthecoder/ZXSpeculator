namespace CSharp.Utils;

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
}