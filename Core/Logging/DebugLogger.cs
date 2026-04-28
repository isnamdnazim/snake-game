using System.Diagnostics;

namespace SnakeGame;

/// <summary>
/// Logger implementation using System.Diagnostics.Debug.
/// Outputs to Debug window in Visual Studio or Debug Output console.
/// </summary>
public sealed class DebugLogger : ILogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    public void Debug(string message) =>
        System.Diagnostics.Debug.WriteLine($"[DEBUG] {message}");

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void Info(string message) =>
        System.Diagnostics.Debug.WriteLine($"[INFO] {message}");

    /// <summary>
    /// Logs a warning message with optional exception.
    /// </summary>
    public void Warn(string message, Exception? ex = null) =>
        System.Diagnostics.Debug.WriteLine($"[WARN] {message}" + (ex != null ? $"\n{ex}" : ""));

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    public void Error(string message, Exception ex) =>
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message}\n{ex}");
}
