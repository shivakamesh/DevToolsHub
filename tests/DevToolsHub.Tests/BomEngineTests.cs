using DevToolsHub.Services;

namespace DevToolsHub.Tests;

public class BomTests
{
    [Fact]
    public void ConsolidatesByMpn_AndCountsReferences()
    {
        const string csv = "Reference,Value,MPN,Unit Price\n\"R1 R2\",10k,RC0603-10K,0.01\nR3,10k,RC0603-10K,0.01\n";
        var line = Assert.Single(BomEngine.Calculate(csv, 10, 0).Lines);
        Assert.Equal(3, line.PerBoard);
        Assert.Equal(30, line.OrderQty);
        Assert.Equal(0.30m, line.Extended);
    }

    [Fact]
    public void Attrition_RoundsUp() =>
        Assert.Equal(11, BomEngine.Calculate("Qty,MPN,Price\n1,A,1", 10, 5).Lines[0].Required);

    [Fact]
    public void Moq_And_Multiple_AreApplied()
    {
        var r = BomEngine.Calculate("Qty,MPN,Price,MOQ,Multiple\n2,A,1,50,25", 10, 0);
        Assert.Equal(20, r.Lines[0].Required);
        Assert.Equal(50, r.Lines[0].OrderQty);
    }

    [Fact]
    public void PriceBreaks_UseHighestApplicableBreak()
    {
        var r = BomEngine.Calculate("Qty,MPN,Price Breaks\n1,A,\"1:0.10; 100:0.05; 1000:0.02\"", 150, 0);
        Assert.Equal(0.05m, r.Lines[0].UnitPrice);
        Assert.Equal(7.50m, r.Lines[0].Extended);
    }

    [Fact]
    public void QuotedCells_WithSemicolonDelimiter()
    {
        var rows = BomEngine.ParseCsv("a;b\n\"x;\"\"y\"\";z\";2");
        Assert.Equal("x;\"y\";z", rows[1][0]);
        Assert.Equal("2", rows[1][1]);
    }

    [Fact]
    public void MissingColumns_ReturnsError() =>
        Assert.NotNull(BomEngine.Calculate("Foo,Bar\n1,2", 1, 0).Error);

    [Fact]
    public void Csv_EscapesQuotes() =>
        Assert.Contains("\"a \"\"b\"\"\"", BomEngine.ToCsv([new BomLine("a \"b\"", "", "", 1, 1, 1, 1, 1, 1, 1)]));

    [Theory]
    [InlineData(0, 10, 5, 0)]
    [InlineData(7, 1, 1, 7)]
    [InlineData(7, 10, 1, 10)]
    [InlineData(26, 1, 25, 50)]
    public void OrderQuantity(int required, int moq, int mult, int expected) =>
        Assert.Equal(expected, BomEngine.OrderQuantity(required, moq, mult));
}
