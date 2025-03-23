using Shared.AppSettings;

namespace Shared;

public static class Logger
{
    private const string LogDirectory = "Logs";

    public static void LogInfo(string message)
    {
        if (!Config.LogToConsole) Log("[INFO]", message);
        else Console.WriteLine($"[INFO] {message}");
    }

    public static void LogWarning(string message)
    {
        if (!Config.LogToConsole)Log("[WARNING]", message);
        else Console.WriteLine($"[WARNING] {message}");
    }

    public static void LogError(string message)
    {
        if (!Config.LogToConsole) Log("[ERROR]", message);
        else Console.WriteLine($"[ERROR] {message}");
    }

    private static void Log(string status, string message)
    {
        try
        {
            // Đảm bảo thư mục Logs tồn tại
            Directory.CreateDirectory(LogDirectory);
            
            // Tạo tên file theo ngày hiện tại
            var logFileName = $"Log-{DateTime.Now:yyyyMMdd}.log";
            var logFilePath = Path.Combine(LogDirectory, logFileName);
            
            // Ghi log vào file
            using StreamWriter sw = File.AppendText(logFilePath);
            sw.WriteLine($"{status} {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }
        catch (Exception ex)
        {
            // Xử lý lỗi nếu cần
            Console.WriteLine($"Lỗi khi ghi log: {ex.Message}");
        }
    }
}