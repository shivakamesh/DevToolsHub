using System.Globalization;
using System.Text.RegularExpressions;

namespace DevToolsHub.Services;

public record QuizQuestion(string Prompt, string[] Options, int Answer, string? Explanation = null, string? Code = null);

public record RegexPuzzle(string Title, string[] Match, string[] Avoid, string Par);

public record BitOp(string Label, Func<int, int> Apply);

public record ShortcutItem(string Action, string Shortcut);

public record SelectorLevel(string Instruction, string Html, string Solution, string Hint);

public enum LetterState { Absent, Present, Correct }

public static class DevGames
{
    // ---------- Typing test ----------
    public static readonly Dictionary<string, string[]> TypingSnippets = new()
    {
        ["C#"] =
        [
            "public async Task<User?> GetUserAsync(int id)\n{\nvar user = await _db.Users.FindAsync(id);\nreturn user is null ? null : user;\n}",
            "var adults = people\n.Where(p => p.Age >= 18)\n.OrderBy(p => p.LastName)\n.Select(p => new { p.FirstName, p.LastName })\n.ToList();",
            "public record Order(int Id, decimal Total);\nforeach (var order in orders)\n{\nif (order.Total > 100m) Console.WriteLine($\"Big order {order.Id}\");\n}",
            "builder.Services.AddScoped<IUserService, UserService>();\nvar app = builder.Build();\napp.MapGet(\"/health\", () => Results.Ok(\"healthy\"));\napp.Run();",
        ],
        ["JavaScript"] =
        [
            "const res = await fetch('/api/users');\nif (!res.ok) throw new Error(res.statusText);\nconst users = await res.json();\nconsole.log(users.length);",
            "const total = items\n.filter(item => item.inStock)\n.map(item => item.price * item.qty)\n.reduce((sum, value) => sum + value, 0);",
            "function debounce(fn, ms) {\nlet timer;\nreturn (...args) => {\nclearTimeout(timer);\ntimer = setTimeout(() => fn(...args), ms);\n};\n}",
            "document.querySelectorAll('.btn').forEach(btn => {\nbtn.addEventListener('click', e => {\ne.preventDefault();\nbtn.classList.toggle('active');\n});\n});",
        ],
        ["SQL"] =
        [
            "SELECT c.name, COUNT(o.id) AS orders\nFROM customers c\nLEFT JOIN orders o ON o.customer_id = c.id\nGROUP BY c.name\nHAVING COUNT(o.id) > 5\nORDER BY orders DESC;",
            "UPDATE products\nSET price = price * 1.10\nWHERE category_id = 3\nAND discontinued = 0;",
            "CREATE TABLE users (\nid INT PRIMARY KEY,\nemail VARCHAR(255) NOT NULL UNIQUE,\ncreated_at DATETIME DEFAULT CURRENT_TIMESTAMP\n);",
            "WITH ranked AS (\nSELECT *, ROW_NUMBER() OVER (PARTITION BY dept ORDER BY salary DESC) AS rn\nFROM employees\n)\nSELECT * FROM ranked WHERE rn = 1;",
        ],
    };

    public static int CalculateWpm(int correctChars, TimeSpan elapsed) =>
        elapsed.TotalSeconds < 1 ? 0 : (int)Math.Round(correctChars / 5.0 / elapsed.TotalMinutes);

