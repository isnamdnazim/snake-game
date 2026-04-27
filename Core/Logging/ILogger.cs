namespace SnakeGame;

/// <summary>
/// Logging interface for diagnostic purposes.
/// </summary>
internal interface ILogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    void Debug(string message);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    void Info(string message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    void Warn(string message, Exception? ex = null);

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    void Error(string message, Exception ex);
}
