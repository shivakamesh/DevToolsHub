namespace DevToolsHub.Services;

public enum VimMode { Normal, Insert }

public record VimLevel(string Title, string Instruction, string Start, string Target, string Par, string Hint);

/// <summary>A tiny subset of Vim normal/insert mode used by the Vim trainer.</summary>
public sealed class VimEngine
{
    public const string Escape = "Escape";

    public List<string> Lines { get; private set; }
    public int Row { get; private set; }
    public int Col { get; private set; }
    public VimMode Mode { get; private set; }
    public int Keystrokes { get; private set; }
    public string Pending { get; private set; } = "";

    private string register = "";
    private bool registerLinewise;
    private readonly Stack<(List<string> Lines, int Row, int Col)> undo = new();

    public VimEngine(string text) => Lines = text.Replace("\r", "").Split('\n').ToList();

    public string Text => string.Join("\n", Lines);

    private string Line => Lines[Row];

    /// <summary>Feeds characters as keys; \u001b is Escape, \n is Enter and \b is Backspace.</summary>
    public void Feed(string keys)
    {
        foreach (var c in keys)
            Key(c switch { '\u001b' => Escape, '\n' => "Enter", '\b' => "Backspace", _ => c.ToString() });
    }

    public bool Key(string key)
    {
        if (key is "Shift" or "Control" or "Alt" or "Meta" or "CapsLock" or "Dead") return false;
        Keystrokes++;
        if (Mode == VimMode.Insert) Insert(key);
        else Normal(key);
        return true;
    }

    private void Insert(string key)
    {
        switch (key)
        {
            case Escape:
                Mode = VimMode.Normal;
                Col = Math.Max(0, Col - 1);
                ClampCol();
                return;
            case "Backspace":
                if (Col > 0) { Lines[Row] = Line.Remove(Col - 1, 1); Col--; }
                else if (Row > 0) { Col = Lines[Row - 1].Length; Lines[Row - 1] += Line; Lines.RemoveAt(Row); Row--; }
                return;
            case "Enter":
                Lines.Insert(Row + 1, Line[Col..]);
                Lines[Row] = Line[..Col];
                Row++;
                Col = 0;
                return;
            case "Tab":
                InsertText("    ");
                return;
        }
        if (key.Length == 1) InsertText(key);
    }

    private void InsertText(string s)
    {
        Lines[Row] = Line.Insert(Col, s);
        Col += s.Length;
    }