    // ---------- Regex golf ----------
    public static readonly RegexPuzzle[] RegexPuzzles =
    [
        new("Starts with test", ["testCase", "testRunner", "tested", "testify"], ["contest", "attest", "retest", "latest"], "^test"),
        new("C# files", ["Program.cs", "Startup.cs", "a.cs", "UserService.cs"], ["Program.css", "cs", "readme.md", "app.csproj"], @"\.cs$"),
        new("Double letters", ["coffee", "bookkeeper", "balloon", "address"], ["cafe", "python", "regex", "golang"], @"(.)\1"),
        new("ISO dates", ["2024-01-31", "1999-12-01", "2030-06-15"], ["2024/01/31", "24-01-31", "2024-1-31", "2024-01-311"], @"^\d{4}-\d\d-\d\d$"),
        new("camelCase", ["getValue", "toString", "isReady", "parseInt"], ["GetValue", "to_string", "value", "PARSE"], "^[a-z]+[A-Z]"),
        new("Semantic versions", ["1.0.0", "2.10.3", "0.0.1", "10.20.30"], ["1.0", "1.0.0.0", "v1.0.0", "1..0"], @"^\d+\.\d+\.\d+$"),
        new("Hex colors", ["#fff", "#1a2b3c", "#abc", "#00ff00"], ["#ggg", "fff", "#12345", "#abcd"], "^#([0-9a-f]{3}){1,2}$"),
        new("HTTP errors", ["404", "500", "418", "503"], ["200", "301", "204", "1404"], "^[45]..$"),
    ];

