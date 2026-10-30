namespace DevToolsHub.Services;

public static class MoreGames
{
    // ---------- Big-O quiz ----------
    private static readonly string[] Complexities = ["O(1)", "O(log n)", "O(n)", "O(n log n)", "O(n²)", "O(2ⁿ)"];

    private static QuizQuestion BigO(string prompt, string code, string answer, string explanation)
    {
        var others = Complexities.Where(c => c != answer).Take(3);
        return new QuizQuestion(prompt, [answer, .. others], 0, explanation, code);
    }

    public static readonly QuizQuestion[] BigOQuestions =
    [
        BigO("Time complexity of reading an array element by index?", "int x = items[42];", "O(1)", "Arrays support direct addressing."),
        BigO("Time complexity of this loop?", "for (int i = 0; i < n; i++)\n    sum += a[i];", "O(n)", "One pass over n elements."),
        BigO("Time complexity of these nested loops?", "for (int i = 0; i < n; i++)\n    for (int j = 0; j < n; j++)\n        count++;", "O(n²)", "n iterations inside n iterations."),
        BigO("Time complexity of this loop?", "for (int i = 1; i < n; i *= 2)\n    Console.WriteLine(i);", "O(log n)", "i doubles each time, so the loop runs log₂ n times."),
        BigO("Time complexity of binary search on a sorted array?", "while (lo <= hi) {\n    var mid = (lo + hi) / 2;\n    if (a[mid] == t) return mid;\n    if (a[mid] < t) lo = mid + 1; else hi = mid - 1;\n}", "O(log n)", "The search space halves every step."),
        BigO("Average time complexity of a Dictionary/HashMap lookup?", "var v = dict[key];", "O(1)", "Hashing gives constant time on average."),
        BigO("Time complexity of this naive recursive Fibonacci?", "int Fib(int n) => n < 2 ? n : Fib(n - 1) + Fib(n - 2);", "O(2ⁿ)", "Each call branches into two more calls."),
        BigO("Time complexity of merge sort?", "Sort(left); Sort(right); Merge(left, right);", "O(n log n)", "log n levels of splitting, O(n) merge work per level."),
        BigO("Worst-case time complexity of bubble sort?", "for (i = 0; i < n; i++)\n  for (j = 0; j < n - i - 1; j++)\n    if (a[j] > a[j+1]) Swap(j, j+1);", "O(n²)", "About n²/2 comparisons."),
        BigO("Time complexity of List.Contains / Array.IndexOf?", "bool found = list.Contains(x);", "O(n)", "It scans the list linearly."),
        BigO("Time complexity of pushing onto or popping from a stack?", "stack.Push(x);\nvar top = stack.Pop();", "O(1)", "Both operate on the top only (amortized)."),
        BigO("Time complexity of Array.Sort / List.Sort in .NET?", "Array.Sort(items);", "O(n log n)", ".NET uses introsort, O(n log n) on average and worst case."),
        BigO("Time complexity of this code?", "for (int i = 0; i < n; i++) { }\nfor (int j = 0; j < n; j++) { }", "O(n)", "Two sequential loops are O(2n), which simplifies to O(n)."),
        BigO("Time complexity of finding an item in a balanced BST?", "while (node != null && node.Value != x)\n    node = x < node.Value ? node.Left : node.Right;", "O(log n)", "A balanced tree has height log n."),
        BigO("Time complexity of generating all subsets of a set?", "void Subsets(int i) {\n  if (i == n) { Print(); return; }\n  Subsets(i + 1); take[i] = true; Subsets(i + 1); take[i] = false;\n}", "O(2ⁿ)", "There are 2ⁿ subsets."),
        BigO("Time complexity of this loop?", "for (int i = 0; i < n; i++)\n    for (int j = 1; j < n; j *= 2)\n        work();", "O(n log n)", "n outer iterations × log n inner iterations."),
        BigO("Time complexity of inserting at the start of a List<T>/ArrayList?", "list.Insert(0, x);", "O(n)", "Every element must shift one position."),
        BigO("Time complexity of checking all pairs for duplicates?", "for (i = 0; i < n; i++)\n  for (j = i + 1; j < n; j++)\n    if (a[i] == a[j]) return true;", "O(n²)", "n(n-1)/2 comparisons. A HashSet would make it O(n)."),
    ];

    // ---------- Guess the language ----------
    private static QuizQuestion Lang(string code, string answer, string explanation, params string[] decoys) =>
        new("Which language is this?", [answer, .. decoys], 0, explanation, code);

