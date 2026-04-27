using System.Text;

namespace SnakeGame;

/// <summary>
/// Logger implementation that persists messages to rolling daily log files.
/// </summary>
internal sealed class FileLogger(string logDirectoryPath) : ILogger
{
    private readonly string _logDirectoryPath = logDirectoryPath;
    private readonly object _syncRoot = new();

    public void Debug(string message) => Write("DEBUG", message);

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message, Exception? ex = null)
    {
        var details = ex is null ? message : $"{message}{Environment.NewLine}{ex}";
        Write("WARN", details);
    }

    public void Error(string message, Exception ex)
    {
        Write("ERROR", $"{message}{Environment.NewLine}{ex}");
    }

    private void Write(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(_logDirectoryPath);
            var now = DateTime.Now;
            var filePath = Path.Combine(_logDirectoryPath, $"snakegame-{now:yyyyMMdd}.log");
            var line = $"[{now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";

            lock (_syncRoot)
            {
                File.AppendAllText(filePath, line, Encoding.UTF8);
            }
        }
        catch
        {
            // Logging must never fail the application flow.
        }
    }
}