    private void Normal(string key)
    {
        if (key == Escape) { Pending = ""; return; }
        key = key switch { "ArrowLeft" => "h", "ArrowRight" => "l", "ArrowUp" => "k", "ArrowDown" => "j", _ => key };
        if (key.Length != 1) return;

        var cmd = Pending + key;
        var i = 0;
        while (i < cmd.Length && char.IsDigit(cmd[i]) && !(i == 0 && cmd[i] == '0')) i++;
        var count = i > 0 ? int.Parse(cmd[..i]) : 1;
        var op = cmd[i..];
        if (op.Length == 0) { Pending = cmd; return; }
        Pending = "";

        if (op.Length == 2 && op[0] == 'r')
        {
            if (Line.Length == 0) return;
            Snapshot();
            var n = Math.Min(count, Line.Length - Col);
            Lines[Row] = Line[..Col] + new string(op[1], n) + Line[(Col + n)..];
            Col += n - 1;
            return;
        }

        switch (op)
        {
            case "h": Col = Math.Max(0, Col - count); break;
            case "l": Col += count; ClampCol(); break;
            case "j": Row = Math.Min(Lines.Count - 1, Row + count); ClampCol(); break;
            case "k": Row = Math.Max(0, Row - count); ClampCol(); break;
            case "0": Col = 0; break;
            case "^": Col = FirstNonBlank(Row); break;
            case "$": Col = Math.Max(0, Line.Length - 1); break;
            case "w": for (var n = 0; n < count; n++) (Row, Col) = WordForward(Row, Col); ClampCol(); break;
            case "b": for (var n = 0; n < count; n++) (Row, Col) = WordBack(Row, Col); break;
            case "e": for (var n = 0; n < count; n++) Col = WordEnd(Col); break;
            case "gg": Row = i > 0 ? Math.Min(Lines.Count - 1, count - 1) : 0; Col = FirstNonBlank(Row); break;
            case "G": Row = i > 0 ? Math.Min(Lines.Count - 1, count - 1) : Lines.Count - 1; Col = FirstNonBlank(Row); break;
            case "i": StartInsert(); break;
            case "a": StartInsert(); Col = Math.Min(Line.Length, Col + 1); break;
            case "A": StartInsert(); Col = Line.Length; break;
            case "I": StartInsert(); Col = FirstNonBlank(Row); break;
            case "o": StartInsert(); Lines.Insert(Row + 1, ""); Row++; Col = 0; break;
            case "O": StartInsert(); Lines.Insert(Row, ""); Col = 0; break;
            case "x":
                if (Line.Length == 0) break;
                Snapshot();
                var xn = Math.Min(count, Line.Length - Col);
                SetRegister(Line.Substring(Col, xn), false);
                Lines[Row] = Line.Remove(Col, xn);
                ClampCol();
                break;
            case "dd":
            {
                Snapshot();
                var n = Math.Min(count, Lines.Count - Row);
                SetRegister(string.Join("\n", Lines.GetRange(Row, n)), true);
                Lines.RemoveRange(Row, n);
                if (Lines.Count == 0) Lines.Add("");
                Row = Math.Min(Row, Lines.Count - 1);
                Col = FirstNonBlank(Row);
                break;
            }
            case "yy":
                SetRegister(string.Join("\n", Lines.GetRange(Row, Math.Min(count, Lines.Count - Row))), true);
                break;
            case "dw":
            case "cw":
            {
                Snapshot();
                var end = op == "cw" ? ChangeWordEnd(count) : DeleteWordEnd(count);
                SetRegister(Line[Col..end], false);
                Lines[Row] = Line.Remove(Col, end - Col);
                if (op == "cw") Mode = VimMode.Insert;
                else ClampCol();
                break;
            }
            case "d$":
            case "D":
            case "C":
                Snapshot();
                SetRegister(Line[Col..], false);
                Lines[Row] = Line[..Col];
                if (op == "C") Mode = VimMode.Insert;
                else ClampCol();
                break;
            case "cc":
                Snapshot();
                SetRegister(Line, true);
                Lines[Row] = "";
                Col = 0;
                Mode = VimMode.Insert;
                break;
            case "p":
            case "P":
                if (register.Length == 0 && !registerLinewise) break;
                Snapshot();
                if (registerLinewise)
                {
                    var at = op == "p" ? Row + 1 : Row;
                    var lines = register.Split('\n');
                    for (var n = 0; n < count; n++) Lines.InsertRange(at, lines);
                    Row = at;
                    Col = FirstNonBlank(Row);
                }
                else
                {
                    var text = string.Concat(Enumerable.Repeat(register, count));
                    var at = op == "p" && Line.Length > 0 ? Col + 1 : Col;
                    Lines[Row] = Line.Insert(at, text);
                    Col = at + text.Length - 1;
                }
                break;
            case "J":
                for (var n = 0; n < Math.Max(1, count - 1) && Row + 1 < Lines.Count; n++)
                {
                    if (n == 0) Snapshot();
                    var next = Lines[Row + 1].TrimStart();
                    Col = Line.TrimEnd().Length;
                    Lines[Row] = Line.TrimEnd() + (next.Length > 0 ? " " + next : "");
                    Lines.RemoveAt(Row + 1);
                }
                break;
            case "u":
                for (var n = 0; n < count && undo.Count > 0; n++)
                {
                    var s = undo.Pop();
                    Lines = s.Lines;
                    Row = s.Row;
                    Col = s.Col;
                }
                break;
            default:
                if (op.Length == 2 && "fFtT".Contains(op[0]))
                {
                    if (FindChar(op[0], op[1], count) is { } target) Col = target;
                }
                else if (op.Length == 3 && "dcy".Contains(op[0]) && "ft".Contains(op[1]))
                {
                    if (FindChar(op[1], op[2], count) is { } target) ApplyRange(op[0], Col, target + 1);
                }
                else if (op.Length == 3 && "dcy".Contains(op[0]) && "ia".Contains(op[1]))
                {
                    if (TextObject(op[1] == 'i', op[2]) is { } range) ApplyRange(op[0], range.Start, range.End);
                }
                else if (IsPrefix(op)) Pending = cmd;
                break;
        }
    }

    private static bool IsPrefix(string op) =>
        (op.Length == 1 && "gdcyrfFtT".Contains(op[0])) ||
        (op.Length == 2 && "dcy".Contains(op[0]) && "iaft".Contains(op[1]));

    private int? FindChar(char kind, char ch, int count)
    {
        var line = Line;
        var c = Col;
        for (var n = 0; n < count; n++)
        {
            var start = kind is 't' && n == 0 ? c + 2 : kind is 'T' && n == 0 ? c - 2 : kind is 'f' or 't' ? c + 1 : c - 1;
            var found = kind is 'f' or 't'
                ? (start < line.Length ? line.IndexOf(ch, Math.Max(0, start)) : -1)
                : (start >= 0 ? line.LastIndexOf(ch, Math.Min(start, line.Length - 1)) : -1);
            if (found < 0) return null;
            c = found;
        }
        return kind switch { 't' => c - 1, 'T' => c + 1, _ => c };
    }

