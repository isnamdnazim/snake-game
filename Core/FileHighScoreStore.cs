using System.Text.Json;

namespace SnakeGame;

internal sealed class FileHighScoreStore(string filePath) : IHighScoreStore
{
    private readonly string _filePath = filePath;

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
            System.Diagnostics.Debug.WriteLine($"High score file corrupted: {ex.Message}");
            return 0;
        }
        catch (IOException ex)
        {
            // File access issues (permission, disk full, etc.)
            System.Diagnostics.Debug.WriteLine($"Failed to read high score file: {ex.Message}");
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
        }
        catch (UnauthorizedAccessException ex)
        {
            // Permission denied
            System.Diagnostics.Debug.WriteLine($"Permission denied saving high score: {ex.Message}");
        }
        catch (IOException ex)
        {
            // Other file access issues
            System.Diagnostics.Debug.WriteLine($"Failed to save high score: {ex.Message}");
        }
        // Save failures should not break gameplay - silently continue
    }

    private sealed record HighScoreData(int BestScore);
}