    /// <summary>Returns null when the pattern is invalid, otherwise per-word results.</summary>
    public static (bool[] Match, bool[] Avoid)? EvaluateRegex(RegexPuzzle puzzle, string pattern)
    {
        Regex regex;
        try { regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(250)); }
        catch (ArgumentException) { return null; }
        try
        {
            return (puzzle.Match.Select(w => regex.IsMatch(w)).ToArray(), puzzle.Avoid.Select(w => !regex.IsMatch(w)).ToArray());
        }
        catch (RegexMatchTimeoutException) { return null; }
    }

    // ---------- HTTP status quiz ----------
    public static readonly (int Code, string Meaning)[] HttpStatuses =
    [
        (100, "Continue"), (101, "Switching Protocols"), (200, "OK"), (201, "Created"), (202, "Accepted"),
        (204, "No Content"), (206, "Partial Content"), (301, "Moved Permanently"), (302, "Found"),
        (304, "Not Modified"), (307, "Temporary Redirect"), (308, "Permanent Redirect"), (400, "Bad Request"),
        (401, "Unauthorized"), (403, "Forbidden"), (404, "Not Found"), (405, "Method Not Allowed"),
        (406, "Not Acceptable"), (408, "Request Timeout"), (409, "Conflict"), (410, "Gone"),
        (413, "Content Too Large"), (415, "Unsupported Media Type"), (418, "I'm a teapot"),
        (422, "Unprocessable Content"), (429, "Too Many Requests"), (451, "Unavailable For Legal Reasons"),
        (500, "Internal Server Error"), (501, "Not Implemented"), (502, "Bad Gateway"),
        (503, "Service Unavailable"), (504, "Gateway Timeout"),
    ];

    public static List<QuizQuestion> BuildHttpQuiz(int count, bool reverse, Random rng)
    {
        var picks = HttpStatuses.OrderBy(_ => rng.Next()).Take(count).ToList();
        return picks.Select(p =>
        {
            var options = HttpStatuses.Where(s => s.Code != p.Code)
                .OrderBy(s => Math.Abs(s.Code / 100 - p.Code / 100)).ThenBy(_ => rng.Next())
                .Take(3).Append(p).OrderBy(_ => rng.Next()).ToArray();
            return reverse
                ? new QuizQuestion($"Which status code means \u201C{p.Meaning}\u201D?", options.Select(o => o.Code.ToString()).ToArray(), Array.IndexOf(options, p), $"{p.Code} {p.Meaning}")
                : new QuizQuestion($"What does HTTP {p.Code} mean?", options.Select(o => o.Meaning).ToArray(), Array.IndexOf(options, p), $"{p.Code} {p.Meaning}");
        }).ToList();
    }

    // ---------- Git quiz ----------
    public static readonly QuizQuestion[] GitQuestions =
    [
        new("You want to undo your last commit but keep the changes staged.", ["git reset --soft HEAD~1", "git reset --hard HEAD~1", "git revert HEAD", "git checkout HEAD~1"], 0, "--soft moves HEAD back and leaves the changes in the index."),
        new("You need to temporarily shelve uncommitted changes to switch branches.", ["git stash", "git commit --amend", "git clean -fd", "git reset"], 0, "git stash saves the working tree and index; git stash pop restores them."),
        new("You want to fix the message of your most recent (unpushed) commit.", ["git commit --amend", "git rebase --continue", "git revert HEAD", "git tag -f"], 0, "--amend replaces the last commit, letting you edit its message."),
        new("You want to apply a single commit from another branch onto the current branch.", ["git cherry-pick <sha>", "git merge <sha>", "git rebase <sha>", "git apply <sha>"], 0, "cherry-pick copies one commit onto HEAD."),
        new("A pushed commit introduced a bug. Undo it without rewriting shared history.", ["git revert <sha>", "git reset --hard <sha>~1", "git commit --amend", "git push --force"], 0, "revert creates a new commit that inverses the changes."),
        new("You want to see who last changed each line of a file.", ["git blame <file>", "git log --oneline", "git show <file>", "git diff <file>"], 0, "blame annotates each line with the commit and author."),
        new("You want to find which commit introduced a bug using binary search.", ["git bisect", "git grep", "git reflog", "git describe"], 0, "bisect checks out commits between a good and bad point to narrow it down."),
        new("You accidentally ran git reset --hard and need to find the lost commit.", ["git reflog", "git log --all", "git fsck --lost", "git stash list"], 0, "reflog records every position HEAD has pointed to."),
        new("Create a new branch called feature and switch to it in one command.", ["git switch -c feature", "git branch feature", "git checkout feature", "git merge feature"], 0, "git switch -c (or git checkout -b) creates and switches."),
        new("Discard local changes to a single file in the working tree.", ["git restore <file>", "git rm <file>", "git reset --soft <file>", "git clean <file>"], 0, "git restore (or git checkout -- <file>) restores it from the index."),
        new("Unstage a file you added with git add, keeping your edits.", ["git restore --staged <file>", "git restore <file>", "git rm <file>", "git stash <file>"], 0, "--staged resets the index entry only."),
        new("Download remote changes without merging them into your branch.", ["git fetch", "git pull", "git clone", "git push"], 0, "fetch updates remote-tracking branches only; pull = fetch + merge/rebase."),
        new("Squash the last 3 commits into one before opening a pull request.", ["git rebase -i HEAD~3", "git merge --squash HEAD~3", "git reset HEAD~3 --hard", "git cherry-pick HEAD~3"], 0, "Interactive rebase lets you mark commits as squash or fixup."),
        new("Remove untracked files and directories from the working tree.", ["git clean -fd", "git rm -r .", "git reset --hard", "git stash drop"], 0, "clean -f removes untracked files; -d includes directories. Use -n for a dry run."),
        new("Show a compact graph of all branches in the terminal.", ["git log --oneline --graph --all", "git branch -a", "git show-branch", "git status -sb"], 0, "--graph draws the branch structure; --all includes every ref."),
        new("Point your local repo at a new remote URL for origin.", ["git remote set-url origin <url>", "git remote add origin <url>", "git clone <url>", "git push -u origin <url>"], 0, "set-url changes an existing remote; add would fail because origin exists."),
        new("Delete a branch on the remote named old-feature.", ["git push origin --delete old-feature", "git branch -d old-feature", "git remote rm old-feature", "git reset origin/old-feature"], 0, "branch -d only deletes the local branch."),
        new("See what changed in files you have already staged.", ["git diff --staged", "git diff", "git status", "git show"], 0, "Plain git diff shows unstaged changes only."),
    ];

    public static List<QuizQuestion> Shuffle(IEnumerable<QuizQuestion> questions, Random rng) =>
        questions.OrderBy(_ => rng.Next()).Select(q =>
        {
            var order = Enumerable.Range(0, q.Options.Length).OrderBy(_ => rng.Next()).ToArray();
            return q with { Options = order.Select(i => q.Options[i]).ToArray(), Answer = Array.IndexOf(order, q.Answer) };
        }).ToList();

    // ---------- Number bases ----------
    public static string Format(long value, int radix) => radix switch
    {
        2 => Convert.ToString(value, 2),
        16 => value.ToString("X"),
        _ => value.ToString(CultureInfo.InvariantCulture),
    };

    public static bool TryParse(string? text, int radix, out long value)
    {
        value = 0;
        var s = (text ?? "").Trim().Replace(" ", "").Replace("_", "").ToLowerInvariant();
        if (radix == 16 && (s.StartsWith("0x") || s.StartsWith("#"))) s = s[(s[0] == '#' ? 1 : 2)..];
        if (radix == 2 && s.StartsWith("0b")) s = s[2..];
        if (s.Length == 0) return false;
        try
        {
            value = radix == 10 ? long.Parse(s, CultureInfo.InvariantCulture) : Convert.ToInt64(s, radix);
            return true;
        }
        catch (Exception e) when (e is FormatException or OverflowException or ArgumentException)
        {
            return false;
        }
    }

    // ---------- Bitwise puzzle ----------
    public static readonly BitOp[] BitOpPool =
    [
        new("NOT", v => ~v & 0xFF),
        new("<< 1", v => (v << 1) & 0xFF), new("<< 2", v => (v << 2) & 0xFF), new("<< 4", v => (v << 4) & 0xFF),
        new(">> 1", v => v >> 1), new(">> 2", v => v >> 2), new(">> 4", v => v >> 4),
        new("XOR 0x0F", v => v ^ 0x0F), new("XOR 0xF0", v => v ^ 0xF0), new("XOR 0x55", v => v ^ 0x55), new("XOR 0x81", v => v ^ 0x81),
        new("AND 0xF0", v => v & 0xF0), new("AND 0x0F", v => v & 0x0F), new("AND 0x3C", v => v & 0x3C),
        new("OR 0x01", v => v | 0x01), new("OR 0x80", v => v | 0x80), new("OR 0x18", v => v | 0x18),
    ];

    public static (int Start, int Target, BitOp[] Ops, int Par) NewBitPuzzle(Random rng, int moves)
    {
        while (true)
        {
            var ops = BitOpPool.OrderBy(_ => rng.Next()).Take(4).ToArray();
            var start = rng.Next(1, 256);
            var value = start;
            for (var i = 0; i < moves; i++) value = ops[rng.Next(ops.Length)].Apply(value);
            if (value == start) continue;
            var par = ShortestSolution(start, value, ops, moves);
            if (par > 1) return (start, value, ops, par);
        }
    }

    public static int ShortestSolution(int start, int target, BitOp[] ops, int maxDepth)
    {
        var frontier = new HashSet<int> { start };
        var seen = new HashSet<int> { start };
        for (var depth = 1; depth <= maxDepth; depth++)
        {
            var next = new HashSet<int>();
            foreach (var v in frontier)
                foreach (var op in ops)
                {
                    var n = op.Apply(v);
                    if (n == target) return depth;
                    if (seen.Add(n)) next.Add(n);
                }
            frontier = next;
        }
        return maxDepth;
    }

    public static string Bin8(int v) => Convert.ToString(v, 2).PadLeft(8, '0');

    // ---------- Code Wordle ----------
    public static readonly string[] WordleWords =
    [
        "async", "await", "class", "yield", "const", "break", "catch", "false", "final", "throw",
        "using", "while", "where", "float", "short", "sbyte", "ulong", "fixed", "event", "super",
        "match", "defer", "trait", "union", "alias", "array", "tuple", "slice", "macro", "regex",
        "stash", "clone",
    ];

    public static LetterState[] ScoreGuess(string guess, string answer)
    {
        var result = new LetterState[guess.Length];
        var remaining = new Dictionary<char, int>();
        for (var i = 0; i < guess.Length; i++)
        {
            if (guess[i] == answer[i]) result[i] = LetterState.Correct;
            else remaining[answer[i]] = remaining.GetValueOrDefault(answer[i]) + 1;
        }
        for (var i = 0; i < guess.Length; i++)
        {
            if (result[i] == LetterState.Correct) continue;
            if (remaining.GetValueOrDefault(guess[i]) > 0)
            {
                result[i] = LetterState.Present;
                remaining[guess[i]]--;
            }
        }
        return result;
    }

    // ---------- Keyboard shortcuts ----------
    public static readonly Dictionary<string, ShortcutItem[]> Shortcuts = new()
    {
        ["VS Code"] =
        [
            new("Command Palette", "Ctrl+Shift+P"), new("Quick Open file", "Ctrl+P"), new("Toggle terminal", "Ctrl+`"),
            new("Toggle sidebar", "Ctrl+B"), new("Go to line", "Ctrl+G"), new("Toggle line comment", "Ctrl+/"),
            new("Copy line down", "Shift+Alt+Down"), new("Move line up", "Alt+Up"), new("Search in files", "Ctrl+Shift+F"),
            new("Rename symbol", "F2"), new("Peek definition", "Alt+F12"), new("Format document", "Shift+Alt+F"),
            new("Add cursor below", "Ctrl+Alt+Down"), new("Select next occurrence", "Ctrl+D"), new("Delete line", "Ctrl+Shift+K"),
            new("Split editor", "Ctrl+\\"), new("Go to symbol in file", "Ctrl+Shift+O"), new("Quick fix", "Ctrl+."),
            new("Show Problems panel", "Ctrl+Shift+M"), new("Show Explorer", "Ctrl+Shift+E"), new("Show Source Control", "Ctrl+Shift+G"),
            new("Show Extensions", "Ctrl+Shift+X"), new("Start debugging", "F5"), new("Toggle breakpoint", "F9"),
            new("Zen mode", "Ctrl+K, Z"), new("Open settings", "Ctrl+,"),
        ],
        ["Visual Studio"] =
        [
            new("Build solution", "Ctrl+Shift+B"), new("Start debugging", "F5"), new("Start without debugging", "Ctrl+F5"),
            new("Stop debugging", "Shift+F5"), new("Toggle breakpoint", "F9"), new("Step over", "F10"),
            new("Find all references", "Shift+F12"), new("Peek definition", "Alt+F12"), new("Rename", "Ctrl+R, Ctrl+R"),
            new("Comment selection", "Ctrl+K, Ctrl+C"), new("Uncomment selection", "Ctrl+K, Ctrl+U"), new("Format document", "Ctrl+K, Ctrl+D"),
            new("Quick actions and refactorings", "Ctrl+."), new("Go to all", "Ctrl+,"), new("Search Solution Explorer", "Ctrl+;"),
            new("Navigate backward", "Ctrl+-"), new("Duplicate line", "Ctrl+D"), new("Delete line", "Ctrl+Shift+L"),
            new("Move line up", "Alt+Up"), new("Run all tests", "Ctrl+R, A"), new("QuickWatch", "Shift+F9"),
            new("Collapse to definitions", "Ctrl+M, Ctrl+O"), new("Go to line", "Ctrl+G"), new("Find in files", "Ctrl+Shift+F"),
        ],
    };

    private static readonly Dictionary<string, string> CodeNames = new()
    {
        ["Backquote"] = "`", ["Slash"] = "/", ["Backslash"] = "\\", ["Period"] = ".", ["Comma"] = ",",
        ["Semicolon"] = ";", ["Quote"] = "'", ["Minus"] = "-", ["Equal"] = "=", ["BracketLeft"] = "[", ["BracketRight"] = "]",
        ["ArrowUp"] = "Up", ["ArrowDown"] = "Down", ["ArrowLeft"] = "Left", ["ArrowRight"] = "Right", ["Escape"] = "Esc",
        ["NumpadSubtract"] = "-", ["NumpadAdd"] = "+", ["NumpadDivide"] = "/", ["NumpadEnter"] = "Enter",
    };

    /// <summary>Converts a KeyboardEvent code plus modifiers to "Ctrl+Shift+Alt+Key"; returns null for modifier-only presses.</summary>
    public static string? Combo(string code, bool ctrl, bool shift, bool alt)
    {
        if (code is "ControlLeft" or "ControlRight" or "ShiftLeft" or "ShiftRight" or "AltLeft" or "AltRight" or "MetaLeft" or "MetaRight" or "") return null;
        string key = code.StartsWith("Key") && code.Length == 4 ? code[3..]
            : code.StartsWith("Digit") && code.Length == 6 ? code[5..]
            : CodeNames.GetValueOrDefault(code, code);
        var parts = new List<string>();
        if (ctrl) parts.Add("Ctrl");
        if (shift) parts.Add("Shift");
        if (alt) parts.Add("Alt");
        parts.Add(key);
        return string.Join('+', parts);
    }

    // ---------- CSS selector game ----------
    public static readonly SelectorLevel[] SelectorLevels =
    [
        new("Select every button", "<div>\n  <button>Save</button>\n  <span>Label</span>\n  <button>Cancel</button>\n</div>", "button", "Type selector: the tag name."),
        new("Select the primary items", "<ul>\n  <li class=\"primary\">Home</li>\n  <li>Blog</li>\n  <li class=\"primary\">Docs</li>\n  <li>About</li>\n</ul>", ".primary", "Class selector starts with a dot."),
        new("Select the main section", "<div>\n  <section>Intro</section>\n  <section id=\"main\">Content</section>\n  <section>Footer</section>\n</div>", "#main", "ID selector starts with #."),
        new("Select links inside the nav only", "<div>\n  <nav>\n    <a>Home</a>\n    <a>Docs</a>\n  </nav>\n  <a>Outside</a>\n</div>", "nav a", "Descendant combinator: a space."),
        new("Select only the top-level list items", "<ul>\n  <li>One</li>\n  <li>Two\n    <ol>\n      <li>Nested</li>\n    </ol>\n  </li>\n</ul>", "ul > li", "Child combinator: >."),
        new("Select the email input", "<form>\n  <input type=\"text\">\n  <input type=\"email\">\n  <input type=\"password\">\n</form>", "input[type=email]", "Attribute selector: [attr=value]."),
        new("Select the first paragraph of each article", "<main>\n  <article>\n    <p>Lead</p>\n    <p>Body</p>\n  </article>\n  <article>\n    <p>Lead</p>\n    <p>Body</p>\n  </article>\n</main>", "p:first-child", "Pseudo-class :first-child."),
        new("Select the odd rows", "<div>\n  <p>Row 1</p>\n  <p>Row 2</p>\n  <p>Row 3</p>\n  <p>Row 4</p>\n  <p>Row 5</p>\n</div>", "p:nth-child(odd)", ":nth-child(odd) or :nth-child(2n+1)."),
        new("Select the tasks that are not done", "<ul>\n  <li class=\"task done\">Write</li>\n  <li class=\"task\">Test</li>\n  <li class=\"task done\">Review</li>\n  <li class=\"task\">Ship</li>\n</ul>", ".task:not(.done)", "Negation pseudo-class :not()."),
        new("Select the paragraph right after each heading", "<div>\n  <h2>Title</h2>\n  <p>Intro</p>\n  <p>More</p>\n  <h2>Title</h2>\n  <p>Intro</p>\n</div>", "h2 + p", "Adjacent sibling combinator: +."),
        new("Select the secure links", "<div>\n  <a href=\"https://a.dev\">A</a>\n  <a href=\"http://b.dev\">B</a>\n  <a href=\"https://c.dev\">C</a>\n  <a href=\"/local\">D</a>\n</div>", "a[href^=https]", "Attribute starts with: [attr^=value]."),
        new("Select the empty cells", "<div>\n  <span>x</span>\n  <span></span>\n  <span>o</span>\n  <span></span>\n</div>", "span:empty", "Pseudo-class :empty."),
    ];
}
