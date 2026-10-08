namespace DevToolsHub;

public record ToolInfo(string Name, string Href, string Icon, string Category, string Summary, string Help);

public static class ToolCatalog
{
    public static readonly string[] Categories = ["Format", "Encode", "Generate", "Convert", "Inspect", "Engineering"];

    // Tools ordered by estimated search popularity (most searched first). Unlisted tools go last.
    private static readonly string[] Popularity =
    [
        "json-formatter", "diff-checker", "base64", "regex-tester", "jwt-decoder", "url-encoder",
        "timestamp-converter", "guid-generator", "password-generator", "hash-generator", "qr-code",
        "color-converter", "lorem-ipsum", "cron-explainer", "format-converter", "xml-formatter",
        "sql-formatter", "markdown-preview", "case-converter", "json-to-csharp", "text-statistics",
        "http-status-codes", "number-base", "image-to-base64", "html-entities", "user-agent-parser",
        "css-generator", "time-zone-converter", "chmod-calculator", "yaml-validator", "json-path",
        "cron-builder", "fake-data", "gitignore-generator", "byte-size", "hmac-generator",
        "ohms-law-calculator", "power-factor-correction",
                "transformer-calculator", "led-resistor-calculator",
                        "unix-timestamp-to-date", "csv-to-json", "uuid-v7-generator", "cidr-calculator", "sql-to-csharp",
                        "slug-generator", "awg-to-mm2", "voltage-divider-calculator",
                        "json-schema-validator", "json-diff", "openapi-to-csharp", "docker-compose-to-kubernetes",
                        "ampacity-table", "bom-calculator",
                                                "pcb-trace-width-calculator", "555-timer-calculator", "smd-resistor-code", "battery-life-calculator",
            ];

    private static int Rank(string href)
    {
        var i = Array.IndexOf(Popularity, href);
        return i < 0 ? int.MaxValue : i;
    }

    public static readonly IReadOnlyList<ToolInfo> All;

    static ToolCatalog() => All = Source.OrderBy(t => Rank(t.Href)).ToList();

