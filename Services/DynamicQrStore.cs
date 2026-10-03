using System.Text.Json;
using Microsoft.JSInterop;

namespace DevToolsHub.Services;

public sealed class DynamicQrScan
{
    public DateTime Utc { get; set; }
    public string? UserAgent { get; set; }
}

public sealed class DynamicQr
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string TargetUrl { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public List<DynamicQrScan> Scans { get; set; } = [];
}

/// <summary>
/// Dynamic QR codes persisted in localStorage.
/// PLACEHOLDER: there is no backend, so redirects and scan analytics only work in the browser that created the code.
/// Replace with a server-side redirect + scan store before relying on it for real-world printed codes.
/// </summary>
public sealed class DynamicQrStore(IJSRuntime js)
{
    public const int FreeLimit = 3;
    private const int MaxScansPerCode = 1000;
    private const string StorageKey = "devtools.dynqr";

    private bool loaded;
    private List<DynamicQr> codes = [];

    public IReadOnlyList<DynamicQr> Codes => codes;

    public async Task LoadAsync()
    {
        if (loaded) return;
        loaded = true;
        var raw = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrEmpty(raw)) return;
        try { codes = JsonSerializer.Deserialize<List<DynamicQr>>(raw) ?? []; }
        catch (JsonException) { codes = []; }
    }

    public DynamicQr? Find(string? id) => codes.FirstOrDefault(c => c.Id == id);

    public async Task<DynamicQr> CreateAsync(string name, string targetUrl)
    {
        var code = new DynamicQr
        {
            Id = Convert.ToHexString(Guid.NewGuid().ToByteArray(), 0, 4).ToLowerInvariant(),
            Name = string.IsNullOrWhiteSpace(name) ? "Untitled" : name.Trim(),
            TargetUrl = targetUrl.Trim(),
            CreatedUtc = DateTime.UtcNow,
        };
        codes.Add(code);
        await SaveAsync();
        return code;
    }

    public async Task UpdateTargetAsync(DynamicQr code, string targetUrl)
    {
        code.TargetUrl = targetUrl.Trim();
        await SaveAsync();
    }

    public async Task DeleteAsync(DynamicQr code)
    {
        codes.Remove(code);
        await SaveAsync();
    }

    public async Task RecordScanAsync(DynamicQr code, string? userAgent)
    {
        code.Scans.Add(new DynamicQrScan { Utc = DateTime.UtcNow, UserAgent = userAgent });
        if (code.Scans.Count > MaxScansPerCode) code.Scans.RemoveRange(0, code.Scans.Count - MaxScansPerCode);
        await SaveAsync();
    }

    public static bool IsValidTarget(string? url) =>
        Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);

    private Task SaveAsync() =>
        js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(codes)).AsTask();
}
