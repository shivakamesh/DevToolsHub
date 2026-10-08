using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DevToolsHub.Services;

public sealed record BomLine(
    string Part, string Description, string References, int PerBoard, int Required, int OrderQty,
    decimal UnitPrice, decimal Extended, int Moq, int Multiple);

public sealed record BomResult(IReadOnlyList<BomLine> Lines, string? Error)
{
    public decimal CostPerBoard => Lines.Sum(l => l.PerBoard * l.UnitPrice);
    public decimal Total => Lines.Sum(l => l.Extended);
    public int Placements => Lines.Sum(l => l.PerBoard);
}

/// <summary>Parses BOM CSV, consolidates parts and calculates order quantities, price breaks and cost.</summary>
public static class BomEngine
{
    public static BomResult Calculate(string csv, int boards, double attritionPercent)
    {
        var rows = ParseCsv(csv);
        if (rows.Count < 2) return new([], null);

        var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
        int Find(params string[] names)
        {
            var exact = header.FindIndex(h => names.Contains(h));
            return exact >= 0 ? exact : header.FindIndex(h => names.Any(n => h.Contains(n)));
        }

        var breaksCol = Find("price breaks", "price break", "breaks");
        var refCol = Find("reference", "references", "designator", "designators", "ref", "refdes");
        var qtyCol = Find("qty", "quantity", "count");
        var mpnCol = Find("mpn", "manufacturer part number", "part number", "partnumber", "pn", "part");
        var descCol = Find("value", "description", "comment", "desc");
        var priceCol = header.FindIndex((i, h) => i != breaksCol && (h is "unit price" or "price" or "unit cost" or "cost"));
        if (priceCol < 0) priceCol = header.FindIndex((i, h) => i != breaksCol && (h.Contains("price") || h.Contains("cost")));
        var moqCol = Find("moq", "minimum order");
        var multCol = Find("multiple", "mult", "reel", "spq", "pack");

        if (mpnCol < 0 && descCol < 0) return new([], "Could not find a part number (MPN/Part) or Value/Description column in the header row.");
        if (qtyCol < 0 && refCol < 0) return new([], "Could not find a Qty or Reference/Designator column in the header row.");

        static string Cell(List<string> r, int i) => i >= 0 && i < r.Count ? r[i].Trim() : "";

        var groups = new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in rows.Skip(1).Where(r => r.Any(c => c.Trim().Length > 0)))
        {
            var refs = Regex.Split(Cell(r, refCol), @"[,;\s]+").Where(x => x.Length > 0).ToList();
            var qty = int.TryParse(Cell(r, qtyCol), NumberStyles.Integer, CultureInfo.InvariantCulture, out var q) ? q : refs.Count;
            if (qty <= 0) continue;
            var key = Cell(r, mpnCol) is { Length: > 0 } mpn ? mpn : Cell(r, descCol);
            if (key.Length == 0) continue;

            var breaks = ParseBreaks(Cell(r, breaksCol));
            if (ParseMoney(Cell(r, priceCol)) is { } unit) breaks.Insert(0, (1, unit));
            var moq = int.TryParse(Cell(r, moqCol), out var m) && m > 0 ? m : 1;
            var mult = int.TryParse(Cell(r, multCol), out var k) && k > 0 ? k : 1;

            if (!groups.TryGetValue(key, out var g)) groups[key] = g = new Group(Cell(r, descCol));
            g.Refs.AddRange(refs);
            g.Qty += qty;
            if (g.Breaks.Count == 0) g.Breaks.AddRange(breaks);
            g.Moq = Math.Max(g.Moq, moq);
            g.Multiple = Math.Max(g.Multiple, mult);
        }

        var factor = 1 + Math.Max(0, attritionPercent) / 100;
        var n = Math.Max(0, boards);
        var lines = groups.Select(kv =>
        {
            var g = kv.Value;
            var required = (int)Math.Ceiling(g.Qty * n * factor - 1e-9);
            var order = OrderQuantity(required, g.Moq, g.Multiple);
            var price = PriceFor(g.Breaks, order);
            return new BomLine(kv.Key, g.Description, string.Join(", ", g.Refs), g.Qty, required, order, price, order * price, g.Moq, g.Multiple);
        }).OrderByDescending(l => l.Extended).ToList();

