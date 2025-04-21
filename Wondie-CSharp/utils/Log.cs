namespace Wondie_CSharp.utils;

public class Log
{
    public static Task Info(string message)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} | INFO | {message}");
        return Task.CompletedTask;
    }

    public static Task Debug(string message)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} | DEBUG | {message}");
        return Task.CompletedTask;
    }

    public static Task Warn(string message)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} | WARN | {message}");
        return Task.CompletedTask;
    }

    public static Task Error(string message)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} | ERROR | {message}");
        return Task.CompletedTask;
    }
}