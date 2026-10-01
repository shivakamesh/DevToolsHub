using System.Text.Json;
using Microsoft.JSInterop;

namespace DevToolsHub.Services;

/// <summary>Favorites and recently used tools, persisted in localStorage.</summary>
public sealed class UserPrefs(IJSRuntime js)
{
    private const int MaxRecent = 6;
    private bool loaded;

    public List<string> Favorites { get; private set; } = [];
    public List<string> Recent { get; private set; } = [];

    public event Action? Changed;

    public async Task EnsureLoadedAsync()
    {
        if (loaded) return;
        Favorites = await ReadAsync("favorites");
        Recent = await ReadAsync("recent");
        loaded = true;
        Changed?.Invoke();
    }

    public bool IsFavorite(string href) => Favorites.Contains(href);

    public async Task ToggleFavoriteAsync(string href)
    {
        await EnsureLoadedAsync();
        if (!Favorites.Remove(href)) Favorites.Add(href);
        await WriteAsync("favorites", Favorites);
        Changed?.Invoke();
    }

    public async Task AddRecentAsync(string href)
    {
        await EnsureLoadedAsync();
        Recent.Remove(href);
        Recent.Insert(0, href);
        if (Recent.Count > MaxRecent) Recent.RemoveRange(MaxRecent, Recent.Count - MaxRecent);
        await WriteAsync("recent", Recent);
        Changed?.Invoke();
    }

    private async Task<List<string>> ReadAsync(string key)
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? [] : JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private ValueTask WriteAsync(string key, List<string> value) =>
        js.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
}
