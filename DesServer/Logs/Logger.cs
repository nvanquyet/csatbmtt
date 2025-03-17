using DesServer.AppSetting;
using Shared.AppSettings;

namespace DesServer.Logs;

public static class Logger
{
    private static readonly string LogFilePath = $"server_log_{DateTime.Now:yyyyMMdd}.txt";
    private static readonly Lock Lock = new();

    public static void Log(string message)
    {
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        
        lock (Lock)
        {
            File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
        }

        if(Config.LogToConsole) Console.WriteLine(logEntry);
    }
}