    private static readonly IReadOnlyList<ToolInfo> Source =
    [
        // Format
        new("JSON Formatter", "json-formatter", "{ }", "Format", "Format, validate and minify JSON.",
            "Paste JSON and choose Format or Minify. Trailing commas and comments are tolerated. Errors show the line and position of the problem."),
        new("JSON to C#", "json-to-csharp", "C#", "Format", "Generate C# classes or records from JSON.",
            "Paste a JSON sample to generate strongly typed C# classes or records, with optional System.Text.Json attributes. Nested objects become separate types; arrays become List<T>."),
        new("Format Converter", "format-converter", "⇄", "Format", "Convert between JSON, YAML, XML and CSV.",
            "Pick the input and output formats. CSV works with arrays of flat objects; XML conversion follows the common Newtonsoft mapping (attributes become @-prefixed properties)."),
        new("XML Formatter", "xml-formatter", "</>", "Format", "Format, validate and minify XML.",
            "Paste XML to pretty-print or minify it. Invalid documents show the parser error with line and position."),
        new("YAML Validator", "yaml-validator", "Y✓", "Format", "Validate YAML syntax.",
            "Paste YAML and click Validate. Errors show the line and column of the problem. Multi-document streams separated by --- are supported."),
        new("SQL Formatter", "sql-formatter", "SQL", "Format", "Beautify SQL queries.",
            "Puts each major clause on its own line, splits select lists and AND/OR conditions, and indents subqueries. Optionally uppercases keywords. String literals and comments are left untouched."),
        new("Regex Tester", "regex-tester", ".*", "Format", "Test .NET regular expressions live.",
            "Type a .NET regular expression and test text. Matches are highlighted and capture groups are listed. Evaluation stops after one second to guard against catastrophic backtracking."),
        new("Cron Explainer", "cron-explainer", "⏱", "Format", "Validate cron and preview next run times.",
            "Enter a 5-field (or 6-field with seconds) cron expression to see the next 10 run times in any time zone."),
        new("Diff Checker", "diff-checker", "±", "Format", "Compare two texts line by line.",
            "Paste the original and changed text to see added and removed lines. Options let you ignore whitespace and letter case."),
        new("Markdown Preview", "markdown-preview", "M↓", "Format", "Live Markdown to HTML preview.",
            "Write Markdown on the left and see the rendered result instantly. Tables, task lists and other GitHub-style extensions are supported. Raw HTML is disabled for safety."),
        new("SQL to C#", "sql-to-csharp", "S#", "Format", "Generate C# classes from CREATE TABLE.",
            "Paste one or more SQL CREATE TABLE statements (SQL Server, PostgreSQL, MySQL or SQLite) to generate C# classes or records with optional data annotations. SQL types are mapped to their C# equivalents and NULL columns become nullable."),
        new("CSV to JSON", "csv-to-json", "⇢{}", "Format", "Convert CSV rows to a JSON array.",
            "Paste CSV to get a JSON array of objects (using the header row) or arrays. Quoted fields, escaped quotes and multi-line values are supported. Numbers and booleans can be detected automatically."),
        new("JSON Path Tester", "json-path", "$.", "Format", "Query JSON with JSONPath expressions.",
            "Enter JSON and a JSONPath expression such as $.store.book[*].author or $..price to list matching values."),

        // Encode
        new("Base64", "base64", "64", "Encode", "Encode and decode Base64 text (UTF-8).",
            "Encode text to Base64 or decode it back. Enable URL-safe to use - and _ instead of + and / without padding."),
        new("URL Encode/Decode", "url-encoder", "%", "Encode", "Percent-encode or decode URLs.",
            "Encodes text for safe use in URLs and query strings, or decodes percent-encoded text. + is treated as a space when decoding."),
        new("HTML Entities", "html-entities", "&;", "Encode", "Encode or decode HTML entities.",
            "Converts characters like < > & \" into HTML entities and back. Optionally encodes every non-ASCII character as a numeric entity."),
        new("String Escape", "string-escape", "\\n", "Encode", "Escape strings for C#, JSON, JavaScript and SQL.",
            "Escape or unescape text for use inside string literals in C#, C# verbatim strings, JSON, JavaScript or SQL."),
        new("JWT Decoder", "jwt-decoder", "🔑", "Encode", "Inspect JWT header, payload and expiry.",
            "Paste a JSON Web Token to see its decoded header and payload and whether it has expired. The signature is not verified."),
        new("JWT Generator", "jwt-generator", "🔏", "Encode", "Create HMAC-signed test JWTs.",
            "Edit the payload, choose HS256/HS384/HS512 and a secret to create a signed token. Intended for development and testing only."),
        new("HMAC Generator", "hmac-generator", "H#", "Encode", "Compute HMAC signatures.",
            "Compute HMAC-SHA1/256/384/512 of a message with a key given as text, hex or Base64. Useful for testing webhook signatures."),
        new("Image to Base64", "image-to-base64", "🖼", "Encode", "Convert images to Base64 data URIs.",
            "Choose an image (up to 5 MB) to get a data URI, an HTML <img> tag and a CSS background snippet. The file never leaves your browser."),
        new("Certificate Decoder", "certificate-decoder", "📜", "Encode", "Decode PEM X.509 certificates.",
            "Paste a PEM certificate to see its subject, issuer, validity, serial number, key algorithm, subject alternative names and fingerprints."),

        // Generate
        new("Hash Generator", "hash-generator", "#", "Generate", "SHA-1, SHA-256, SHA-384, SHA-512.",
            "Type text to compute SHA hashes of its UTF-8 bytes instantly. Use the copy buttons to grab individual hashes."),
        new("GUID Generator", "guid-generator", "ID", "Generate", "Generate one or many GUIDs/UUIDs.",
            "Generate up to 1000 random version 4 GUIDs in standard, compact, braces or parentheses format."),
        new("UUID v7 Generator", "uuid-v7-generator", "v7", "Generate", "Time-ordered UUIDv7 identifiers.",
            "Generate RFC 9562 version 7 UUIDs, which start with a millisecond Unix timestamp so they sort by creation time and index well in databases. Paste a UUIDv7 to decode its timestamp."),
        new("Text to Slug", "slug-generator", "a-b", "Generate", "Create URL-friendly slugs from text.",
            "Converts each line of text into a URL slug: accents are removed, punctuation becomes a separator and repeated separators are collapsed. Choose the separator, case and an optional maximum length."),
        new("Password Generator", "password-generator", "🔒", "Generate", "Strong, secure random passwords.",
            "Passwords are created with a cryptographically secure random generator and always include at least one character from each selected set."),
        new("QR Code Generator", "qr-code", "▦", "Generate", "Create QR codes as PNG or SVG.",
            "Enter text or a URL, choose colors, size and error correction level, then download the QR code as PNG or SVG."),
        new("Dynamic QR Code", "dynamic-qr-code", "▦↻", "Generate", "Editable QR codes with scan analytics.",
            "Create a QR code that points to a short link, change its destination at any time and see scan counts by day and device. Demo mode: data is stored only in this browser."),
        new("Lorem Ipsum", "lorem-ipsum", "Lo", "Generate", "Placeholder text generator.",
            "Generate placeholder words, sentences or paragraphs, optionally starting with the classic 'Lorem ipsum dolor sit amet'."),
        new("Fake Data Generator", "fake-data", "👤", "Generate", "Realistic test data as JSON or CSV.",
            "Pick fields, a locale and a row count to generate realistic fake people, companies and addresses for testing and demos."),
        new("Cron Builder", "cron-builder", "⏲", "Generate", "Build cron expressions visually.",
            "Choose a schedule type and time to build a cron expression, then preview its next run times or open it in the Cron Explainer."),
        new(".gitignore Generator", "gitignore-generator", "⊘", "Generate", "Combine .gitignore templates.",
            "Select languages, frameworks, editors and operating systems to combine into a single .gitignore file."),

        // Convert
        new("Timestamp Converter", "timestamp-converter", "📅", "Convert", "Unix epoch to date and back.",
            "Converts Unix timestamps in seconds or milliseconds (detected automatically) to dates, and dates back to timestamps."),
        new("Unix Timestamp to Date", "unix-timestamp-to-date", "⏲→📅", "Convert", "Convert epoch timestamps to readable dates.",
            "Enter one or more Unix timestamps, one per line. Seconds and milliseconds are detected automatically and each is shown in UTC, your local time zone and ISO 8601."),
        new("Number Base Converter", "number-base", "01", "Convert", "Binary, octal, decimal, hex and more.",
            "Convert whole numbers of any size between bases 2 to 36. Negative numbers are supported."),
        new("Color Converter", "color-converter", "🎨", "Convert", "HEX, RGB, HSL and contrast checker.",
            "Enter a color as HEX, rgb() or hsl(), or use the picker. The contrast checker shows the WCAG ratio and AA/AAA results against a second color."),
        new("Case Converter", "case-converter", "Aa", "Convert", "camelCase, snake_case, kebab-case and more.",
            "Converts text into camelCase, PascalCase, snake_case, CONSTANT_CASE, kebab-case, Title Case and more. Each line is converted separately."),
        new("Time Zone Converter", "time-zone-converter", "🌍", "Convert", "Compare a time across time zones.",
            "Pick a date, time and source time zone to see the same moment in other zones. Add or remove zones as needed."),
        new("Byte Size Converter", "byte-size", "KB", "Convert", "Bytes, KB, MB, GB, KiB, MiB and more.",
            "Converts sizes between decimal units (KB = 1000 bytes) and binary units (KiB = 1024 bytes), plus bits."),
        new("Chmod Calculator", "chmod-calculator", "rwx", "Convert", "Unix permissions in octal and symbolic form.",
            "Tick read, write and execute for owner, group and others, or type an octal value like 755, to get the symbolic notation and chmod command."),
        new("HTML to PDF", "html-to-pdf", "PDF", "Convert", "Convert HTML or Markdown to PDF.",
            "Paste HTML or Markdown, choose page size, orientation and margins, then click Download PDF and pick Save as PDF in the print dialog. Scripts in the HTML are not executed."),
        new("Invoice / Quote Generator", "invoice-generator", "🧾", "Generate", "Create invoices and quotes in any currency.",
            "Fill in your business and client details, add line items, tax and discount, pick a currency, then click Download PDF and choose Save as PDF in the print dialog. Details are saved in your browser only."),

        // Inspect
        new("Text Statistics", "text-statistics", "¶", "Inspect", "Count words, characters, lines and bytes.",
            "Shows character, word, line, sentence and paragraph counts, UTF-8 byte size, estimated reading time and the most frequent words."),
        new("HTTP Status Codes", "http-status-codes", "200", "Inspect", "Searchable HTTP status code reference.",
            "Search by code or name to see what each HTTP status code means and when it is used."),
        new("User-Agent Parser", "user-agent-parser", "UA", "Inspect", "Detect browser, OS and device.",
            "Paste a User-Agent string, or use your own, to detect the browser, rendering engine, operating system, device type and bots."),
        new("CSS Generator", "css-generator", "✦", "Inspect", "Gradients and box shadows with live preview.",
            "Design CSS gradients and box shadows visually with a live preview, then copy the generated CSS."),
        new("Subnet / CIDR Calculator", "cidr-calculator", "/24", "Inspect", "IPv4 subnet, mask and host range.",
            "Enter an IPv4 address with a prefix (192.168.1.10/24) or a dotted subnet mask to get the network and broadcast addresses, usable host range, host count, subnet and wildcard masks."),
        new("Uptime & SSL Monitor", "uptime-monitor", "⏻", "Inspect", "Check uptime and SSL expiry from several regions.",
            "Add one check endpoint per region (saved in your browser), enter a URL and click Check to see HTTP status, response time and SSL certificate expiry from each region."),

        // Engineering
        new("Cable Size & Voltage Drop", "cable-size-calculator", "⚡", "Engineering", "Electrical cable sizing and voltage drop (IEC / NEC).",
            "Choose IEC (mm²) or NEC (AWG/kcmil), the supply system, conductor material and insulation, then enter voltage, load, length and the allowed voltage drop. The smallest cable that satisfies both ampacity and voltage drop is recommended."),
        new("Ohm's Law & Power", "ohms-law-calculator", "Ω", "Engineering", "Calculate voltage, current, resistance and power.",
            "Enter any two of voltage, current, resistance and power; the other two are calculated using V = I×R and P = V×I. The two most recently edited fields are used as inputs."),
        new("Power Factor Correction", "power-factor-correction", "cosφ", "Engineering", "Size capacitor banks in kVAR and µF.",
            "Enter active power, voltage, frequency and the existing and target power factor to get the required capacitor kVAR, capacitance per phase (star or delta) and the reduction in current."),
        new("kW, kVA & Amps Converter", "power-converter", "kVA", "Engineering", "Convert between amps, kW, kVA and hp.",
            "Choose single-phase, three-phase or DC, enter the voltage and power factor, then enter a known current, kW, kVA or hp value to calculate the others."),
        new("Resistor Color Code", "resistor-color-code", "▮▮▮", "Engineering", "Decode 4, 5 and 6 band resistor colors.",
            "Pick the number of bands and the color of each band to get the resistance, tolerance range and temperature coefficient."),
        new("Transformer Current & Fault", "transformer-calculator", "⧖", "Engineering", "Full-load and short-circuit current from kVA and %Z.",
            "Enter the transformer rating, impedance and primary and secondary voltages to get full-load current on each winding and the maximum (infinite bus) short-circuit current."),
        new("LED Resistor Calculator", "led-resistor-calculator", "💡", "Engineering", "Series resistor value and wattage for LEDs.",
            "Enter supply voltage, LED forward voltage, current and number of LEDs in series to get the exact and nearest E24 resistor, actual current and required power rating."),
        new("Voltage Divider Calculator", "voltage-divider-calculator", "V÷", "Engineering", "Output voltage or R2 for a resistor divider.",
            "Enter Vin and R1, then either R2 to calculate Vout or a target Vout to find R2 (exact and nearest E24). An optional load resistance is taken into account. Divider current and resistor power are also shown."),
        new("AWG to mm² Converter", "awg-to-mm2", "AWG", "Engineering", "Wire gauge to mm² and diameter.",
            "Pick an AWG size (4/0 to 40) to see its cross-section in mm² and kcmil, diameter and copper resistance, or enter a metric size in mm² to find the nearest AWG."),
        new("Ampacity Table", "ampacity-table", "A≈", "Engineering", "NEC 310.16 ampacity with temperature and bundling corrections.",
            "Choose copper or aluminum, the ambient temperature and the number of current-carrying conductors to see corrected ampacities at 60, 75 and 90 °C. Enter a load to highlight the smallest conductor that carries it."),
        new("BOM Cost Calculator", "bom-calculator", "BOM", "Engineering", "Consolidate a BOM and calculate order quantities and cost.",
            "Paste a CSV bill of materials with a header row (Reference, Qty, Value, MPN, Unit Price). Optional MOQ, Multiple and Price Breaks columns are applied. Parts are merged by MPN, order quantities include the attrition percentage, and the cost per board and order total are shown. Copy the purchase list as CSV."),
                    new("PCB Trace Width Calculator", "pcb-trace-width-calculator", "PCB", "Engineering", "IPC-2221 trace width for a given current.",
                        "Enter the current, allowed temperature rise, copper weight and trace length to get the minimum width for external and internal layers, plus resistance, voltage drop and power loss."),
                    new("555 Timer Calculator", "555-timer-calculator", "555", "Engineering", "Frequency, duty cycle and pulse width of a 555.",
                        "Choose astable or monostable mode and enter R1, R2 and C to get frequency, period, high/low time and duty cycle, or the one-shot pulse width."),
                    new("Battery Life Calculator", "battery-life-calculator", "🔋", "Engineering", "Estimate battery runtime with sleep/active duty cycling.",
                        "Enter battery capacity, active and sleep current, the percentage of time active and the usable capacity to get the average current and expected battery life."),
                    new("SMD Resistor Code", "smd-resistor-code", "103", "Engineering", "Decode SMD resistor markings.",
                        "Type the marking printed on an SMD resistor: 3-digit (103), 4-digit (1002), R notation (4R7) or EIA-96 (01C) to get the resistance."),

        new("JSON Schema Validator", "json-schema-validator", "{✓}", "Format", "Validate JSON against a JSON Schema.",
            "Paste a JSON Schema and JSON data, then click Validate. Supports types, required, properties, patterns, formats, enums, array rules, local and external $ref (paste referenced schemas with an $id), dependentRequired/dependentSchemas, unevaluatedProperties, and allOf/anyOf/oneOf/not/if-then-else. Errors are listed by JSON path."),
        new("JSON Diff", "json-diff", "{±}", "Format", "Compare two JSON documents by path.",
            "Paste the original and changed JSON to list added, removed and changed values with their JSON paths. Key order is ignored; enable Ignore array order to compare arrays as sets."),
        new("OpenAPI to C#", "openapi-to-csharp", "OA#", "Format", "Generate C# models from OpenAPI/Swagger.",
            "Paste an OpenAPI 3.x or Swagger 2.0 document in JSON or YAML. Classes or records and enums are generated from components.schemas or definitions, with optional [JsonPropertyName] attributes."),
        new("GraphQL to C#", "graphql-to-csharp", "GQ#", "Format", "Generate C# types from a GraphQL schema.",
            "Paste GraphQL SDL to generate C# classes or records for types and inputs, interfaces, enums and union marker interfaces. Non-null fields become required; lists become List<T>."),
        new("SVG Optimizer", "svg-optimizer", "SVG", "Format", "Minify and clean up SVG files.",
            "Paste SVG markup and click Optimize. Comments, metadata, editor namespaces and empty groups are removed and coordinates are rounded. Compare the before and after preview and size."),
        new("Docker Compose ↔ Kubernetes", "docker-compose-to-kubernetes", "☸", "Convert", "Convert docker-compose.yml to Kubernetes manifests and back.",
            "Compose services become Deployments, Services and PersistentVolumeClaims (with env, ports, resources, health checks and volumes). Kubernetes Deployments, StatefulSets and Services can be converted back to a Compose file. Unsupported features are listed as warnings."),
        new("Epoch Batch Converter", "epoch-batch-converter", "⏲", "Convert", "Convert many Unix timestamps at once.",
            "Paste one timestamp or date per line. Seconds, milliseconds, microseconds and nanoseconds are detected by length (or choose the unit). Results show UTC, local time and epoch seconds and milliseconds, and can be copied as CSV."),
    ];

    public static IEnumerable<IGrouping<string, ToolInfo>> ByCategory =>
        All.GroupBy(t => t.Category).OrderBy(g => Array.IndexOf(Categories, g.Key));

    public static ToolInfo? Find(string href) => All.FirstOrDefault(t => t.Href == href);
}
