using System.Text;

namespace DevToolsHub.Services;

/// <summary>Lightweight, dialect-agnostic SQL beautifier.</summary>
public static class SqlFormatter
{
    private enum Kind { None, Word, String, LineComment, BlockComment, Punct }

    private readonly record struct Token(Kind Kind, string Text)
    {
        public string Upper => Text?.ToUpperInvariant() ?? "";
    }

    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "IN", "IS", "NULL", "LIKE", "BETWEEN", "EXISTS",
        "GROUP", "BY", "ORDER", "HAVING", "LIMIT", "OFFSET", "TOP", "DISTINCT", "AS", "ON", "USING",
        "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS", "UNION", "ALL", "EXCEPT", "INTERSECT",
        "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "CREATE", "TABLE", "ALTER", "DROP", "INDEX",
        "CASE", "WHEN", "THEN", "ELSE", "END", "ASC", "DESC", "WITH", "OVER", "PARTITION", "COUNT", "SUM",
        "AVG", "MIN", "MAX", "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "DEFAULT", "CONSTRAINT", "FETCH", "NEXT", "ROWS", "ONLY",
    };

    private static readonly HashSet<string> Clauses = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "FROM", "WHERE", "GROUP", "ORDER", "HAVING", "LIMIT", "OFFSET", "FETCH", "UNION", "EXCEPT", "INTERSECT",
        "INSERT", "VALUES", "UPDATE", "SET", "DELETE", "WITH",
    };

    private static readonly HashSet<string> Functions = new(StringComparer.OrdinalIgnoreCase)
    {
        "COUNT", "SUM", "AVG", "MIN", "MAX", "OVER",
    };

    private static readonly HashSet<string> JoinPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "LEFT", "RIGHT", "INNER", "FULL", "CROSS", "OUTER",
    };

    public static string Format(string sql, bool uppercase = true, int indentSize = 4)
    {
        var tokens = Tokenize(sql);
        var w = new Writer(indentSize);
        var parens = new Stack<(bool Subquery, int Indent)>();
        var betweenPending = false;

        for (var i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            var prev = i > 0 ? tokens[i - 1] : default;
            var next = i + 1 < tokens.Count ? tokens[i + 1] : default;
            var inline = parens.Count > 0 && !parens.Peek().Subquery;

            switch (t.Kind)
            {
                case Kind.LineComment:
                    w.Write(t.Text);
                    w.NewLine();
                    continue;
                case Kind.BlockComment:
                case Kind.String:
                    w.Write(t.Text);
                    continue;
                case Kind.Punct:
                    switch (t.Text)
                    {
                        case "(":
                            var sub = next.Kind == Kind.Word && next.Upper is "SELECT" or "WITH";
                            var isCall = prev.Kind == Kind.Word && (!Keywords.Contains(prev.Text) || Functions.Contains(prev.Text));
                            w.Write("(", spaceBefore: !isCall);
                            w.NoSpace();
                            parens.Push((sub, w.Indent));
                            if (sub) { w.Indent++; w.NewLine(); }
                            break;
                        case ")":
                            if (parens.Count > 0)
                            {
                                var (wasSub, indent) = parens.Pop();
                                if (wasSub) { w.Indent = indent; w.NewLine(); }
                            }
                            w.Write(")", spaceBefore: false);
                            break;
                        case ",":
                            w.Write(",", spaceBefore: false);
                            if (!inline) w.NewLine(1);
                            break;
                        case ";":
                            w.Write(";", spaceBefore: false);
                            w.Indent = 0;
                            w.NewLine();
                            w.BlankLine();
                            break;
                        case ".":
                            w.Write(".", spaceBefore: false);
                            w.NoSpace();
                            break;
                        default:
                            w.Write(t.Text);
                            break;
                    }
                    continue;
            }

            var upper = t.Upper;
            if (!inline)
            {
                var isJoinPrefix = JoinPrefixes.Contains(upper) && next.Kind == Kind.Word && next.Upper is "JOIN" or "OUTER";
                var isJoin = upper == "JOIN" && !JoinPrefixes.Contains(prev.Upper);
                var isClause = Clauses.Contains(upper)
                               && !(upper == "FROM" && prev.Upper == "DELETE")
                               && !(upper == "SELECT" && prev.Upper is "UNION" or "ALL" or "EXCEPT" or "INTERSECT");

                if (isJoinPrefix || isJoin || isClause)
                {
                    w.NewLine();
                    if (isClause) w.LastClause = upper;
                }
                else if (upper == "ON")
                {
                    w.NewLine(1);
                }
                else if (upper is "AND" or "OR" && !betweenPending)
                {
                    w.NewLine(1);
                }
            }

            if (upper == "BETWEEN") betweenPending = true;
            else if (upper == "AND" && betweenPending) betweenPending = false;

            w.Write(uppercase && Keywords.Contains(t.Text) ? upper : t.Text);
        }

        return w.ToString();
    }

    private static List<Token> Tokenize(string sql)
    {
        var tokens = new List<Token>();
        var i = 0;
        while (i < sql.Length)
        {
            var c = sql[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (c == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
            {
                var end = sql.IndexOf('\n', i);
                end = end < 0 ? sql.Length : end;
                tokens.Add(new(Kind.LineComment, sql[i..end].TrimEnd()));
                i = end;
            }
            else if (c == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
            {
                var end = sql.IndexOf("*/", i + 2, StringComparison.Ordinal);
                end = end < 0 ? sql.Length : end + 2;
                tokens.Add(new(Kind.BlockComment, sql[i..end]));
                i = end;
            }
            else if (c is '\'' or '"' or '`' or '[')
            {
                var close = c == '[' ? ']' : c;
                var j = i + 1;
                while (j < sql.Length)
                {
                    if (sql[j] == close)
                    {
                        if (close != ']' && j + 1 < sql.Length && sql[j + 1] == close) { j += 2; continue; }
                        break;
                    }
                    j++;
                }
                j = Math.Min(j + 1, sql.Length);
                tokens.Add(new(c == '\'' ? Kind.String : Kind.Word, sql[i..j]));
                i = j;
            }
            else if (char.IsLetterOrDigit(c) || c is '_' or '@' or '#' or '$')
            {
                var j = i;
                while (j < sql.Length && (char.IsLetterOrDigit(sql[j]) || sql[j] is '_' or '@' or '#' or '$')) j++;
                if (j < sql.Length && sql[j] == '.' && char.IsDigit(c) && j + 1 < sql.Length && char.IsDigit(sql[j + 1]))
                {
                    j++;
                    while (j < sql.Length && char.IsDigit(sql[j])) j++;
                }
                tokens.Add(new(Kind.Word, sql[i..j]));
                i = j;
            }
            else
            {
                var two = i + 1 < sql.Length ? sql.Substring(i, 2) : "";
                if (two is "<=" or ">=" or "<>" or "!=" or "||" or "::")
                {
                    tokens.Add(new(Kind.Punct, two));
                    i += 2;
                }
                else
                {
                    tokens.Add(new(Kind.Punct, c.ToString()));
                    i++;
                }
            }
        }
        return tokens;
    }

    private sealed class Writer(int indentSize)
    {
        private readonly StringBuilder sb = new();
        private int lineStart;
        private bool suppressSpace = true;

        public int Indent { get; set; }
        public string LastClause { get; set; } = "";

        public void NoSpace() => suppressSpace = true;

        public void Write(string text, bool spaceBefore = true)
        {
            var atLineStart = sb.Length == lineStart + CurrentPad;
            if (spaceBefore && !suppressSpace && !atLineStart) sb.Append(' ');
            sb.Append(text);
            suppressSpace = false;
        }

        private int CurrentPad => currentPad;
        private int currentPad;

        public void NewLine(int extra = 0)
        {
            while (sb.Length > 0 && sb[^1] == ' ') sb.Length--;
            if (sb.Length == 0)
            {
                lineStart = 0;
                currentPad = 0;
                return;
            }
            if (sb.Length == lineStart)
            {
                // nothing written on this line yet: just re-indent
            }
            else
            {
                sb.Append('\n');
                lineStart = sb.Length;
            }
            currentPad = (Indent + extra) * indentSize;
            sb.Append(' ', currentPad);
            suppressSpace = true;
        }

        public void BlankLine()
        {
            if (sb.Length == lineStart + currentPad)
            {
                sb.Length = lineStart;
                sb.Append('\n');
                lineStart = sb.Length;
                currentPad = 0;
            }
        }

        public override string ToString() => sb.ToString().TrimEnd() + "\n";
    }
}