    private (int Start, int End)? TextObject(bool inner, char obj)
    {
        var line = Line;
        if (line.Length == 0) return null;
        switch (obj)
        {
            case 'w':
            {
                var k = Cls(line[Col]);
                int s = Col, e = Col;
                while (s > 0 && Cls(line[s - 1]) == k) s--;
                while (e < line.Length && Cls(line[e]) == k) e++;
                if (!inner)
                {
                    var e2 = e;
                    while (e2 < line.Length && Cls(line[e2]) == 0) e2++;
                    if (e2 > e) e = e2;
                    else while (s > 0 && Cls(line[s - 1]) == 0) s--;
                }
                return (s, e);
            }
            case '"' or '\'' or '`':
            {
                int open = line.LastIndexOf(obj, Col), close;
                if (open >= 0 && line.Take(open).Count(ch => ch == obj) % 2 == 1) open = line.LastIndexOf(obj, Math.Max(0, open - 1));
                if (open >= 0 && (close = line.IndexOf(obj, Math.Max(open + 1, line[Col] == obj && open < Col ? Col : open + 1))) > open)
                    return inner ? (open + 1, close) : (open, close + 1);
                open = line.IndexOf(obj, Col);
                if (open < 0 || (close = line.IndexOf(obj, open + 1)) < 0) return null;
                return inner ? (open + 1, close) : (open, close + 1);
            }
        }
        var (o, cl) = obj switch
        {
            '(' or ')' or 'b' => ('(', ')'),
            '{' or '}' or 'B' => ('{', '}'),
            '[' or ']' => ('[', ']'),
            '<' or '>' => ('<', '>'),
            _ => ('\0', '\0'),
        };
        if (o == '\0') return null;
        int depth = 0, start = -1;
        for (var i = Math.Min(Col, line.Length - 1); i >= 0; i--)
        {
            if (line[i] == cl && i != Col) depth++;
            else if (line[i] == o) { if (depth == 0) { start = i; break; } depth--; }
        }
        if (start < 0) return null;
        depth = 0;
        for (var i = start + 1; i < line.Length; i++)
        {
            if (line[i] == o) depth++;
            else if (line[i] == cl) { if (depth == 0) return inner ? (start + 1, i) : (start, i + 1); depth--; }
        }
        return null;
    }

    private void ApplyRange(char op, int start, int end)
    {
        if (end < start) (start, end) = (end, start);
        end = Math.Min(end, Line.Length);
        SetRegister(Line[start..end], false);
        if (op == 'y') { Col = start; return; }
        Snapshot();
        Lines[Row] = Line.Remove(start, end - start);
        Col = start;
        if (op == 'c') Mode = VimMode.Insert;
        else ClampCol();
    }

    private void StartInsert()
    {
        Snapshot();
        Mode = VimMode.Insert;
    }

    private void Snapshot() => undo.Push((Lines.ToList(), Row, Col));

    private void SetRegister(string text, bool linewise)
    {
        register = text;
        registerLinewise = linewise;
    }

    private void ClampCol() => Col = Math.Clamp(Col, 0, Math.Max(0, Line.Length - 1));

    private int FirstNonBlank(int row)
    {
        var l = Lines[row];
        var c = 0;
        while (c < l.Length && char.IsWhiteSpace(l[c])) c++;
        return Math.Min(c, Math.Max(0, l.Length - 1));
    }

    private static int Cls(char c) => char.IsWhiteSpace(c) ? 0 : char.IsLetterOrDigit(c) || c == '_' ? 1 : 2;

    private (int Row, int Col) WordForward(int r, int c)
    {
        var line = Lines[r];
        if (c < line.Length)
        {
            var k = Cls(line[c]);
            if (k != 0) while (c < line.Length && Cls(line[c]) == k) c++;
            while (c < line.Length && Cls(line[c]) == 0) c++;
            if (c < line.Length) return (r, c);
        }
        if (r + 1 < Lines.Count) return (r + 1, FirstNonBlank(r + 1));
        return (r, Math.Max(0, line.Length - 1));
    }

    private (int Row, int Col) WordBack(int r, int c)
    {
        c--;
        while (true)
        {
            if (c < 0)
            {
                if (r == 0) return (0, 0);
                r--;
                c = Lines[r].Length - 1;
                if (c < 0) return (r, 0);
            }
            var line = Lines[r];
            while (c > 0 && Cls(line[c]) == 0) c--;
            if (Cls(line[c]) == 0) { c = -1; continue; }
            var k = Cls(line[c]);
            while (c > 0 && Cls(line[c - 1]) == k) c--;
            return (r, c);
        }
    }

