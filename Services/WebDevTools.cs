using System.Globalization;
using System.Text;

namespace DevToolsHub.Services;

public static class ContrastChecker
{
    public static bool TryParseHex(string? text, out (int R, int G, int B) rgb)
    {
        rgb = default;
        var s = (text ?? "").Trim().TrimStart('#');
        if (s.Length == 3) s = string.Concat(s.Select(c => $"{c}{c}"));
        if (s.Length != 6 || !int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v)) return false;
        rgb = ((v >> 16) & 0xFF, (v >> 8) & 0xFF, v & 0xFF);
        return true;
    }

    public static string ToHex((int R, int G, int B) c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    public static double Luminance((int R, int G, int B) c)
    {
        static double Ch(int v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Ch(c.R) + 0.7152 * Ch(c.G) + 0.0722 * Ch(c.B);
    }

    public static double Ratio((int R, int G, int B) a, (int R, int G, int B) b)
    {
        var l1 = Luminance(a);
        var l2 = Luminance(b);
        return (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
    }

    /// <summary>Darkens or lightens the foreground until it reaches the target ratio against the background.</summary>
    public static (int R, int G, int B)? Suggest((int R, int G, int B) fg, (int R, int G, int B) bg, double target)
    {
        if (Ratio(fg, bg) >= target) return fg;
        var darken = Luminance(bg) > 0.18;
        for (var step = 1; step <= 100; step++)
        {
            var f = step / 100.0;
            int Mix(int v) => (int)Math.Round(darken ? v * (1 - f) : v + (255 - v) * f);
            var c = (Mix(fg.R), Mix(fg.G), Mix(fg.B));
            if (Ratio(c, bg) >= target) return c;
        }
        return null;
    }
}

public enum DbKind { SqlServer, PostgreSql, MySql, Sqlite, MongoDb, Redis }

public record ConnectionOptions(
    DbKind Kind, string Host, int? Port, string Database, string User, string Password,
    bool IntegratedSecurity = false, bool Ssl = false, bool TrustServerCertificate = false, int? Timeout = null);

public static class ConnectionStrings
{
    public static int DefaultPort(DbKind kind) => kind switch
    {
        DbKind.SqlServer => 1433,
        DbKind.PostgreSql => 5432,
        DbKind.MySql => 3306,
        DbKind.MongoDb => 27017,
        DbKind.Redis => 6379,
        _ => 0,
    };

    public static string Build(ConnectionOptions o)
    {
        var port = o.Port ?? DefaultPort(o.Kind);
        var parts = new List<string>();
        switch (o.Kind)
        {
            case DbKind.SqlServer:
                parts.Add($"Server={o.Host}{(port != 1433 ? $",{port}" : "")}");
                parts.Add($"Database={o.Database}");
                if (o.IntegratedSecurity) parts.Add("Integrated Security=True");
                else { parts.Add($"User Id={o.User}"); parts.Add($"Password={o.Password}"); }
                if (o.Ssl) parts.Add("Encrypt=True");
                if (o.TrustServerCertificate) parts.Add("TrustServerCertificate=True");
                if (o.Timeout is { } t) parts.Add($"Connect Timeout={t}");
                return string.Join(";", parts) + ";";
            case DbKind.PostgreSql:
                parts.Add($"Host={o.Host}");
                parts.Add($"Port={port}");
                parts.Add($"Database={o.Database}");
                if (o.IntegratedSecurity) parts.Add("Integrated Security=true");
                else { parts.Add($"Username={o.User}"); parts.Add($"Password={o.Password}"); }
                if (o.Ssl) parts.Add("SSL Mode=Require");
                if (o.TrustServerCertificate) parts.Add("Trust Server Certificate=true");
                if (o.Timeout is { } pt) parts.Add($"Timeout={pt}");
                return string.Join(";", parts) + ";";
            case DbKind.MySql:
                parts.Add($"Server={o.Host}");
                parts.Add($"Port={port}");
                parts.Add($"Database={o.Database}");
                parts.Add($"Uid={o.User}");
                parts.Add($"Pwd={o.Password}");
                parts.Add($"SslMode={(o.Ssl ? "Required" : "Preferred")}");
                if (o.Timeout is { } mt) parts.Add($"Connection Timeout={mt}");
                return string.Join(";", parts) + ";";
            case DbKind.Sqlite:
                return $"Data Source={(string.IsNullOrWhiteSpace(o.Database) ? "app.db" : o.Database)}";
            case DbKind.MongoDb:
            {
                var auth = string.IsNullOrEmpty(o.User) ? "" : $"{Uri.EscapeDataString(o.User)}:{Uri.EscapeDataString(o.Password)}@";
                var query = new List<string>();
                if (o.Ssl) query.Add("tls=true");
                if (o.TrustServerCertificate) query.Add("tlsAllowInvalidCertificates=true");
                if (o.Timeout is { } st) query.Add($"connectTimeoutMS={st * 1000}");
                return $"mongodb://{auth}{o.Host}:{port}/{o.Database}" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            }
            case DbKind.Redis:
                parts.Add($"{o.Host}:{port}");
                if (!string.IsNullOrEmpty(o.Password)) parts.Add($"password={o.Password}");
                if (!string.IsNullOrEmpty(o.User)) parts.Add($"user={o.User}");
                if (o.Ssl) parts.Add("ssl=True");
                if (int.TryParse(o.Database, out var db)) parts.Add($"defaultDatabase={db}");
                if (o.Timeout is { } rt) parts.Add($"connectTimeout={rt * 1000}");
                return string.Join(",", parts);
            default:
                return "";
        }
    }

    public static string AppSettings(string name, string connectionString) =>
        $"{{\n  \"ConnectionStrings\": {{\n    \"{name}\": \"{connectionString.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"\n  }}\n}}";

    public static string EnvVar(string name, string connectionString) =>
        $"ConnectionStrings__{name}=\"{connectionString.Replace("\"", "\\\"")}\"";
}

