using System.Text.Json;
using Microsoft.JSInterop;
using SnakeGame;

namespace SnakeGame.Web;

public sealed record WebGamePreferences(DifficultyLevel Difficulty, bool ShowArrowControls);

/// <summary>
/// Persists web-specific player preferences in browser <c>localStorage</c>.
/// </summary>
public sealed class BrowserPreferencesStore(IJSRuntime jsRuntime)
{
    private const string PreferencesKey = "snake-game-preferences";
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async ValueTask<WebGamePreferences> LoadAsync()
    {
        try
        {
            var raw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", PreferencesKey);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var parsed = JsonSerializer.Deserialize<WebGamePreferences>(raw);
                if (parsed is not null && Enum.IsDefined(parsed.Difficulty))
                {
                    return parsed;
                }
            }
        }
        catch (JSException)
        {
        }
        catch (JsonException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        return new WebGamePreferences(DifficultyLevel.Normal, false);
    }

    public async ValueTask SaveAsync(WebGamePreferences preferences)
    {
        try
        {
            var json = JsonSerializer.Serialize(preferences);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", PreferencesKey, json);
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