    public static readonly QuizQuestion[] LanguageQuestions =
    [
        Lang("fn main() {\n    let mut v = vec![1, 2, 3];\n    v.push(4);\n    println!(\"{:?}\", v);\n}", "Rust", "fn, let mut, vec! and println! macros.", "Go", "Swift", "Kotlin"),
        Lang("package main\n\nimport \"fmt\"\n\nfunc main() {\n    x := 42\n    fmt.Println(x)\n}", "Go", "package main, func and the := short declaration.", "Rust", "Swift", "Dart"),
        Lang("def greet(name):\n    return f\"Hello, {name}!\"\n\nprint(greet(\"world\"))", "Python", "def, f-strings and indentation-based blocks.", "Ruby", "Elixir", "Lua"),
        Lang("def greet(name)\n  \"Hello, #{name}!\"\nend\n\nputs greet(\"world\")", "Ruby", "def ... end, #{} interpolation and puts.", "Python", "Elixir", "Lua"),
        Lang("<?php\n$name = 'world';\necho \"Hello, $name!\";", "PHP", "<?php tag and $variables.", "Bash", "Ruby", "JavaScript"),
        Lang("fun main() {\n    val names = listOf(\"a\", \"b\")\n    names.forEach { println(it) }\n}", "Kotlin", "fun, val, listOf and the implicit it parameter.", "Swift", "Scala", "Go"),
        Lang("let names = [\"a\", \"b\"]\nfor name in names {\n    print(name)\n}\nguard let x = opt else { return }", "Swift", "guard let and for-in without parentheses.", "Kotlin", "Rust", "Dart"),
        Lang("var adults = people.Where(p => p.Age >= 18)\n                    .Select(p => p.Name)\n                    .ToList();", "C#", "LINQ methods Where/Select/ToList with PascalCase.", "Java", "TypeScript", "Kotlin"),
        Lang("List<String> names = people.stream()\n    .filter(p -> p.getAge() >= 18)\n    .map(Person::getName)\n    .collect(Collectors.toList());", "Java", "Streams, -> lambdas and Collectors.", "C#", "Kotlin", "Scala"),
        Lang("interface User {\n  id: number;\n  name?: string;\n}\nconst u: User = { id: 1 };", "TypeScript", "Interfaces and type annotations on top of JavaScript.", "JavaScript", "Dart", "Swift"),
        Lang("const sum = arr.reduce((acc, x) => acc + x, 0);\ndocument.querySelector('#out').textContent = sum;", "JavaScript", "Arrow functions and the DOM API without type annotations.", "TypeScript", "Dart", "C#"),
        Lang("#include <iostream>\n#include <vector>\nint main() {\n    std::vector<int> v{1, 2, 3};\n    std::cout << v.size() << std::endl;\n}", "C++", "std:: namespace, iostream and templates.", "C", "Rust", "C#"),
        Lang("#include <stdio.h>\nint main(void) {\n    char *s = malloc(10);\n    printf(\"%d\\n\", 42);\n    free(s);\n}", "C", "stdio.h, printf and manual malloc/free.", "C++", "Go", "Rust"),
        Lang("quicksort [] = []\nquicksort (p:xs) = quicksort [x | x <- xs, x < p]\n               ++ [p]\n               ++ quicksort [x | x <- xs, x >= p]", "Haskell", "Pattern matching on (p:xs) and list comprehensions.", "Elixir", "F#", "Scala"),
        Lang("defmodule Math do\n  def sum(a, b), do: a + b\nend\n\n[1, 2, 3] |> Enum.map(&(&1 * 2))", "Elixir", "defmodule, the |> pipe and & capture syntax.", "Ruby", "Haskell", "F#"),
        Lang("let square x = x * x\n[1; 2; 3]\n|> List.map square\n|> printfn \"%A\"", "F#", "let-bound functions, ; separated lists and printfn.", "Haskell", "Elixir", "Scala"),
        Lang("object Main extends App {\n  val xs = List(1, 2, 3)\n  xs.map(_ * 2).foreach(println)\n}", "Scala", "object ... extends App and the _ placeholder.", "Kotlin", "Java", "F#"),
        Lang("local t = { 1, 2, 3 }\nfor i, v in ipairs(t) do\n  print(i, v)\nend", "Lua", "local, ipairs and do ... end.", "Ruby", "Python", "Bash"),
        Lang("#!/bin/bash\nfor f in *.log; do\n  echo \"$f\"\ndone\n[ -z \"$VAR\" ] && exit 1", "Bash", "Shebang, for ... do ... done and [ ] tests.", "PHP", "Lua", "Ruby"),
        Lang("void main() {\n  final list = <int>[1, 2, 3];\n  list.forEach((x) => print(x));\n  String? name;\n}", "Dart", "void main(), final, <int>[] literals and print.", "Java", "TypeScript", "Kotlin"),
    ];