public enum AppStack { DotNet, Node, Python }

public record DockerOptions(AppStack Stack, string Version, string ProjectName, int Port, string BuildCommand = "npm run build", string StartCommand = "");

public record WorkflowOptions(AppStack Stack, string Version, string Branch, bool RunTests, bool DockerPush, string ImageName);

public static class DevOpsGenerator
{
    public static string Dockerfile(DockerOptions o)
    {
        var sb = new StringBuilder();
        switch (o.Stack)
        {
            case AppStack.DotNet:
                var proj = string.IsNullOrWhiteSpace(o.ProjectName) ? "MyApp" : o.ProjectName.Trim();
                sb.AppendLine($"FROM mcr.microsoft.com/dotnet/sdk:{o.Version} AS build");
                sb.AppendLine("WORKDIR /src");
                sb.AppendLine($"COPY [\"{proj}.csproj\", \"./\"]");
                sb.AppendLine($"RUN dotnet restore \"{proj}.csproj\"");
                sb.AppendLine("COPY . .");
                sb.AppendLine($"RUN dotnet publish \"{proj}.csproj\" -c Release -o /app/publish /p:UseAppHost=false");
                sb.AppendLine();
                sb.AppendLine($"FROM mcr.microsoft.com/dotnet/aspnet:{o.Version} AS final");
                sb.AppendLine("WORKDIR /app");
                sb.AppendLine($"ENV ASPNETCORE_HTTP_PORTS={o.Port}");
                sb.AppendLine($"EXPOSE {o.Port}");
                sb.AppendLine("COPY --from=build /app/publish .");
                sb.AppendLine("USER $APP_UID");
                sb.AppendLine($"ENTRYPOINT [\"dotnet\", \"{proj}.dll\"]");
                break;
            case AppStack.Node:
                sb.AppendLine($"FROM node:{o.Version}-alpine AS build");
                sb.AppendLine("WORKDIR /app");
                sb.AppendLine("COPY package*.json ./");
                sb.AppendLine("RUN npm ci");
                sb.AppendLine("COPY . .");
                if (!string.IsNullOrWhiteSpace(o.BuildCommand)) sb.AppendLine($"RUN {o.BuildCommand.Trim()}");
                sb.AppendLine();
                sb.AppendLine($"FROM node:{o.Version}-alpine AS final");
                sb.AppendLine("WORKDIR /app");
                sb.AppendLine("ENV NODE_ENV=production");
                sb.AppendLine("COPY package*.json ./");
                sb.AppendLine("RUN npm ci --omit=dev");
                sb.AppendLine("COPY --from=build /app .");
                sb.AppendLine($"EXPOSE {o.Port}");
                sb.AppendLine("USER node");
                sb.AppendLine($"CMD {ExecForm(string.IsNullOrWhiteSpace(o.StartCommand) ? "node index.js" : o.StartCommand)}");
                break;
            case AppStack.Python:
                sb.AppendLine($"FROM python:{o.Version}-slim");
                sb.AppendLine("ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1");
                sb.AppendLine("WORKDIR /app");
                sb.AppendLine("COPY requirements.txt .");
                sb.AppendLine("RUN pip install --no-cache-dir -r requirements.txt");
                sb.AppendLine("COPY . .");
                sb.AppendLine("RUN useradd --create-home appuser");
                sb.AppendLine("USER appuser");
                sb.AppendLine($"EXPOSE {o.Port}");
                sb.AppendLine($"CMD {ExecForm(string.IsNullOrWhiteSpace(o.StartCommand) ? $"gunicorn -b 0.0.0.0:{o.Port} app:app" : o.StartCommand)}");
                break;
        }
        return sb.ToString();
    }

