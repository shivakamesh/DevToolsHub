using System.Globalization;
using System.Text.RegularExpressions;

namespace DevToolsHub.Services;

public static class ElectronicsCalc
{
    // ---- PCB trace width (IPC-2221) ----
    public const double MilPerOz = 1.378;

    /// <summary>Required trace width in mils for a current (A), temperature rise (°C) and copper weight (oz/ft²).</summary>
    public static double TraceWidthMils(double amps, double riseC, double copperOz, bool internalLayer)
    {
        if (amps <= 0 || riseC <= 0 || copperOz <= 0) return double.NaN;
        var k = internalLayer ? 0.024 : 0.048;
        var area = Math.Pow(amps / (k * Math.Pow(riseC, 0.44)), 1 / 0.725);
        return area / (copperOz * MilPerOz);
    }

    /// <summary>Maximum current (A) of a trace of given width in mils.</summary>
    public static double TraceCurrent(double widthMils, double riseC, double copperOz, bool internalLayer)
    {
        var k = internalLayer ? 0.024 : 0.048;
        return k * Math.Pow(riseC, 0.44) * Math.Pow(widthMils * copperOz * MilPerOz, 0.725);
    }

    /// <summary>DC resistance in ohms of a copper trace (width in mils, length in mm) at temperature °C.</summary>
    public static double TraceResistance(double widthMils, double lengthMm, double copperOz, double tempC = 25)
    {
        const double rho = 1.72e-8, alpha = 0.00393;
        var areaM2 = widthMils * 25.4e-6 * copperOz * MilPerOz * 25.4e-6;
        return rho * (1 + alpha * (tempC - 25)) * (lengthMm / 1000) / areaM2;
    }

    // ---- 555 timer ----
    public readonly record struct Astable(double FrequencyHz, double HighS, double LowS, double DutyPercent);

    public static Astable Timer555Astable(double r1Ohm, double r2Ohm, double cFarad)
    {
        var high = Math.Log(2) * (r1Ohm + r2Ohm) * cFarad;
        var low = Math.Log(2) * r2Ohm * cFarad;
        return new(1 / (high + low), high, low, high / (high + low) * 100);
    }

    public static double Timer555Monostable(double rOhm, double cFarad) => Math.Log(3) * rOhm * cFarad;

    // ---- Battery life ----
    public static double AverageCurrent(double activeMa, double sleepMa, double activeDutyPercent)
    {
        var d = Math.Clamp(activeDutyPercent, 0, 100) / 100;
        return activeMa * d + sleepMa * (1 - d);
    }

    /// <summary>Battery life in hours for a capacity (mAh), average current (mA) and usable fraction (0–1).</summary>
    public static double BatteryHours(double capacityMah, double averageMa, double usableFraction) =>
        averageMa <= 0 ? double.PositiveInfinity : capacityMah * Math.Clamp(usableFraction, 0, 1) / averageMa;

    // ---- SMD resistor codes ----
    private static readonly int[] E96 =
    [
        100, 102, 105, 107, 110, 113, 115, 118, 121, 124, 127, 130, 133, 137, 140, 143, 147, 150, 154, 158,
        162, 165, 169, 174, 178, 182, 187, 191, 196, 200, 205, 210, 215, 221, 226, 232, 237, 243, 249, 255,
        261, 267, 274, 280, 287, 294, 301, 309, 316, 324, 332, 340, 348, 357, 365, 374, 383, 392, 402, 412,
        422, 432, 442, 453, 464, 475, 487, 499, 511, 523, 536, 549, 562, 576, 590, 604, 619, 634, 649, 665,
        681, 698, 715, 732, 750, 768, 787, 806, 825, 845, 866, 887, 909, 931, 953, 976
    ];

    private static readonly Dictionary<char, double> Eia96Multiplier = new()
    {
        ['Z'] = 0.001, ['Y'] = 0.01, ['R'] = 0.01, ['X'] = 0.1, ['S'] = 0.1, ['A'] = 1,
        ['B'] = 10, ['H'] = 10, ['C'] = 100, ['D'] = 1_000, ['E'] = 10_000, ['F'] = 100_000
    };

    /// <summary>Decodes SMD resistor markings: 3-digit (103), 4-digit (1002), R/m notation (4R7, R10, 2m2) and EIA-96 (01C).</summary>
    public static (double Ohms, string System)? DecodeSmd(string code)
    {
        var raw = code.Trim();
        if (raw.Length == 0) return null;
        if (Regex.IsMatch(raw, @"^0+$")) return (0, "Zero-ohm jumper");

        var m = Regex.Match(raw, @"^(\d*)([RrMm])(\d*)$");
        if (m.Success && (m.Groups[1].Length + m.Groups[3].Length) is >= 1 and <= 4)
        {
            var value = double.Parse($"{(m.Groups[1].Length > 0 ? m.Groups[1].Value : "0")}.{(m.Groups[3].Length > 0 ? m.Groups[3].Value : "0")}", CultureInfo.InvariantCulture);
            return m.Groups[2].Value is "m" or "M" ? (value / 1000, "Milliohm (m) notation") : (value, "R notation");
        }

        var c = raw.ToUpperInvariant();
        m = Regex.Match(c, @"^(\d{2})([A-Z])$");
        if (m.Success && Eia96Multiplier.TryGetValue(m.Groups[2].Value[0], out var mult))
        {
            var idx = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            if (idx is >= 1 and <= 96) return (E96[idx - 1] * mult, "EIA-96 (1%)");
        }

        if (Regex.IsMatch(c, @"^\d{3}$")) return (int.Parse(c[..2], CultureInfo.InvariantCulture) * Math.Pow(10, c[2] - '0'), "3-digit (5%)");
        if (Regex.IsMatch(c, @"^\d{4}$")) return (int.Parse(c[..3], CultureInfo.InvariantCulture) * Math.Pow(10, c[3] - '0'), "4-digit (1%)");
        return null;
    }

    public static string FormatOhms(double ohms) => ohms switch
    {
        >= 1e6 => (ohms / 1e6).ToString("0.###", CultureInfo.InvariantCulture) + " MΩ",
        >= 1e3 => (ohms / 1e3).ToString("0.###", CultureInfo.InvariantCulture) + " kΩ",
        >= 1 or 0 => ohms.ToString("0.###", CultureInfo.InvariantCulture) + " Ω",
        _ => (ohms * 1000).ToString("0.###", CultureInfo.InvariantCulture) + " mΩ"
    };
}
