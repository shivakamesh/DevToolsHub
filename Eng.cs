using System.Globalization;
using System.Text.RegularExpressions;

namespace DevToolsHub;

/// <summary>Shared helpers for the electrical/electronics calculators.</summary>
public static partial class Eng
{
    private static readonly (double Factor, string Prefix)[] Prefixes =
        [(1e12, "T"), (1e9, "G"), (1e6, "M"), (1e3, "k"), (1, ""), (1e-3, "m"), (1e-6, "µ"), (1e-9, "n"), (1e-12, "p")];

    public static string N(double v, string fmt = "G4") => v.ToString(fmt, CultureInfo.InvariantCulture);

    /// <summary>Formats a value with an engineering SI prefix, e.g. 4700 Ω → "4.7 kΩ".</summary>
    public static string Si(double v, string unit, string fmt = "G4")
    {
        if (v == 0 || double.IsNaN(v) || double.IsInfinity(v)) return $"{N(v, fmt)} {unit}";
        var a = Math.Abs(v);
        foreach (var (f, p) in Prefixes)
        {
            if (a >= f * 0.99995) return $"{N(v / f, fmt)} {p}{unit}";
        }
        return $"{N(v / 1e-12, fmt)} p{unit}";
    }

    [GeneratedRegex(@"^(?<a>\d+(?:\.\d+)?|\.\d+)?(?<p>[pnuµmkKMGRr])?(?<b>\d+)?$")]
    private static partial Regex SiPattern();

    /// <summary>Parses values like 4.7k, 4k7, 100n, 2u2, 4R7, 1M, 1e-6.</summary>
    public static bool TryParseSi(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var s = text.Trim().Replace(" ", "").Replace("μ", "u").Replace("µ", "u");
        foreach (var suffix in new[] { "ohms", "ohm", "Ω", "F", "H" })
        {
            if (s.Length > suffix.Length && s.EndsWith(suffix, StringComparison.Ordinal)) { s = s[..^suffix.Length]; break; }
        }
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return value >= 0;

        var m = SiPattern().Match(s);
        if (!m.Success) return false;
        var a = m.Groups["a"].Value;
        var p = m.Groups["p"].Value;
        var b = m.Groups["b"].Value;
        if (p.Length == 0 || (a.Length == 0 && b.Length == 0) || (b.Length > 0 && a.Contains('.'))) return false;
        var number = (a.Length == 0 ? "0" : a) + (b.Length > 0 ? "." + b : "");
        var mult = p switch
        {
            "p" => 1e-12, "n" => 1e-9, "u" => 1e-6, "m" => 1e-3,
            "k" or "K" => 1e3, "M" => 1e6, "G" => 1e9, _ => 1,
        };
        value = double.Parse(number, CultureInfo.InvariantCulture) * mult;
        return true;
    }

    // E-series preferred values
    public static readonly double[] E6 = [1.0, 1.5, 2.2, 3.3, 4.7, 6.8];
    public static readonly double[] E12 = [1.0, 1.2, 1.5, 1.8, 2.2, 2.7, 3.3, 3.9, 4.7, 5.6, 6.8, 8.2];
    public static readonly double[] E24 = [1.0, 1.1, 1.2, 1.3, 1.5, 1.6, 1.8, 2.0, 2.2, 2.4, 2.7, 3.0, 3.3, 3.6, 3.9, 4.3, 4.7, 5.1, 5.6, 6.2, 6.8, 7.5, 8.2, 9.1];

    public static double[] Series(int n) => n switch
    {
        6 => E6,
        12 => E12,
        24 => E24,
        _ => Enumerable.Range(0, n)
            .Select(i => Math.Round(Math.Pow(10, (double)i / n), 2))
            .Select(v => n == 192 && v == 9.19 ? 9.20 : v)
            .ToArray(),
    };

    /// <summary>All values of an E-series between min and max (inclusive).</summary>
    public static List<double> SeriesValues(int n, double min = 1, double max = 1e7)
    {
        var list = new List<double>();
        for (var exp = (int)Math.Floor(Math.Log10(min)); exp <= (int)Math.Ceiling(Math.Log10(max)); exp++)
        {
            var decade = Math.Pow(10, exp);
            foreach (var v in Series(n))
            {
                var value = Math.Round(v * decade, 12);
                if (value >= min * 0.9999 && value <= max * 1.0001) list.Add(value);
            }
        }
        return list;
    }

    public static double NearestInSeries(int n, double value)
    {
        var decade = Math.Pow(10, Math.Floor(Math.Log10(value)));
        return Series(n).Select(e => e * decade).Append(10 * decade)
            .OrderBy(v => Math.Abs(Math.Log(v / value))).First();
    }

    // Conductors
    public static readonly double[] MetricSizes = [0.5, 0.75, 1, 1.5, 2.5, 4, 6, 10, 16, 25, 35, 50, 70, 95, 120, 150, 185, 240, 300, 400, 500, 630];

    /// <summary>AWG and kcmil sizes ordered from smallest to largest.</summary>
    public static readonly (string Label, double Mm2)[] AwgSizes = BuildAwg();

    private static (string, double)[] BuildAwg()
    {
        var list = new List<(string, double)>();
        for (var n = 30; n >= -3; n--)
        {
            var d = 0.127 * Math.Pow(92, (36.0 - n) / 39.0);
            var label = n > 0 ? n.ToString(CultureInfo.InvariantCulture) : $"{1 - n}/0";
            list.Add((label, Math.PI / 4 * d * d));
        }
        foreach (var k in new[] { 250, 300, 350, 400, 500, 600, 750, 1000 })
        {
            list.Add(($"{k} kcmil", k * 0.506707));
        }
        return [.. list];
    }

    public static string AwgLabel(string label) => label.Contains("kcmil") ? label : label + " AWG";

    /// <summary>Resistivity at 20 °C in Ω·mm²/m and temperature coefficient per K.</summary>
    public static (double Rho20, double Alpha) Material(string material) => material switch
    {
        "al" => (0.028264, 0.00403),
        "ag" => (0.01587, 0.0038),
        "au" => (0.02214, 0.0034),
        "steel" => (0.143, 0.0045),
        "nichrome" => (1.10, 0.0004),
        _ => (0.017241, 0.00393),
    };

    public static double Resistivity(string material, double tempC)
    {
        var (rho, alpha) = Material(material);
        return rho * (1 + alpha * (tempC - 20));
    }
}
