using DevToolsHub.Services;
using Xunit;

namespace DevToolsHub.Tests;

public class DevGamesTests
{
    [Fact]
    public void RegexPuzzle_ParSolutionsSolveEveryPuzzle()
    {
        foreach (var p in DevGames.RegexPuzzles)
        {
            var r = DevGames.EvaluateRegex(p, p.Par);
            Assert.NotNull(r);
            Assert.All(r.Value.Match, ok => Assert.True(ok, p.Title));
            Assert.All(r.Value.Avoid, ok => Assert.True(ok, p.Title));
        }
    }

    [Fact]
    public void RegexPuzzle_InvalidPatternReturnsNull() =>
        Assert.Null(DevGames.EvaluateRegex(DevGames.RegexPuzzles[0], "("));

    [Fact]
    public void WordleWords_AreFiveLowercaseLetters() =>
        Assert.All(DevGames.WordleWords, w => Assert.Matches("^[a-z]{5}$", w));

    [Theory]
    [InlineData("class", "class", "CCCCC")]
    [InlineData("sbyte", "yield", "AAPAP")]
    [InlineData("allay", "array", "CAACC")]
    [InlineData("eerie", "event", "CPAAA")]
    public void ScoreGuess_HandlesDuplicates(string guess, string answer, string expected)
    {
        var map = new Dictionary<LetterState, char> { [LetterState.Correct] = 'C', [LetterState.Present] = 'P', [LetterState.Absent] = 'A' };
        Assert.Equal(expected, new string(DevGames.ScoreGuess(guess, answer).Select(s => map[s]).ToArray()));
    }

    [Theory]
    [InlineData("0xFF", 16, 255)]
    [InlineData("ff", 16, 255)]
    [InlineData("0b1010", 2, 10)]
    [InlineData("1111 0000", 2, 240)]
    [InlineData("42", 10, 42)]
    public void TryParse_AcceptsPrefixesAndSpaces(string text, int radix, long expected)
    {
        Assert.True(DevGames.TryParse(text, radix, out var v));
        Assert.Equal(expected, v);
    }

    [Fact]
    public void TryParse_RejectsInvalidDigits() => Assert.False(DevGames.TryParse("102", 2, out _));

    [Theory]
    [InlineData("KeyP", true, true, false, "Ctrl+Shift+P")]
    [InlineData("Backquote", true, false, false, "Ctrl+`")]
    [InlineData("ArrowDown", false, true, true, "Shift+Alt+Down")]
    [InlineData("F5", false, false, false, "F5")]
    [InlineData("ShiftLeft", false, true, false, null)]
    public void Combo_FormatsLikeShortcutData(string code, bool ctrl, bool shift, bool alt, string? expected) =>
        Assert.Equal(expected, DevGames.Combo(code, ctrl, shift, alt));

    [Fact]
    public void BitPuzzle_IsSolvableWithinPar()
    {
        var rng = new Random(1);
        for (var i = 0; i < 50; i++)
        {
            var (start, target, ops, par) = DevGames.NewBitPuzzle(rng, 5);
            Assert.NotEqual(start, target);
            Assert.Equal(par, DevGames.ShortestSolution(start, target, ops, 5));
        }
    }

    [Fact]
    public void HttpQuiz_HasFourDistinctOptionsWithCorrectAnswer()
    {
        var quiz = DevGames.BuildHttpQuiz(10, false, new Random(2));
        Assert.Equal(10, quiz.Count);
        Assert.All(quiz, q =>
        {
            Assert.Equal(4, q.Options.Distinct().Count());
            Assert.Contains(q.Options[q.Answer], q.Explanation);
        });
    }

    [Fact]
    public void Shuffle_KeepsCorrectAnswer()
    {
        var shuffled = DevGames.Shuffle(DevGames.GitQuestions, new Random(3));
        foreach (var q in shuffled)
        {
            var original = DevGames.GitQuestions.Single(o => o.Prompt == q.Prompt);
            Assert.Equal(original.Options[original.Answer], q.Options[q.Answer]);
        }
    }
}
