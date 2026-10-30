using DevToolsHub.Services;
using Xunit;

namespace DevToolsHub.Tests;

public class NewToolsTests
{
    [Fact]
    public void VimLevels_ParSolutionsReachTarget()
    {
        foreach (var level in VimEngine.Levels)
        {
            var vim = new VimEngine(level.Start);
            vim.Feed(level.Par);
            Assert.True(level.Target == vim.Text, $"{level.Title}: got '{vim.Text}'");
            Assert.Equal(VimMode.Normal, vim.Mode);
        }
    }

    [Fact]
    public void Vim_UndoRestoresText()
    {
        var vim = new VimEngine("abc\ndef");
        vim.Feed("ddxu");
        Assert.Equal("def", vim.Text);
        vim.Feed("u");
        Assert.Equal("abc\ndef", vim.Text);
    }

    [Fact]
    public void Vim_CountsAndMotions()
    {
        var vim = new VimEngine("one two three four");
        vim.Feed("2w");
        Assert.Equal(8, vim.Col);
        vim.Feed("b");
        Assert.Equal(4, vim.Col);
        vim.Feed("$");
        Assert.Equal(17, vim.Col);
        vim.Feed("0");
        Assert.Equal(0, vim.Col);
    }

    [Fact]
    public void Quizzes_HaveValidAnswersAndCode()
    {
        foreach (var q in MoreGames.BigOQuestions.Concat(MoreGames.LanguageQuestions))
        {
            Assert.InRange(q.Answer, 0, q.Options.Length - 1);
            Assert.Equal(q.Options.Length, q.Options.Distinct().Count());
            Assert.False(string.IsNullOrWhiteSpace(q.Code));
        }
    }

    [Fact]
    public void SqlMystery_ChecksSuspectCaseInsensitively()
    {
        Assert.True(MoreGames.CheckSuspect("  jeremy bowers "));
        Assert.False(MoreGames.CheckSuspect("Tom Thread"));
    }

    [Theory]
    [InlineData("#000000", "#FFFFFF", 21.0)]
    [InlineData("#FFFFFF", "#FFFFFF", 1.0)]
    [InlineData("#767676", "#FFFFFF", 4.54)]
    public void Contrast_RatioMatchesWcag(string fg, string bg, double expected)
    {
        Assert.True(ContrastChecker.TryParseHex(fg, out var a));
        Assert.True(ContrastChecker.TryParseHex(bg, out var b));
        Assert.Equal(expected, ContrastChecker.Ratio(a, b), 2);
    }

    [Fact]
    public void Contrast_SuggestReachesTarget()
    {
        ContrastChecker.TryParseHex("#AAAAAA", out var fg);
        ContrastChecker.TryParseHex("#FFF", out var bg);
        var s = ContrastChecker.Suggest(fg, bg, 4.5);
        Assert.NotNull(s);
        Assert.True(ContrastChecker.Ratio(s.Value, bg) >= 4.5);
    }

    [Fact]
    public void ConnectionStrings_BuildExpectedFormats()
    {
        Assert.Equal("Server=db,1444;Database=App;User Id=sa;Password=p;TrustServerCertificate=True;",
            ConnectionStrings.Build(new(DbKind.SqlServer, "db", 1444, "App", "sa", "p", TrustServerCertificate: true)));
        Assert.Equal("Host=localhost;Port=5432;Database=app;Username=u;Password=p;SSL Mode=Require;",
            ConnectionStrings.Build(new(DbKind.PostgreSql, "localhost", null, "app", "u", "p", Ssl: true)));
        Assert.Equal("mongodb://u:p%40ss@h:27017/db",
            ConnectionStrings.Build(new(DbKind.MongoDb, "h", null, "db", "u", "p@ss")));
        Assert.Equal("cache:6379,password=pw,ssl=True,defaultDatabase=2",
            ConnectionStrings.Build(new(DbKind.Redis, "cache", null, "2", "", "pw", Ssl: true)));
    }

    [Fact]
    public void DevOps_GeneratesDockerfileAndWorkflow()
    {
        var docker = DevOpsGenerator.Dockerfile(new(AppStack.DotNet, "8.0", "Api", 8080));
        Assert.Contains("FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build", docker);
        Assert.Contains("ENTRYPOINT [\"dotnet\", \"Api.dll\"]", docker);

        var wf = DevOpsGenerator.GitHubWorkflow(new(AppStack.Node, "20", "main", true, true, "Me/App"));
        Assert.Contains("node-version: '20'", wf);
        Assert.Contains("npm test", wf);
        Assert.Contains("ghcr.io/me/app:${{ github.sha }}", wf);
    }
}
