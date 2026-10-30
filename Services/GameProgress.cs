using System.Text.Json;
using Microsoft.JSInterop;

namespace DevToolsHub.Services;

/// <summary>Game progress (solved levels, best scores, streaks), persisted in localStorage.</summary>
public sealed class GameProgress(IJSRuntime js)
{
    private const string Prefix = "game:";

    public async Task<T> GetAsync<T>(string key, T fallback)
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", Prefix + key);
            return string.IsNullOrEmpty(json) ? fallback : JsonSerializer.Deserialize<T>(json) ?? fallback;
        }
        catch (Exception e) when (e is JsonException or JSException or InvalidOperationException)
        {
            return fallback;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        try { await js.InvokeVoidAsync("localStorage.setItem", Prefix + key, JsonSerializer.Serialize(value)); }
        catch (Exception e) when (e is JSException or InvalidOperationException) { }
    }

    /// <summary>Stores the score if it beats the previous best and returns the best score.</summary>
    public async Task<int> SaveBestAsync(string key, int score, bool lowerIsBetter = false)
    {
        var best = await GetAsync<int?>(key, null);
        if (best is null || (lowerIsBetter ? score < best : score > best))
        {
            await SetAsync(key, score);
            return score;
        }
        return best.Value;
    }
}