    public static string DockerIgnore(AppStack stack) => stack switch
    {
        AppStack.DotNet => "**/bin\n**/obj\n**/.vs\n**/.vscode\n**/*.user\n.git\nDockerfile\n.dockerignore\n",
        AppStack.Node => "node_modules\nnpm-debug.log\ndist\n.git\n.env\nDockerfile\n.dockerignore\n",
        _ => "__pycache__\n*.pyc\n.venv\nvenv\n.git\n.env\nDockerfile\n.dockerignore\n",
    };

    private static string ExecForm(string command) =>
        "[" + string.Join(", ", command.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => $"\"{p.Replace("\"", "\\\"")}\"")) + "]";

    public static string GitHubWorkflow(WorkflowOptions o)
    {
        var branch = string.IsNullOrWhiteSpace(o.Branch) ? "main" : o.Branch.Trim();
        var sb = new StringBuilder();
        sb.AppendLine("name: CI");
        sb.AppendLine();
        sb.AppendLine("on:");
        sb.AppendLine("  push:");
        sb.AppendLine($"    branches: [ \"{branch}\" ]");
        sb.AppendLine("  pull_request:");
        sb.AppendLine($"    branches: [ \"{branch}\" ]");
        sb.AppendLine();
        if (o.DockerPush)
        {
            sb.AppendLine("permissions:");
            sb.AppendLine("  contents: read");
            sb.AppendLine("  packages: write");
            sb.AppendLine();
        }
        sb.AppendLine("jobs:");
        sb.AppendLine("  build:");
        sb.AppendLine("    runs-on: ubuntu-latest");
        sb.AppendLine("    steps:");
        sb.AppendLine("      - uses: actions/checkout@v4");
        switch (o.Stack)
        {
            case AppStack.DotNet:
                sb.AppendLine("      - uses: actions/setup-dotnet@v4");
                sb.AppendLine("        with:");
                sb.AppendLine($"          dotnet-version: '{o.Version}.x'");
                sb.AppendLine("      - run: dotnet restore");
                sb.AppendLine("      - run: dotnet build --no-restore -c Release");
                if (o.RunTests) sb.AppendLine("      - run: dotnet test --no-build -c Release --verbosity normal");
                break;
            case AppStack.Node:
                sb.AppendLine("      - uses: actions/setup-node@v4");
                sb.AppendLine("        with:");
                sb.AppendLine($"          node-version: '{o.Version}'");
                sb.AppendLine("          cache: 'npm'");
                sb.AppendLine("      - run: npm ci");
                sb.AppendLine("      - run: npm run build --if-present");
                if (o.RunTests) sb.AppendLine("      - run: npm test");
                break;
            case AppStack.Python:
                sb.AppendLine("      - uses: actions/setup-python@v5");
                sb.AppendLine("        with:");
                sb.AppendLine($"          python-version: '{o.Version}'");
                sb.AppendLine("          cache: 'pip'");
                sb.AppendLine("      - run: pip install -r requirements.txt");
                if (o.RunTests) sb.AppendLine("      - run: pip install pytest && pytest");
                break;
        }
        if (o.DockerPush)
        {
            var image = string.IsNullOrWhiteSpace(o.ImageName) ? "${{ github.repository }}" : o.ImageName.Trim().ToLowerInvariant();
            sb.AppendLine();
            sb.AppendLine("  docker:");
            sb.AppendLine("    needs: build");
            sb.AppendLine($"    if: github.event_name == 'push' && github.ref == 'refs/heads/{branch}'");
            sb.AppendLine("    runs-on: ubuntu-latest");
            sb.AppendLine("    steps:");
            sb.AppendLine("      - uses: actions/checkout@v4");
            sb.AppendLine("      - uses: docker/login-action@v3");
            sb.AppendLine("        with:");
            sb.AppendLine("          registry: ghcr.io");
            sb.AppendLine("          username: ${{ github.actor }}");
            sb.AppendLine("          password: ${{ secrets.GITHUB_TOKEN }}");
            sb.AppendLine("      - uses: docker/build-push-action@v6");
            sb.AppendLine("        with:");
            sb.AppendLine("          context: .");
            sb.AppendLine("          push: true");
            sb.AppendLine($"          tags: ghcr.io/{image}:latest,ghcr.io/{image}:${{{{ github.sha }}}}");
        }
        return sb.ToString();
    }
}