        return new(lines, null);
    }

    public static int OrderQuantity(int required, int moq, int multiple)
    {
        if (required <= 0) return 0;
        var q = Math.Max(required, Math.Max(1, moq));
        var mult = Math.Max(1, multiple);
        return (q + mult - 1) / mult * mult;
    }

    /// <summary>Returns the unit price of the highest break whose quantity is &lt;= <paramref name="qty"/>.</summary>
    public static decimal PriceFor(IReadOnlyList<(int Qty, decimal Price)> breaks, int qty)
    {
        if (breaks.Count == 0) return 0;
        var ordered = breaks.OrderBy(b => b.Qty).ToList();
        var price = ordered[0].Price;
        foreach (var b in ordered)
            if (qty >= b.Qty) price = b.Price;
        return price;
    }

    /// <summary>Parses breaks such as "1:0.10; 100:0.05; 1000:0.02" (separators ; | or space, qty:price or qty=price).</summary>
    public static List<(int Qty, decimal Price)> ParseBreaks(string text)
    {
        var list = new List<(int, decimal)>();
        foreach (Match m in Regex.Matches(text, @"(\d[\d,]*)\s*[:=@]\s*([^\s;|]+)"))
            if (int.TryParse(m.Groups[1].Value.Replace(",", ""), out var qty) && ParseMoney(m.Groups[2].Value) is { } price)
                list.Add((qty, price));
        return list;
    }

    public static decimal? ParseMoney(string text)
    {
        var cleaned = Regex.Replace(text, @"[^\d.\-]", "");
        return cleaned.Length > 0 && decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    public static string ToCsv(IEnumerable<BomLine> lines)
    {
        var sb = new StringBuilder("Part,Description,References,QtyPerBoard,Required,OrderQty,UnitPrice,Extended\n");
        foreach (var l in lines)
            sb.Append(Esc(l.Part)).Append(',').Append(Esc(l.Description)).Append(',').Append(Esc(l.References)).Append(',')
              .Append(l.PerBoard).Append(',').Append(l.Required).Append(',').Append(l.OrderQty).Append(',')
              .Append(l.UnitPrice.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(l.Extended.ToString(CultureInfo.InvariantCulture)).Append('\n');
        return sb.ToString();
    }

    private static string Esc(string s) => s.IndexOfAny([',', '"', '\n']) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;

    public static List<List<string>> ParseCsv(string text)
    {
        var t = text.Trim().Replace("\r\n", "\n");
        var firstLine = t.Split('\n')[0];
        var delimiter = new[] { ',', ';', '\t' }.OrderByDescending(d => firstLine.Count(c => c == d)).First();
        var rows = new List<List<string>>();
        var row = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < t.Length; i++)
        {
            var c = t[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < t.Length && t[i + 1] == '"') { cell.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else cell.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == delimiter) { row.Add(cell.ToString().Trim()); cell.Clear(); }
            else if (c == '\n') { row.Add(cell.ToString().Trim()); cell.Clear(); rows.Add(row); row = []; }
            else cell.Append(c);
        }
        row.Add(cell.ToString().Trim());
        rows.Add(row);
        return rows;
    }

    private sealed class Group(string description)
    {
        public string Description { get; } = description;
        public List<string> Refs { get; } = [];
        public List<(int Qty, decimal Price)> Breaks { get; } = [];
        public int Qty { get; set; }
        public int Moq { get; set; } = 1;
        public int Multiple { get; set; } = 1;
    }
}

internal static class ListExtensions
{
    public static int FindIndex<T>(this List<T> list, Func<int, T, bool> predicate)
    {
        for (var i = 0; i < list.Count; i++)
            if (predicate(i, list[i])) return i;
        return -1;
    }
}
