using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DevToolsHub.Services;

public readonly record struct SchemaError(string Path, string Message);

/// <summary>
/// Validates JSON against the commonly used keywords of JSON Schema draft-07, 2019-09 and 2020-12.
/// References may point into the root schema (#/...) or into additional schemas registered by $id.
/// </summary>
public sealed class JsonSchemaChecker
{
    public static readonly JsonDocumentOptions ParseOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private readonly JsonNode? root;
    private readonly Dictionary<string, JsonNode?> documents = new(StringComparer.Ordinal);

    public JsonSchemaChecker(JsonNode? schema, IEnumerable<JsonNode?>? additionalSchemas = null)
    {
        root = schema;
        Register(schema);
        foreach (var s in additionalSchemas ?? []) Register(s);
    }

    private void Register(JsonNode? schema)
    {
        if (schema is JsonObject o && o["$id"] is JsonValue id && id.GetValueKind() == JsonValueKind.String)
        {
            var value = id.GetValue<string>().TrimEnd('#');
            documents[value] = schema;
            var slash = value.LastIndexOf('/');
            if (slash >= 0 && slash < value.Length - 1) documents.TryAdd(value[(slash + 1)..], schema);
        }
    }

    public List<SchemaError> Validate(JsonNode? instance)
    {
        var errors = new List<SchemaError>();
        Check(instance, root, root, "$", errors, 0);
        return errors;
    }

    private bool Valid(JsonNode? v, JsonNode? s, JsonNode? doc, int depth, out HashSet<string> evaluated)
    {
        var tmp = new List<SchemaError>();
        evaluated = Check(v, s, doc, "$", tmp, depth + 1);
        return tmp.Count == 0;
    }

    private bool Valid(JsonNode? v, JsonNode? s, JsonNode? doc, int depth) => Valid(v, s, doc, depth, out _);

