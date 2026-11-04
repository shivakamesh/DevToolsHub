using System.Text.Json;
using Microsoft.JSInterop;

namespace DevToolsHub.Services;

/// <summary>Typed per-tool state (notes, request history, settings), persisted in localStorage.</summary>
public sealed class LocalStore(IJSRuntime js)
{
    private const string Prefix = "tool:";

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
}
