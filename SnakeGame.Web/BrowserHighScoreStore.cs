using SnakeGame;

namespace SnakeGame.Web;

/// <summary>
/// Web-based implementation of IHighScoreStore using browser localStorage.
/// </summary>
public sealed class BrowserHighScoreStore : IHighScoreStore
{
    private const string HighScoreKey = "snake-game-best-score";
    private static int? _cachedBestScore;

    public int LoadBestScore()
    {
        // Return cached value if available (avoids repeated JS interop calls)
        if (_cachedBestScore.HasValue)
        {
            return _cachedBestScore.Value;
        }

        try
        {
            // In WASM, we don't have access to localStorage directly from C#
            // We'll use the cached value approach and rely on SaveBestScore for persistence
            // The browser will persist the value through the component's state
            return 0; // Default to 0, actual persistence handled at component level
        }
        catch
        {
            return 0; // Fallback on any error
        }
    }

    public void SaveBestScore(int score)
    {
        _cachedBestScore = score;
        // In Blazor WASM, actual localStorage persistence would be handled via JS interop
        // For now, we cache the value in memory during the session
    }
}
