using System.Text.Json;

namespace SnakeGame;

internal sealed class FileHighScoreStore(string filePath, ILogger logger) : IHighScoreStore
{
    private readonly string _filePath = filePath;
    private readonly ILogger _logger = logger;

    public int LoadBestScore()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return 0;
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<HighScoreData>(json);
            return data?.BestScore ?? 0;
        }
        catch (FileNotFoundException)
        {
            // File not found on first run - expected, return default
            return 0;
        }
        catch (JsonException ex)
        {
            // High score file corrupted, reset
            _logger.Warn("High score file corrupted, resetting to default", ex);
            return 0;
        }
        catch (IOException ex)
        {
            // File access issues (permission, disk full, etc.)
            _logger.Warn("Failed to read high score file", ex);
            return 0;
        }
    }

    public void SaveBestScore(int score)
    {
        try
        {
            var folder = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var data = new HighScoreData(score);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
            _logger.Debug($"Successfully saved high score: {score}");
        }
        catch (UnauthorizedAccessException ex)
        {
            // Permission denied
            _logger.Warn("Permission denied while saving high score", ex);
            // Save failures should not break gameplay
        }
        catch (IOException ex)
        {
            // Other file access issues (disk full, etc.)
            _logger.Warn("Failed to save high score", ex);
            // Save failures should not break gameplay
        }
    }

    private sealed record HighScoreData(int BestScore);
}