    // ---------- SQL mystery ----------
    public const string SqlMysterySchema = """
CREATE TABLE crime_scene_report (date INTEGER, type TEXT, description TEXT, city TEXT);
CREATE TABLE person (id INTEGER PRIMARY KEY, name TEXT, license_id INTEGER, address_number INTEGER, address_street_name TEXT);
CREATE TABLE drivers_license (id INTEGER PRIMARY KEY, age INTEGER, height INTEGER, eye_color TEXT, hair_color TEXT, gender TEXT, plate_number TEXT, car_make TEXT);
CREATE TABLE interview (person_id INTEGER, transcript TEXT);
CREATE TABLE gym_member (id TEXT PRIMARY KEY, person_id INTEGER, name TEXT, membership_status TEXT);
CREATE TABLE gym_checkin (membership_id TEXT, check_in_date INTEGER, check_in_time INTEGER, check_out_time INTEGER);

INSERT INTO crime_scene_report VALUES
 (20240114, 'theft', 'A laptop was stolen from a parked car.', 'Bytesville'),
 (20240115, 'murder', 'Security footage shows two witnesses. The first lives at the last house on Null Street. The second, named Annabel, lives somewhere on Lambda Avenue.', 'Codeburg'),
 (20240115, 'murder', 'Victim found near the river. Case closed: accidental.', 'Bytesville'),
 (20240115, 'assault', 'Bar fight, no serious injuries.', 'Codeburg'),
 (20240116, 'fraud', 'Fake invoices sent to a local company.', 'Codeburg');

INSERT INTO drivers_license VALUES
 (1001, 34, 70, 'brown', 'black', 'male', 'T42W', 'Tesla'),
 (1002, 29, 64, 'blue', 'red', 'female', 'K9X1', 'Toyota'),
 (1003, 51, 68, 'green', 'grey', 'male', 'Z7Q2', 'Ford'),
 (1004, 41, 72, 'brown', 'brown', 'male', '0H42', 'Honda'),
 (1005, 38, 66, 'hazel', 'blonde', 'female', 'P3R5', 'BMW'),
 (1006, 45, 71, 'blue', 'black', 'male', 'AH42K', 'Audi'),
 (1007, 27, 69, 'brown', 'black', 'male', 'TT12', 'Kia');

INSERT INTO person VALUES
 (1, 'Morty Pointer', 1003, 4919, 'Null Street'),
 (2, 'Annabel Async', 1002, 103, 'Lambda Avenue'),
 (3, 'Jeremy Bowers', 1006, 530, 'Pointer Lane'),
 (4, 'Victor Stack', 1004, 77, 'Heap Road'),
 (5, 'Linda Queue', 1005, 12, 'Null Street'),
 (6, 'Tom Thread', 1001, 9, 'Mutex Way'),
 (7, 'Greg Garbage', 1007, 40, 'Heap Road');

INSERT INTO interview VALUES
 (1, 'I heard a gunshot and saw a man run out. He had a "Get Fit Now Gym" bag. The membership number on the bag started with "48Z". Only gold members have those bags. The man got into a car with a plate that included "H42".'),
 (2, 'I saw the murder happen, and I recognized the killer from my gym when I was working out last week on January the 9th.'),
 (4, 'I was home all night.'),
 (6, 'I did not see anything.');

INSERT INTO gym_member VALUES
 ('48Z7A', 3, 'Jeremy Bowers', 'gold'),
 ('48Z55', 6, 'Tom Thread', 'gold'),
 ('48Z9K', 4, 'Victor Stack', 'silver'),
 ('90081', 2, 'Annabel Async', 'gold'),
 ('11AB2', 7, 'Greg Garbage', 'regular');

INSERT INTO gym_checkin VALUES
 ('48Z7A', 20240109, 1530, 1700),
 ('48Z55', 20240109, 1530, 1600),
 ('90081', 20240109, 1600, 1700),
 ('48Z9K', 20240109, 1700, 1800),
 ('11AB2', 20240110, 900, 1000);
""";

    public const string SqlMysteryKiller = "Jeremy Bowers";

    public static bool CheckSuspect(string? name) =>
        string.Equals((name ?? "").Trim(), SqlMysteryKiller, StringComparison.OrdinalIgnoreCase);
}
