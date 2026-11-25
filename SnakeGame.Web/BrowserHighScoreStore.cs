using System.Globalization;
using Microsoft.JSInterop;
using SnakeGame;

namespace SnakeGame.Web;

/// <summary>
/// Web-based implementation of <see cref="IHighScoreStore"/> backed by browser <c>localStorage</c>.
/// </summary>
public sealed class BrowserHighScoreStore(IJSRuntime jsRuntime) : IHighScoreStore
{
    private const string HighScoreKey = "snake-game-best-score";
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private int? _cachedBestScore;

    public int LoadBestScore()
    {
        if (_cachedBestScore.HasValue)
        {
            return _cachedBestScore.Value;
        }

        if (_jsRuntime is IJSInProcessRuntime inProcessJs)
        {
            try
            {
                var raw = inProcessJs.Invoke<string?>("localStorage.getItem", HighScoreKey);
                if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0)
                {
                    _cachedBestScore = parsed;
                    return parsed;
                }
            }
            catch (JSException)
            {
                // Storage may be unavailable in restricted/private browser contexts.
            }
            catch (InvalidOperationException)
            {
                // JS interop not available yet.
            }
        }

        return _cachedBestScore ?? 0;
    }

    public void SaveBestScore(int score)
    {
        var normalized = Math.Max(0, score);
        _cachedBestScore = normalized;

        if (_jsRuntime is IJSInProcessRuntime inProcessJs)
        {
            try
            {
                inProcessJs.InvokeVoid(
                    "localStorage.setItem",
                    HighScoreKey,
                    normalized.ToString(CultureInfo.InvariantCulture));
            }
            catch (JSException)
            {
                // Keep in-memory fallback if browser blocks storage writes.
            }
            catch (InvalidOperationException)
            {
                // JS interop not available yet.
            }
        }
    }

    public async ValueTask<int> LoadBestScoreAsync()
    {
        if (_cachedBestScore.HasValue)
        {
            return _cachedBestScore.Value;
        }

        try
        {
            var raw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", HighScoreKey);
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0)
            {
                _cachedBestScore = parsed;
                return parsed;
            }
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        return _cachedBestScore ?? 0;
    }

    public async ValueTask SaveBestScoreAsync(int score)
    {
        var normalized = Math.Max(0, score);
        _cachedBestScore = normalized;

        try
        {
            await _jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                HighScoreKey,
                normalized.ToString(CultureInfo.InvariantCulture));
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
