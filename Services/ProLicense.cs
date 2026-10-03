using System.Text.Json;
using Microsoft.JSInterop;

namespace DevToolsHub.Services;

public sealed record ProPlan(string Id, string Name, string Price, string PaymentLink, int Months);

/// <summary>
/// Pro subscription (monthly or yearly) purchased via Stripe Payment Links, persisted in localStorage.
/// PLACEHOLDER: purchases are not verified server-side, so the unlock can be bypassed.
/// Replace with real verification (e.g. a Stripe webhook + API) before relying on it for revenue.
/// </summary>
public sealed class ProLicense(IJSRuntime js)
{
    /// <summary>Master switch for the paid Pro tier. When false, all Pro UI and features are hidden.</summary>
    public static readonly bool Enabled = false;

    private const string StorageKey = "devtools.pro";
    private static readonly TimeSpan GracePeriod = TimeSpan.FromDays(3);

    // In Stripe: create one product with a monthly and a yearly recurring price, then a Payment Link for each.
    // Set each link's "After payment" redirect to: https://<your-site>/pro?unlock=success&plan=<plan id>
    public static readonly ProPlan[] Plans =
    [
        new("monthly", "Monthly", "$5 / month", "https://buy.stripe.com/REPLACE_WITH_MONTHLY_LINK", 1),
        new("yearly", "Yearly", "$48 / year (save 20%)", "https://buy.stripe.com/REPLACE_WITH_YEARLY_LINK", 12),
    ];

    // Stripe customer portal link (Settings → Billing → Customer portal) so subscribers can cancel or update cards.
    public const string ManageLink = "https://billing.stripe.com/p/login/REPLACE_WITH_PORTAL_LINK";

    // Unlock codes you send to subscribers (e.g. in the confirmation email), mapped to their plan.
    private static readonly Dictionary<string, string> ValidCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DTH-MONTHLY-2025"] = "monthly",
        ["DTH-YEARLY-2025"] = "yearly",
    };

    private sealed record State(string Plan, DateTime? ExpiresUtc);

    private bool loaded;
    private State? state;

    public bool IsPro => Enabled && state is not null && (state.ExpiresUtc is null || DateTime.UtcNow < state.ExpiresUtc.Value + GracePeriod);
    public bool IsExpired => Enabled && state is not null && !IsPro;
    public ProPlan? Plan => Plans.FirstOrDefault(p => p.Id == state?.Plan);
    public DateTime? ExpiresUtc => state?.ExpiresUtc;

    public event Action? Changed;

    public static ProPlan? FindPlan(string? id) => Plans.FirstOrDefault(p => p.Id == id);

    public async Task LoadAsync()
    {
        if (loaded) return;
        loaded = true;
        var raw = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (raw == "1")
        {
            // Earlier one-time purchases stay active with no expiry.
            state = new State("lifetime", null);
        }
        else if (!string.IsNullOrEmpty(raw))
        {
            try { state = JsonSerializer.Deserialize<State>(raw); }
            catch (JsonException) { state = null; }
        }
        if (state is not null) Changed?.Invoke();
    }

    public async Task<bool> TryUnlockAsync(string? code)
    {
        var key = code?.Trim();
        if (string.IsNullOrEmpty(key) || !ValidCodes.TryGetValue(key, out var planId)) return false;
        await ActivateAsync(planId);
        return true;
    }

    /// <summary>Called when Stripe redirects back after a successful subscription payment.</summary>
    public async Task<bool> UnlockFromCheckoutAsync(string? planId)
    {
        if (FindPlan(planId) is null) return false;
        await ActivateAsync(planId!);
        return true;
    }

    public async Task DeactivateAsync()
    {
        state = null;
        loaded = true;
        await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        Changed?.Invoke();
    }

    private async Task ActivateAsync(string planId)
    {
        var plan = FindPlan(planId)!;
        var start = IsPro && state?.ExpiresUtc is { } current && current > DateTime.UtcNow ? current : DateTime.UtcNow;
        state = new State(plan.Id, start.AddMonths(plan.Months));
        loaded = true;
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(state));
        Changed?.Invoke();
    }
}
