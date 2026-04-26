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
        catch
        {
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
        catch
        {
            // Intentionally no-op. Save failures should not break gameplay.
        }
    }

    private sealed record HighScoreData(int BestScore);
}
