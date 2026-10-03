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
                "power-converter", "resistor-color-code", "transformer-calculator", "led-resistor-calculator",
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
    ];

    public static IEnumerable<IGrouping<string, ToolInfo>> ByCategory =>
        All.GroupBy(t => t.Category).OrderBy(g => Array.IndexOf(Categories, g.Key));

    public static ToolInfo? Find(string href) => All.FirstOrDefault(t => t.Href == href);
}