    /// <returns>The property names of <paramref name="v"/> evaluated by this schema (for unevaluatedProperties).</returns>
    private HashSet<string> Check(JsonNode? v, JsonNode? s, JsonNode? doc, string path, List<SchemaError> errs, int depth)
    {
        var evaluated = new HashSet<string>(StringComparer.Ordinal);
        if (depth > 64) throw new InvalidOperationException("Schema recursion is too deep (circular $ref?).");
        if (s is null) return evaluated;
        if (s is JsonValue bv && bv.GetValueKind() is JsonValueKind.True or JsonValueKind.False)
        {
            if (bv.GetValueKind() == JsonValueKind.False) errs.Add(new(path, "No value is allowed here (schema is false)."));
            else if (v is JsonObject all) evaluated.UnionWith(all.Select(p => p.Key));
            return evaluated;
        }
        if (s is not JsonObject so) return evaluated;

        if (so["$ref"] is JsonValue rv)
        {
            var (target, targetDoc) = Resolve(rv.GetValue<string>(), doc);
            evaluated.UnionWith(Check(v, target, targetDoc, path, errs, depth + 1));
        }

        var kind = Kind(v);
        if (so["type"] is { } t)
        {
            var types = t is JsonArray ta ? ta.Select(x => x!.GetValue<string>()).ToArray() : [t.GetValue<string>()];
            if (!types.Any(x => x == kind || (x == "number" && kind == "integer")))
                errs.Add(new(path, $"Expected {string.Join(" or ", types)} but got {kind}."));
        }
        if (so["enum"] is JsonArray en && !en.Any(e => JsonNode.DeepEquals(e, v)))
            errs.Add(new(path, $"Value must be one of {en.ToJsonString()}."));
        if (so.ContainsKey("const") && !JsonNode.DeepEquals(so["const"], v))
            errs.Add(new(path, $"Value must equal {so["const"]?.ToJsonString() ?? "null"}."));

        if (kind is "number" or "integer")
        {
            var n = v!.GetValue<double>();
            if (Num(so, "minimum") is { } min && n < min) errs.Add(new(path, $"Must be >= {min}."));
            if (Num(so, "maximum") is { } max && n > max) errs.Add(new(path, $"Must be <= {max}."));
            if (Num(so, "exclusiveMinimum") is { } emin && n <= emin) errs.Add(new(path, $"Must be > {emin}."));
            if (Num(so, "exclusiveMaximum") is { } emax && n >= emax) errs.Add(new(path, $"Must be < {emax}."));
            if (Num(so, "multipleOf") is { } mo && mo > 0 && Math.Abs(n / mo - Math.Round(n / mo)) > 1e-9)
                errs.Add(new(path, $"Must be a multiple of {mo}."));
        }

        if (kind == "string")
        {
            var str = v!.GetValue<string>();
            var len = str.EnumerateRunes().Count();
            if (Num(so, "minLength") is { } minL && len < minL) errs.Add(new(path, $"Length must be >= {minL}."));
            if (Num(so, "maxLength") is { } maxL && len > maxL) errs.Add(new(path, $"Length must be <= {maxL}."));
            if (so["pattern"] is JsonValue pv && !Regex.IsMatch(str, pv.GetValue<string>(), RegexOptions.None, RegexTimeout))
                errs.Add(new(path, $"Does not match pattern {pv.GetValue<string>()}."));
            if (so["format"] is JsonValue fv && !FormatOk(fv.GetValue<string>(), str))
                errs.Add(new(path, $"Not a valid {fv.GetValue<string>()}."));
        }

        if (v is JsonArray arr)
        {
            if (Num(so, "minItems") is { } minI && arr.Count < minI) errs.Add(new(path, $"Must have at least {minI} items."));
            if (Num(so, "maxItems") is { } maxI && arr.Count > maxI) errs.Add(new(path, $"Must have at most {maxI} items."));
            var prefix = so["prefixItems"] as JsonArray ?? so["items"] as JsonArray;
            var start = 0;
            if (prefix is not null)
            {
                for (var i = 0; i < Math.Min(prefix.Count, arr.Count); i++) Check(arr[i], prefix[i], doc, $"{path}[{i}]", errs, depth + 1);
                start = prefix.Count;
            }
            var rest = so["items"] is JsonArray ? so["additionalItems"] : so["items"];
            if (rest is not null)
                for (var i = start; i < arr.Count; i++) Check(arr[i], rest, doc, $"{path}[{i}]", errs, depth + 1);
            if (so["uniqueItems"] is JsonValue uv && uv.GetValueKind() == JsonValueKind.True)
            {
                for (var i = 0; i < arr.Count; i++)
                    for (var j = i + 1; j < arr.Count; j++)
                        if (JsonNode.DeepEquals(arr[i], arr[j])) errs.Add(new($"{path}[{j}]", $"Duplicate of item {i}; items must be unique."));
            }
            if (so["contains"] is { } c)
            {
                var matches = arr.Count(x => Valid(x, c, doc, depth));
                var minC = Num(so, "minContains") ?? 1;
                if (matches < minC) errs.Add(new(path, minC <= 1 ? "No item matches the 'contains' schema." : $"At least {minC} items must match 'contains' (found {matches})."));
                if (Num(so, "maxContains") is { } maxC && matches > maxC) errs.Add(new(path, $"At most {maxC} items may match 'contains' (found {matches})."));
            }
        }

        if (v is JsonObject obj)
        {
            if (so["required"] is JsonArray req)
                foreach (var r in req.Select(x => x!.GetValue<string>()).Where(r => !obj.ContainsKey(r)))
                    errs.Add(new(path, $"Missing required property '{r}'."));
            if (Num(so, "minProperties") is { } minP && obj.Count < minP) errs.Add(new(path, $"Must have at least {minP} properties."));
            if (Num(so, "maxProperties") is { } maxP && obj.Count > maxP) errs.Add(new(path, $"Must have at most {maxP} properties."));

            // dependentRequired (2019-09+) and the array form of draft-07 "dependencies".
            foreach (var key in new[] { "dependentRequired", "dependencies" })
                if (so[key] is JsonObject deps)
                    foreach (var (prop, needed) in deps)
                        if (obj.ContainsKey(prop) && needed is JsonArray names)
                            foreach (var n in names.Select(x => x!.GetValue<string>()).Where(n => !obj.ContainsKey(n)))
                                errs.Add(new(path, $"Property '{n}' is required when '{prop}' is present."));

            // dependentSchemas (2019-09+) and the schema form of draft-07 "dependencies".
            foreach (var key in new[] { "dependentSchemas", "dependencies" })
                if (so[key] is JsonObject deps)
                    foreach (var (prop, schema) in deps)
                        if (obj.ContainsKey(prop) && schema is not JsonArray)
                            evaluated.UnionWith(Check(v, schema, doc, path, errs, depth + 1));

            if (so["propertyNames"] is { } pn)
                foreach (var key in obj.Select(p => p.Key))
                    if (!Valid(JsonValue.Create(key), pn, doc, depth))
                        errs.Add(new($"{path}.{key}", $"Property name '{key}' does not match propertyNames."));

            var props = so["properties"] as JsonObject;
            var patterns = so["patternProperties"] as JsonObject;
            foreach (var (key, val) in obj)
            {
                var p = $"{path}.{key}";
                var matched = false;
                if (props is not null && props.TryGetPropertyValue(key, out var ps))
                {
                    matched = true;
                    Check(val, ps, doc, p, errs, depth + 1);
                }
                if (patterns is not null)
                    foreach (var (pk, pv) in patterns)
                        if (Regex.IsMatch(key, pk, RegexOptions.None, RegexTimeout))
                        {
                            matched = true;
                            Check(val, pv, doc, p, errs, depth + 1);
                        }
                if (matched) evaluated.Add(key);
                else if (so["additionalProperties"] is { } ap)
                {
                    evaluated.Add(key);
                    if (ap is JsonValue av && av.GetValueKind() == JsonValueKind.False) errs.Add(new(p, $"Property '{key}' is not allowed."));
                    else Check(val, ap, doc, p, errs, depth + 1);
                }
            }
        }

        if (so["allOf"] is JsonArray allOf)
            foreach (var sub in allOf) evaluated.UnionWith(Check(v, sub, doc, path, errs, depth + 1));
        if (so["anyOf"] is JsonArray anyOf)
        {
            var any = false;
            foreach (var sub in anyOf)
                if (Valid(v, sub, doc, depth, out var ev)) { any = true; evaluated.UnionWith(ev); }
            if (!any) errs.Add(new(path, "Value does not match any schema in anyOf."));
        }
        if (so["oneOf"] is JsonArray oneOf)
        {
            var count = 0;
            foreach (var sub in oneOf)
                if (Valid(v, sub, doc, depth, out var ev)) { count++; evaluated.UnionWith(ev); }
            if (count != 1) errs.Add(new(path, $"Value must match exactly one schema in oneOf (matched {count})."));
        }
        if (so["not"] is { } not && Valid(v, not, doc, depth))
            errs.Add(new(path, "Value must not match the 'not' schema."));
        if (so["if"] is { } cond)
        {
            var ok = Valid(v, cond, doc, depth, out var ev);
            if (ok) evaluated.UnionWith(ev);
            var branch = ok ? so["then"] : so["else"];
            if (branch is not null) evaluated.UnionWith(Check(v, branch, doc, path, errs, depth + 1));
        }

        if (v is JsonObject uo && so["unevaluatedProperties"] is { } up)
        {
            foreach (var (key, val) in uo.Where(p => !evaluated.Contains(p.Key)).ToList())
            {
                if (up is JsonValue uv && uv.GetValueKind() == JsonValueKind.False) errs.Add(new($"{path}.{key}", $"Property '{key}' is not allowed (unevaluated)."));
                else Check(val, up, doc, $"{path}.{key}", errs, depth + 1);
                evaluated.Add(key);
            }
        }

        return evaluated;
    }

