namespace SnakeGame;

/// <summary>
/// Logger that forwards messages to multiple logger implementations.
/// </summary>
internal sealed class CompositeLogger(params ILogger[] loggers) : ILogger
{
    private readonly IReadOnlyList<ILogger> _loggers = loggers;

    public void Debug(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.Debug(message);
        }
    }

    public void Info(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.Info(message);
        }
    }

    public void Warn(string message, Exception? ex = null)
    {
        foreach (var logger in _loggers)
        {
            logger.Warn(message, ex);
        }
    }

    public void Error(string message, Exception ex)
    {
        foreach (var logger in _loggers)
        {
            logger.Error(message, ex);
        }
    }
}