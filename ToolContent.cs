namespace DevToolsHub;

public record Faq(string Question, string Answer);

public record ToolSeo(string Keyword, string About, string[] Steps, string? Example, Faq[] Faqs);

/// <summary>Search-friendly long-form content shown under each tool.</summary>
public static class ToolContent
{
    private static readonly Faq Privacy = new("Is my data safe?",
        "Yes. The tool runs entirely in your browser using WebAssembly. Nothing you type or upload is sent to a server, logged or stored.");

    private static readonly Faq Free = new("Is it free?",
        "Yes, completely free with no sign-up, no limits and no watermarks. It also works offline once the page has loaded.");

    private static ToolSeo S(string keyword, string about, string[] steps, string? example, params Faq[] faqs) =>
        new(keyword, about, steps, example, [.. faqs, Privacy, Free]);

    private static readonly Dictionary<string, ToolSeo> Content = new(StringComparer.OrdinalIgnoreCase)
    {
        ["json-formatter"] = S("JSON formatter and validator",
            "JSON (JavaScript Object Notation) is the most common data format for APIs and configuration files. This online JSON formatter pretty-prints minified JSON with consistent indentation, validates the syntax and pinpoints errors, and can minify JSON again for production.",
            ["Paste or type your JSON into the input box.", "Click Format to beautify it, or Minify to compact it.", "Copy the result with one click."],
            "{\"name\":\"Ada\",\"skills\":[\"math\",\"code\"]}  →  nicely indented, multi-line JSON",
            new Faq("Why does my JSON fail to validate?", "Common causes are trailing commas, single quotes instead of double quotes, unquoted property names and comments, none of which are allowed in strict JSON."),
            new Faq("What is the difference between format and minify?", "Formatting adds whitespace and line breaks for readability; minifying removes all unnecessary whitespace to reduce size.")),

        ["json-to-csharp"] = S("JSON to C# class converter",
            "Convert any JSON sample into strongly typed C# classes or records. Property types (string, int, double, bool, DateTime, nested objects and lists) are inferred automatically, which saves time when consuming REST APIs with System.Text.Json or Newtonsoft.Json.",
            ["Paste a JSON sample.", "Choose class or record output and a root type name.", "Copy the generated C# code into your project."],
            "{\"id\":1,\"tags\":[\"a\"]}  →  public class Root { public int Id { get; set; } public List<string> Tags { get; set; } }",
            new Faq("Does it handle nested objects and arrays?", "Yes. Nested objects become their own classes and arrays become List<T> of the inferred element type."),
            new Faq("Which serializer attributes are generated?", "Property names are converted to PascalCase and [JsonPropertyName] attributes keep the original JSON names.")),

        ["format-converter"] = S("JSON, YAML, XML and CSV converter",
            "Convert data between JSON, YAML, XML and CSV formats in one place. Useful for turning API responses into spreadsheets, Kubernetes YAML into JSON, or legacy XML into modern formats.",
            ["Choose the input and output formats.", "Paste your data.", "Copy or download the converted output."],
            "JSON array of objects  →  CSV with a header row",
            new Faq("Can I convert nested JSON to CSV?", "CSV is flat, so nested objects are best converted to JSON or YAML. Arrays of flat objects convert cleanly to CSV.")),

        ["xml-formatter"] = S("XML formatter and beautifier",
            "Pretty-print and validate XML documents online. The formatter indents elements consistently, preserves attributes and comments, and reports malformed markup with a clear error message.",
            ["Paste your XML.", "Click Format or Minify.", "Copy the result."], null,
            new Faq("Does it validate against an XSD?", "No, it checks that the XML is well-formed (correct nesting and syntax) but does not validate against a schema.")),

        ["yaml-validator"] = S("YAML validator",
            "Check YAML files such as Kubernetes manifests, Docker Compose files and CI pipelines for syntax errors. The validator parses the YAML and reports the exact line and column of any problem.",
            ["Paste your YAML.", "Click Validate.", "Fix any reported error and validate again."], null,
            new Faq("Does it validate against a schema?", "No, it checks that the YAML is syntactically valid but does not validate it against a schema such as a Kubernetes or Compose specification.")),

        ["sql-formatter"] = S("SQL formatter and beautifier",
            "Make long, single-line SQL queries readable. This SQL beautifier puts each clause (SELECT, FROM, JOIN, WHERE, GROUP BY, ORDER BY) on its own line, indents subqueries and conditions, and can uppercase keywords. It works with SQL Server, PostgreSQL, MySQL, SQLite and Oracle syntax.",
            ["Paste your SQL query.", "Choose indent size and keyword casing.", "Click Format and copy the result."],
            "select id,name from users where active=1  →  SELECT id, name FROM users WHERE active = 1 (one clause per line)",
            new Faq("Which SQL dialects are supported?", "The formatter is dialect-agnostic and handles standard SQL plus common syntax from T-SQL, PostgreSQL and MySQL, including brackets, backticks and double-quoted identifiers."),
            new Faq("Will it change my query logic?", "No. Only whitespace and keyword casing change; strings, comments and identifiers are preserved exactly.")),

        ["regex-tester"] = S("regex tester",
            "Test regular expressions live against sample text. Matches and capture groups are highlighted as you type, with support for common options like ignore case and multiline. It uses the .NET regular expression engine.",
            ["Enter a regular expression pattern.", "Paste test text.", "Inspect the highlighted matches and groups."],
            "Pattern \\d{3}-\\d{4} matches 555-1234",
            new Faq("Which regex flavor does it use?", "The .NET flavor, which is very close to PCRE and JavaScript for everyday patterns and also supports named groups and lookbehind.")),

        ["cron-explainer"] = S("cron expression explainer with time zones",
            "Paste any cron expression and get a plain-English description plus the next scheduled run times. Supports standard 5-field cron and 6-field cron with seconds, as used in Linux crontab, Kubernetes CronJobs, GitHub Actions, Hangfire and Quartz.",
            ["Enter a cron expression.", "Read the human-readable description.", "Check the list of upcoming run times."],
            "*/15 9-17 * * 1-5  →  every 15 minutes, 09:00–17:59, Monday to Friday",
            new Faq("What do the five cron fields mean?", "Minute, hour, day of month, month and day of week, in that order. An asterisk means \"every\".")),

        ["diff-checker"] = S("text diff checker",
            "Compare two blocks of text or code and see exactly what changed. Added and removed lines are highlighted side by side, which is handy for comparing configs, API responses or document versions.",
            ["Paste the original text on the left.", "Paste the changed text on the right.", "Review the highlighted differences."], null,
            new Faq("Does it compare character by character?", "It compares line by line, which is the most readable view for code and configuration files.")),

        ["html-to-pdf"] = S("HTML to PDF converter",
            "Turn HTML or Markdown into a clean PDF document for invoices, reports, receipts and documentation. Set the page size, orientation and margins with CSS @page rules and preview the result before exporting. Conversion uses your browser's own print engine, so CSS, web fonts and images render exactly as they do on screen.",
            ["Paste HTML or Markdown.", "Choose page size, orientation and margins.", "Click Download PDF and select Save as PDF."],
            "<h1>Invoice</h1><table>...</table>  →  invoice.pdf",
            new Faq("Does JavaScript in my HTML run?", "No. The document is rendered in a sandboxed frame with scripts disabled, so only HTML and CSS are used."),
            new Faq("How do I add page breaks?", "Use the CSS rule page-break-before: always (or break-before: page) on the element that should start a new page.")),

        ["markdown-preview"] = S("Markdown editor with live preview",
            "Write Markdown and see the rendered HTML instantly. Supports GitHub-flavored Markdown including tables, task lists, fenced code blocks and autolinks, making it ideal for drafting README files.",
            ["Type Markdown in the editor.", "Watch the live preview update.", "Copy the generated HTML if needed."], null,
            new Faq("Does it support GitHub-flavored Markdown?", "Yes, including tables, strikethrough, task lists and fenced code blocks.")),

        ["json-path"] = S("JSONPath tester",
            "Evaluate JSONPath expressions against your JSON and see the matching values instantly. JSONPath is to JSON what XPath is to XML and is used in tools like Postman, Kubernetes and many API gateways.",
            ["Paste JSON.", "Enter a JSONPath expression such as $.store.book[*].author.", "View the matched results."],
            "$..price  →  all price values anywhere in the document",
            new Faq("What does $.. mean?", "The recursive descent operator searches for the property at every depth of the document.")),

        ["base64"] = S("Base64 encoder and decoder",
            "Encode text to Base64 or decode Base64 back to readable text. Base64 is used to embed binary data in JSON, emails, data URLs and HTTP Basic authentication headers. Full UTF-8 support ensures emojis and non-Latin characters round-trip correctly.",
            ["Paste text or a Base64 string.", "Click Encode or Decode.", "Copy the result."],
            "Hello  →  SGVsbG8=",
            new Faq("Is Base64 encryption?", "No. Base64 is an encoding, not encryption. Anyone can decode it, so never use it to protect secrets.")),

        ["url-encoder"] = S("URL encoder and decoder",
            "Percent-encode text so it is safe to use in URLs and query strings, or decode encoded URLs back to readable text.",
            ["Paste text or an encoded URL.", "Click Encode or Decode.", "Copy the result."],
            "a b&c  →  a%20b%26c",
            new Faq("When do I need URL encoding?", "Whenever a query string value contains spaces or reserved characters such as &, =, ?, # or /.")),

        ["html-entities"] = S("HTML entity encoder and decoder",
            "Escape special characters like <, >, & and quotes into HTML entities, or decode entities back to characters. Escaping prevents broken markup and cross-site scripting (XSS) when displaying user content.",
            ["Paste text or HTML.", "Choose Encode or Decode.", "Copy the result."],
            "<b>  →  &lt;b&gt;"),

        ["string-escape"] = S("string escape and unescape tool",
            "Escape or unescape strings for JSON, C#, JavaScript, SQL, regex and more. Handles quotes, backslashes, newlines and Unicode so you can safely paste text into source code.",
            ["Paste your text.", "Choose the target language.", "Escape or unescape and copy."], null),

        ["jwt-decoder"] = S("JWT decoder",
            "Decode JSON Web Tokens to inspect the header and payload claims, including expiry (exp), issued-at (iat) and audience. Timestamps are shown as readable dates so you can quickly debug authentication issues.",
            ["Paste a JWT (three Base64URL parts separated by dots).", "Inspect the decoded header and payload.", "Check whether the token has expired."], null,
            new Faq("Does decoding verify the signature?", "No. Decoding only reads the token. Anyone can decode a JWT, which is why you should never put secrets in the payload."),
            new Faq("Is it safe to paste production tokens?", "The token never leaves your browser, but treat live tokens as credentials and prefer test tokens when possible.")),

        ["jwt-generator"] = S("JWT generator (HS256, HS384, HS512)",
            "Create signed JSON Web Tokens for testing APIs. Edit the payload, choose an HMAC algorithm (HS256, HS384, HS512) and a secret, and get a valid token instantly.",
            ["Edit the JSON payload.", "Choose the algorithm and enter a secret.", "Copy the generated token."], null),

        ["hmac-generator"] = S("HMAC generator",
            "Generate HMAC signatures using SHA-256, SHA-384, SHA-512, SHA-1 or MD5. HMAC is used to verify webhooks (GitHub, Stripe, Shopify) and sign API requests.",
            ["Enter the message.", "Enter the secret key.", "Choose the algorithm and copy the hex or Base64 signature."], null,
            new Faq("How do I verify a webhook signature?", "Compute the HMAC of the raw request body with your webhook secret and compare it to the signature header sent by the provider.")),

        ["image-to-base64"] = S("image to Base64 converter",
            "Convert images (PNG, JPG, GIF, SVG, WebP) into Base64 data URLs that can be embedded directly in HTML, CSS or JSON without a separate file request.",
            ["Drop or choose an image file.", "Copy the data URL, HTML img tag or CSS snippet."], null,
            new Faq("When should I inline images as Base64?", "For small icons and images under a few kilobytes. Larger images are better served as separate cached files.")),

        ["certificate-decoder"] = S("SSL certificate decoder",
            "Decode PEM-encoded X.509 certificates to view the subject, issuer, validity dates, serial number and subject alternative names. Useful for debugging TLS/HTTPS configuration.",
            ["Paste a certificate starting with -----BEGIN CERTIFICATE-----.", "Review the decoded fields and expiry date."], null),

        ["hash-generator"] = S("hash generator (MD5, SHA-1, SHA-256, SHA-512)",
            "Generate MD5, SHA-1, SHA-256, SHA-384 and SHA-512 hashes of any text at once. Use them to verify file integrity, compare values or generate checksums.",
            ["Type or paste text.", "All hashes update instantly.", "Copy the one you need."],
            "hello  →  SHA-256 2cf24dba5fb0a30e...",
            new Faq("Should I use MD5 for passwords?", "No. MD5 and SHA hashes are too fast for password storage. Use bcrypt, scrypt, Argon2 or PBKDF2 instead.")),

        ["guid-generator"] = S("GUID / UUID generator",
            "Generate random version-4 GUIDs (UUIDs) in bulk, with options for uppercase, braces and hyphen-free formats. GUIDs are used as unique database keys, correlation IDs and identifiers in distributed systems.",
            ["Choose how many GUIDs you need.", "Pick the format.", "Copy them all with one click."], null,
            new Faq("What is the difference between a GUID and a UUID?", "They are the same thing. GUID is Microsoft's name for a UUID."),
            new Faq("Can two GUIDs collide?", "In practice no. Version-4 UUIDs have 122 random bits, making collisions astronomically unlikely.")),

        ["password-generator"] = S("strong password generator",
            "Generate strong, random passwords using a cryptographically secure random number generator. Choose length and character sets, and see an estimate of password strength.",
            ["Choose the length.", "Select which character types to include.", "Generate and copy."], null,
            new Faq("How long should a password be?", "At least 16 characters for important accounts. Length matters more than complexity.")),

        ["qr-code"] = S("QR code generator",
            "Create QR codes for URLs, text, Wi-Fi details and more. Customize colors, size and error correction, then download as PNG or SVG for print and web.",
            ["Enter the text or URL.", "Adjust colors and error correction.", "Download PNG or SVG."], null,
            new Faq("Do the QR codes expire?", "No. These are static QR codes that encode the content directly and work forever."),
            new Faq("Which error correction level should I use?", "M is a good default. Use H if you plan to place a logo over the code or print it on rough surfaces.")),

        ["lorem-ipsum"] = S("Lorem Ipsum generator",
            "Generate placeholder text by paragraphs, sentences or words for mockups, wireframes and layout testing.",
            ["Choose the amount and unit.", "Click Regenerate for new text.", "Copy it into your design."], null),

        ["fake-data"] = S("fake test data generator",
            "Generate realistic fake data such as names, emails, addresses, companies, phone numbers and IBANs in many locales. Export as JSON or CSV to seed databases and test applications without using real personal data.",
            ["Pick the fields you need.", "Choose the row count and locale.", "Generate and download JSON or CSV."], null,
            new Faq("Is the data real?", "No. All values are randomly generated and do not belong to real people.")),

        ["cron-builder"] = S("cron expression generator",
            "Build cron expressions visually without memorizing syntax. Pick a schedule such as every N minutes, daily, weekly or monthly and get a valid cron string with upcoming run times.",
            ["Choose a schedule type.", "Set the time and days.", "Copy the generated cron expression."], null),

        ["gitignore-generator"] = S(".gitignore generator",
            "Create a .gitignore file by combining templates for languages, frameworks, editors and operating systems, including .NET, Node, Python, Java, Unity, VS Code, JetBrains, Windows and macOS.",
            ["Select your stack.", "Review the combined file.", "Download .gitignore into your repository root."], null),

        ["timestamp-converter"] = S("Unix timestamp converter",
            "Convert Unix epoch timestamps (seconds or milliseconds) to human-readable dates in UTC and local time, and convert dates back to timestamps.",
            ["Paste a timestamp or pick a date.", "See the converted values.", "Copy the format you need."],
            "1700000000  →  2023-11-14 22:13:20 UTC",
            new Faq("What is a Unix timestamp?", "The number of seconds since 1 January 1970 00:00:00 UTC, also called epoch time.")),

        ["number-base"] = S("number base converter",
            "Convert numbers between binary, octal, decimal, hexadecimal and any base from 2 to 36. Supports arbitrarily large integers.",
            ["Enter a number.", "Select its base.", "Read the conversions."],
            "255  →  0b11111111, 0o377, 0xff"),

        ["color-converter"] = S("color converter and contrast checker",
            "Convert colors between HEX, RGB and HSL and check WCAG contrast ratios for accessibility. Instantly see whether text and background colors pass AA and AAA requirements.",
            ["Enter a color in any format or use the picker.", "Copy HEX, RGB or HSL.", "Choose a background to check contrast."], null,
            new Faq("What contrast ratio do I need?", "WCAG AA requires 4.5:1 for normal text and 3:1 for large text. AAA requires 7:1 and 4.5:1.")),

        ["case-converter"] = S("text case converter",
            "Convert text between camelCase, PascalCase, snake_case, kebab-case, CONSTANT_CASE, Title Case and more. Handy for renaming variables, database columns and URL slugs.",
            ["Type or paste text.", "See every case at once.", "Copy the one you need."],
            "hello world  →  helloWorld, HelloWorld, hello_world, hello-world"),

        ["time-zone-converter"] = S("time zone converter",
            "Convert a date and time across multiple time zones at once, with automatic daylight-saving handling. Ideal for scheduling meetings with distributed teams.",
            ["Pick a date, time and source zone.", "Add the zones you care about.", "Read the converted local times."], null),

        ["byte-size"] = S("byte size converter",
            "Convert between bytes, KB, MB, GB, TB and binary units KiB, MiB, GiB, TiB. Understand the difference between decimal (1000) and binary (1024) units.",
            ["Enter a size.", "Pick its unit.", "Read both decimal and binary conversions."], null,
            new Faq("Why does my 1 TB drive show 931 GB?", "Drive makers use decimal units (1 TB = 10¹² bytes) while many operating systems show binary units (1 TiB = 2⁴⁰ bytes).")),

        ["chmod-calculator"] = S("chmod calculator",
            "Calculate Unix and Linux file permissions. Convert between octal notation (755, 644) and symbolic notation (rwxr-xr-x) and get the matching chmod command.",
            ["Tick the permissions for owner, group and others, or type an octal value.", "Read the symbolic notation.", "Copy the chmod command."],
            "755  →  rwxr-xr-x",
            new Faq("What does chmod 755 mean?", "The owner can read, write and execute; group and others can read and execute."),
            new Faq("What does chmod 644 mean?", "The owner can read and write; group and others can only read. It is the usual permission for regular files.")),

        ["cable-size-calculator"] = S("cable size and voltage drop calculator (IEC 60364 / NEC)",
            "Size electrical cables for single-phase, three-phase and DC circuits using IEC 60364-5-52 metric sizes or NEC Table 310.16 AWG and kcmil sizes. The calculator checks current-carrying capacity with a derating factor and voltage drop, and recommends the smallest conductor that satisfies both.",
            ["Pick IEC or NEC, the system type, conductor and insulation.", "Enter voltage, load current or power, cable length and maximum voltage drop.", "Read the recommended cable size, voltage drop and power loss."],
            "400 V 3-phase, 32 A, 50 m, Cu PVC  →  6 mm² (≈ 2.1 % drop)",
            new Faq("What voltage drop is acceptable?", "IEC 60364-5-52 suggests 3 % for lighting and 5 % for other uses on public supplies; NEC informational notes recommend 3 % on branch circuits and 5 % overall."),
            new Faq("What is the derating factor?", "The product of correction factors for ambient temperature and grouping of circuits. Use 1.0 for a single circuit at 30 °C.")),

        ["ohms-law-calculator"] = S("Ohm's law calculator",
            "Solve Ohm's law and electrical power equations instantly. Enter any two of voltage, current, resistance and power to find the remaining values for DC or resistive AC circuits.",
            ["Enter any two known values.", "Read the calculated values.", "Edit a field to recalculate."],
            "230 V, 10 A  →  23 Ω, 2300 W"),

        ["power-factor-correction"] = S("power factor correction capacitor calculator (kVAR and µF)",
            "Calculate the capacitor bank size needed to raise power factor for single-phase and three-phase loads. Shows kVAR, capacitance per phase in star and delta, and the drop in current, apparent power and line losses.",
            ["Choose the system, voltage and frequency.", "Enter active power and existing and target power factor.", "Read the required kVAR and capacitance."],
            "100 kW, PF 0.75 → 0.95  →  55.3 kVAR",
            new Faq("Why correct power factor?", "A higher power factor lowers current, which reduces cable losses, voltage drop and utility reactive-power charges and frees transformer capacity.")),

        ["power-converter"] = S("kW, kVA and amps converter",
            "Convert electrical load values between current (A), active power (kW), apparent power (kVA) and motor horsepower for single-phase, three-phase and DC systems using the voltage and power factor.",
            ["Choose the system, voltage and power factor.", "Pick the known quantity and enter its value.", "Read the calculated amps, kW, kVA and kVAR."],
            "400 V 3-phase, 22 kW, PF 0.85  →  37.4 A, 25.9 kVA",
            new Faq("What is the difference between kW and kVA?", "kW is real (active) power that does work; kVA is apparent power. kW = kVA × power factor.")),

        ["resistor-color-code"] = S("resistor color code calculator",
            "Decode the colored bands on through-hole resistors. Supports 4-band, 5-band and 6-band codes with tolerance and temperature coefficient.",
            ["Choose the number of bands.", "Select the color of each band.", "Read the resistance and tolerance."],
            "Brown, Black, Red, Gold  →  1 kΩ ± 5 %",
            new Faq("Which end do I read from?", "Start from the band closest to a lead; the tolerance band (often gold or silver) is usually spaced slightly apart on the other end.")),

        ["transformer-calculator"] = S("transformer fault current calculator (kVA and %Z)",
            "Calculate the rated full-load current of single-phase and three-phase transformers on both windings, and the maximum short-circuit current at the secondary terminals from the percentage impedance.",
            ["Enter the kVA rating and %Z.", "Enter primary and secondary voltages.", "Read full-load and fault currents."],
            "1000 kVA, 11 kV / 400 V, 6 %Z  →  1443 A, 24.1 kA",
            new Faq("Why is the real fault current lower?", "The calculation assumes an infinite upstream source. Utility and cable impedance reduce the actual fault current.")),

        ["led-resistor-calculator"] = S("LED resistor calculator",
            "Find the current-limiting resistor for one or more LEDs in series, including the nearest standard E24 value, actual current and resistor wattage.",
            ["Enter supply voltage and LED forward voltage.", "Enter the desired current and number of LEDs.", "Read the resistor value and power rating."],
            "12 V, red LED 2 V, 20 mA  →  500 Ω → 510 Ω (E24)",
            new Faq("Why round up to the next standard value?", "A higher resistance keeps the current at or below the target, protecting the LED.")),

        ["text-statistics"] = S("word and character counter",
            "Count words, characters (with and without spaces), lines, sentences, paragraphs and UTF-8 bytes, and estimate reading time. Useful for essays, tweets, meta descriptions and SMS limits.",
            ["Paste your text.", "Read the statistics instantly."], null),

        ["http-status-codes"] = S("HTTP status codes reference",
            "A searchable list of HTTP response status codes with clear explanations: 1xx informational, 2xx success, 3xx redirects, 4xx client errors and 5xx server errors.",
            ["Search by number or name.", "Read what the code means and when it is used."], null,
            new Faq("What is the difference between 401 and 403?", "401 means you are not authenticated; 403 means you are authenticated but not allowed to access the resource."),
            new Faq("What is the difference between 301 and 302?", "301 is a permanent redirect that search engines follow and transfer ranking to; 302 is temporary.")),

        ["user-agent-parser"] = S("User-Agent parser",
            "Parse a User-Agent string to identify the browser, version, rendering engine, operating system and device type, and detect bots and crawlers.",
            ["Paste a User-Agent or click Use my browser.", "Read the detected details."], null),

        ["css-generator"] = S("CSS gradient and box-shadow generator",
            "Design CSS linear and radial gradients and box shadows with live preview and copy-ready CSS code.",
            ["Choose gradient colors and angle.", "Adjust the shadow sliders.", "Copy the generated CSS."], null),

        ["unix-timestamp-to-date"] = S("Unix timestamp to date converter",
            "Convert Unix epoch timestamps to human-readable dates. Paste one timestamp or a whole list from logs, and each value is shown in UTC, your local time and ISO 8601. Seconds and milliseconds are detected automatically.",
            ["Paste one or more Unix timestamps, one per line.", "Read the UTC, local and ISO 8601 dates.", "Copy the converted results."],
            "1700000000  →  2023-11-14T22:13:20Z",
            new Faq("What is a Unix timestamp?", "It is the number of seconds elapsed since 1 January 1970 00:00:00 UTC (the Unix epoch), ignoring leap seconds."),
            new Faq("How do I know if a timestamp is in seconds or milliseconds?", "Current timestamps in seconds have 10 digits; in milliseconds they have 13 digits. The converter detects this automatically.")),

        ["csv-to-json"] = S("CSV to JSON converter",
            "Turn CSV exports from Excel, Google Sheets or databases into JSON. The header row becomes property names, quoted fields and embedded commas are handled correctly, and numbers and booleans can be detected automatically.",
            ["Paste your CSV data.", "Choose the delimiter and output shape (objects or arrays).", "Copy or download the JSON."],
            "name,age / Ada,36  →  [{\"name\":\"Ada\",\"age\":36}]",
            new Faq("Can I use semicolon or tab separated files?", "Yes. Choose a custom delimiter such as semicolon, tab or pipe."),
            new Faq("Are quoted fields with commas supported?", "Yes. Fields wrapped in double quotes may contain commas, line breaks and escaped quotes.")),

        ["uuid-v7-generator"] = S("UUID v7 generator",
            "UUID version 7 (RFC 9562) embeds a Unix millisecond timestamp in the first 48 bits, so IDs sort by creation time. That makes them a great database primary key because inserts stay index-friendly, unlike random UUID v4.",
            ["Choose how many UUIDs to generate.", "Click Generate and copy the list.", "Paste an existing UUIDv7 to decode its timestamp."], null,
            new Faq("UUID v4 vs v7: which should I use?", "Use v7 when IDs are database keys or need to sort by time; use v4 when you want no embedded information at all."),
            new Faq("Is UUID v7 unique?", "Yes. Besides the timestamp it contains 74 random bits, so collisions are practically impossible.")),

        ["cidr-calculator"] = S("subnet and CIDR calculator",
            "Calculate IPv4 subnet details from CIDR notation: network address, broadcast address, first and last usable host, number of hosts, subnet mask and wildcard mask. Handy for cloud VNets/VPCs, firewall rules and network planning.",
            ["Enter an IP address with prefix, e.g. 10.0.0.0/24.", "Read the network range, mask and host count.", "Copy any value you need."],
            "192.168.1.0/24  →  mask 255.255.255.0, hosts 192.168.1.1 – 192.168.1.254 (254 usable)",
            new Faq("How many hosts are in a /24?", "A /24 has 256 addresses, of which 254 are usable hosts (network and broadcast are reserved)."),
            new Faq("Why does Azure or AWS show fewer usable addresses?", "Cloud providers reserve extra addresses in each subnet (Azure and AWS reserve 5).")),

        ["sql-to-csharp"] = S("SQL to C# class generator",
            "Generate C# classes or records from SQL CREATE TABLE statements. Column types from SQL Server, PostgreSQL, MySQL and SQLite are mapped to C# types, and nullable columns become nullable properties, ideal for Entity Framework Core or Dapper models.",
            ["Paste one or more CREATE TABLE statements.", "Choose class or record output.", "Copy the generated C# code."],
            "CREATE TABLE Users (Id INT NOT NULL, Name NVARCHAR(50) NULL)  →  public class Users { public int Id { get; set; } public string? Name { get; set; } }",
            new Faq("Which SQL types are supported?", "Common numeric, text, date/time, boolean, GUID/uuid and binary types across the major databases are mapped to their C# equivalents.")),

        ["slug-generator"] = S("URL slug generator",
            "Convert blog titles and product names into clean, SEO-friendly URL slugs. Accents are transliterated, punctuation is removed and spaces become hyphens.",
            ["Type or paste your title.", "Choose the separator and casing.", "Copy the slug."],
            "Héllo, World! 2024  →  hello-world-2024",
            new Faq("Why are slugs important for SEO?", "Short, readable, keyword-rich URLs are easier to share and help search engines understand the page topic.")),

        ["awg-to-mm2"] = S("AWG to mm² wire gauge converter",
            "Convert American Wire Gauge (AWG) sizes to metric cross-sectional area in mm² and diameter in mm, or find the nearest AWG for a metric cable size. Useful when working with US and IEC wiring standards side by side.",
            ["Select an AWG size or enter a mm² value.", "Read the equivalent area and diameter."],
            "12 AWG  →  3.31 mm² (2.05 mm diameter)",
            new Faq("Is a higher AWG number a thicker wire?", "No. Higher AWG numbers mean thinner wires; every 6 gauges roughly halves the diameter."),
            new Faq("What is the closest metric size to 10 AWG?", "10 AWG is 5.26 mm², so 6 mm² is the nearest standard metric size above it.")),

        ["voltage-divider-calculator"] = S("voltage divider calculator",
            "Calculate the output voltage of a two-resistor voltage divider, or find the resistor value needed for a target output. The current through the divider and the power dissipated in each resistor are shown too.",
            ["Enter the input voltage and resistor values.", "Read Vout, current and power.", "Or enter a target Vout to solve for R2."],
            "Vin 12 V, R1 10 kΩ, R2 4.7 kΩ  →  Vout ≈ 3.84 V",
            new Faq("What is the voltage divider formula?", "Vout = Vin × R2 / (R1 + R2)."),
            new Faq("Can I power a device from a voltage divider?", "Generally no. The load changes the output voltage; use a regulator to power circuits and dividers for signals or reference levels.")),

        ["invoice-generator"] = S("invoice and quote generator",
            "Create clean, professional invoices and quotes without accounting software or sign-up. Add your business and client details, line items, quantities and unit prices, then apply a discount and tax rate, and the totals are calculated automatically in your chosen currency. When you are done, export the document as a PDF ready to email or print.",
            ["Enter your business and client details.", "Add line items with quantity and unit price.", "Set discount and tax, then export the invoice or quote as PDF."],
            "2 × Web design @ 450.00 + 10% tax  →  Subtotal 900.00, Tax 90.00, Total 990.00",
            new Faq("What is the difference between an invoice and a quote?", "A quote (or estimate) is an offer sent before work starts; an invoice is a request for payment after goods or services are delivered. Switch between the two with one setting."),
            new Faq("Which currencies are supported?", "All major currencies can be selected, and amounts are formatted with the right symbol and number of decimal places throughout the document.")),

        ["dynamic-qr-code"] = S("dynamic QR code generator",
            "A dynamic QR code points to a short redirect link instead of encoding the final URL directly, so you can change where it leads after it has been shared and see how often it is scanned. This tool creates dynamic QR codes, lets you edit their destination at any time and counts scans. Codes and scan counts are stored in your browser.",
            ["Enter the destination URL and a name for the code.", "Download the generated QR code.", "Edit the destination or check the scan count whenever you need."],
            null,
            new Faq("What is the difference between a static and a dynamic QR code?", "A static QR code contains the destination URL itself and can never change. A dynamic QR code contains a redirect link, so the destination can be updated without reprinting the code."),
            new Faq("Where are my codes stored?", "In your browser's local storage, so they are tied to this browser and device. Clearing site data removes them.")),

        ["uptime-monitor"] = new("uptime and SSL certificate checker",
            "Check whether a website is up, how fast it responds and when its SSL/TLS certificate expires, from several regions at once. Because browsers cannot inspect TLS certificates or send requests from other locations, each region calls a small check endpoint you host (for example an Azure Function or AWS Lambda), giving you a multi-region view without a paid monitoring service.",
            ["Add one or more regional check endpoints.", "Enter the URL of the site to check.", "Click Check and compare status, response time and certificate expiry per region."],
            "https://example.com  →  West Europe: 200 OK, 182 ms, certificate expires in 64 days",
            [
                new Faq("Why do I need my own check endpoints?", "Browsers block reading certificate details and cannot make requests from other regions. A tiny serverless function in each region performs the check and returns the result."),
                new Faq("Is my data sent anywhere?", "Only the URL you check is sent, and only to the endpoints you configured. Your endpoint list is stored in your browser's local storage."),
                new Faq("Why does SSL certificate expiry matter?", "An expired certificate causes browser security warnings and breaks API clients, so it is worth checking well before the expiry date."),
                new Faq("Is it free?", "Yes, the checker is free with no sign-up. You only pay for whatever hosting your own check endpoints use, which is usually within serverless free tiers."),
            ]),
    };

    public static ToolSeo? For(string href) => Content.GetValueOrDefault(href);
}