    private (JsonNode? Node, JsonNode? Doc) Resolve(string reference, JsonNode? currentDoc)
    {
        var hash = reference.IndexOf('#');
        var docPart = hash < 0 ? reference : reference[..hash];
        var pointer = hash < 0 ? "" : reference[(hash + 1)..];

        var doc = currentDoc;
        if (docPart.Length > 0)
        {
            if (!documents.TryGetValue(docPart, out doc))
            {
                var slash = docPart.LastIndexOf('/');
                if (slash < 0 || !documents.TryGetValue(docPart[(slash + 1)..], out doc))
                    throw new InvalidOperationException($"Cannot resolve $ref '{reference}'. Add the referenced schema (with a matching $id) to the additional schemas.");
            }
        }

        var node = doc;
        foreach (var raw in pointer.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            var seg = Uri.UnescapeDataString(raw).Replace("~1", "/").Replace("~0", "~");
            node = node switch
            {
                JsonObject o => o[seg],
                JsonArray a when int.TryParse(seg, out var i) && i < a.Count => a[i],
                _ => null
            };
            if (node is null) throw new InvalidOperationException($"Cannot resolve $ref '{reference}'.");
        }
        return (node, doc);
    }

    private static double? Num(JsonObject s, string key) =>
        s[key] is JsonValue jv && jv.GetValueKind() == JsonValueKind.Number ? jv.GetValue<double>() : null;

    public static string Kind(JsonNode? v) => v switch
    {
        null => "null",
        JsonObject => "object",
        JsonArray => "array",
        JsonValue jv => jv.GetValueKind() switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Number => jv.GetValue<double>() is var d && d == Math.Floor(d) && !double.IsInfinity(d) ? "integer" : "number",
            _ => "null"
        },
        _ => "null"
    };

    public static bool FormatOk(string format, string s) => format switch
    {
        "email" => Regex.IsMatch(s, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
        "date-time" => Regex.IsMatch(s, @"^\d{4}-\d{2}-\d{2}[Tt ]\d{2}:\d{2}:\d{2}(\.\d+)?([Zz]|[+-]\d{2}:\d{2})$")
                       && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
        "date" => DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
        "time" => Regex.IsMatch(s, @"^\d{2}:\d{2}:\d{2}(\.\d+)?([Zz]|[+-]\d{2}:\d{2})?$"),
        "uuid" => Guid.TryParseExact(s, "D", out _),
        "uri" => Uri.TryCreate(s, UriKind.Absolute, out _),
        "uri-reference" => Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out _),
        "ipv4" => IPAddress.TryParse(s, out var ip4) && ip4.AddressFamily == AddressFamily.InterNetwork && s.Count(c => c == '.') == 3,
        "ipv6" => IPAddress.TryParse(s, out var ip6) && ip6.AddressFamily == AddressFamily.InterNetworkV6,
        "hostname" => Regex.IsMatch(s, @"^(?=.{1,253}$)[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)*$", RegexOptions.IgnoreCase),
        _ => true
    };
}