    private int WordEnd(int c)
    {
        var line = Line;
        c++;
        while (c < line.Length && Cls(line[c]) == 0) c++;
        if (c >= line.Length) return Math.Max(0, line.Length - 1);
        var k = Cls(line[c]);
        while (c + 1 < line.Length && Cls(line[c + 1]) == k) c++;
        return c;
    }

    private int DeleteWordEnd(int count)
    {
        var (r, c) = (Row, Col);
        for (var n = 0; n < count; n++)
        {
            (r, c) = WordForward(r, c);
            if (r != Row) return Line.Length;
        }
        return c == Col ? Line.Length : c;
    }

    private int ChangeWordEnd(int count)
    {
        var line = Line;
        var c = Col;
        for (var n = 0; n < count && c < line.Length; n++)
        {
            if (n > 0) while (c < line.Length && Cls(line[c]) == 0) c++;
            if (c >= line.Length) break;
            var k = Cls(line[c]);
            while (c < line.Length && Cls(line[c]) == k) c++;
        }
        return c;
    }

    public static readonly VimLevel[] Levels =
    [
        new("Delete a word", "Remove the duplicated \"the\".", "the the quick fox", "the quick fox", "dw", "dw deletes from the cursor to the start of the next word."),
        new("Delete a line", "Remove the debug line.", "a\nconsole.log(x);\nb", "a\nb", "jdd", "j moves down; dd deletes the whole line."),
        new("Join lines", "Join the two lines into one.", "Hello\nworld", "Hello world", "J", "J joins the next line onto the current one."),
        new("Duplicate a line", "Duplicate the line.", "item();", "item();\nitem();", "yyp", "yy yanks (copies) the line; p pastes it below."),
        new("Swap lines", "Put the lines in the right order.", "second\nfirst", "first\nsecond", "ddp", "dd cuts the line; p pastes it below the next one."),
        new("Swap letters", "Fix the typo in \"cuont\".", "var cuont = 0;", "var count = 0;", "wlxp", "w jumps a word, x cuts a character and p pastes it after the cursor."),
        new("Append to lines", "Add a semicolon to the end of both lines.", "int a = 1\nint b = 2", "int a = 1;\nint b = 2;", "A;\u001bjA;\u001b", "A appends at the end of the line; Esc returns to normal mode."),
        new("Change a word", "Change red to blue.", "let color = 'red';", "let color = 'blue';", "3wlcwblue\u001b", "Use a count with w (3w), then cw to change the word."),
        new("Delete to end of line", "Remove the trailing comment and the space before it.", "total = price * qty // TODO remove", "total = price * qty", "5whD", "5w jumps five words; D deletes to the end of the line."),
        new("Open a line", "Add a return statement inside the block.", "if (ok) {\n}", "if (ok) {\n    return;\n}", "o    return;\u001b", "o opens a new line below and enters insert mode."),
        new("Insert characters", "Turn the comparison into a strict equality.", "if (a == b)", "if (a === b)", "3wi=\u001b", "3w jumps to ==; i inserts before the cursor."),
        new("Go to the end", "Delete the last line of the file.", "one\ntwo\nthree\nfour\nfive", "one\ntwo\nthree\nfour", "Gdd", "G jumps to the last line."),
        new("Jump to a character", "Flip the comparison to <.", "if (a > b) {", "if (a < b) {", "f>r<", "f> jumps to the next >; r replaces one character."),
        new("Delete until", "Delete everything before the comment.", "x = 1 // set x", "// set x", "dt/", "dt/ deletes up to (not including) the next /."),
        new("Change inner word", "Rename result to total.", "var result = compute();", "var total = compute();", "wciwtotal\u001b", "ciw changes the whole word under the cursor, wherever the cursor is in it."),
        new("Delete a word with space", "Remove the word extra.", "keep this extra word", "keep this word", "2wdaw", "daw deletes a word and the space after it."),
        new("Change a string", "Change Alice to Bob.", "name = \"Alice\"", "name = \"Bob\"", "ci\"Bob\u001b", "ci\" changes the text inside the next pair of quotes."),
        new("Empty the arguments", "Remove all the arguments.", "print(debug, value)", "print()", "f(di(", "di( deletes everything inside the parentheses."),
        new("Empty the block", "Remove the body of the block.", "fn() { old(); }", "fn() {}", "f{di{", "di{ deletes inside braces, even when other brackets are nested."),
    ];
}
