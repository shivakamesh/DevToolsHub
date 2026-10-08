using DevToolsHub.Services;

namespace DevToolsHub.Tests;

public class ElectronicsCalcTests
{
    [Fact]
    public void TraceWidth_1A_10C_1oz_External_IsAbout12Mil()
    {
        var w = ElectronicsCalc.TraceWidthMils(1, 10, 1, false);
        Assert.InRange(w, 11.5, 12.1);
        Assert.Equal(1, ElectronicsCalc.TraceCurrent(w, 10, 1, false), 6);
    }

    [Fact]
    public void InternalTrace_IsWider() =>
        Assert.True(ElectronicsCalc.TraceWidthMils(2, 10, 1, true) > ElectronicsCalc.TraceWidthMils(2, 10, 1, false));

    [Fact]
    public void Astable_1k_10k_10uF()
    {
        var a = ElectronicsCalc.Timer555Astable(1e3, 10e3, 10e-6);
        Assert.InRange(a.FrequencyHz, 6.85, 6.9);
        Assert.InRange(a.DutyPercent, 52.3, 52.5);
    }

    [Fact]
    public void Monostable_IsAbout1_1RC() =>
        Assert.Equal(1.0986, ElectronicsCalc.Timer555Monostable(1e3, 1e-3), 3);

    [Fact]
    public void BatteryLife_DutyCycled()
    {
        var avg = ElectronicsCalc.AverageCurrent(20, 0.05, 1);
        Assert.Equal(0.2495, avg, 6);
        Assert.Equal(1000, ElectronicsCalc.BatteryHours(1000, 1, 1));
        Assert.True(double.IsPositiveInfinity(ElectronicsCalc.BatteryHours(1000, 0, 1)));
    }

    [Theory]
    [InlineData("103", 10_000)]
    [InlineData("472", 4_700)]
    [InlineData("1002", 10_000)]
    [InlineData("4R7", 4.7)]
    [InlineData("R10", 0.1)]
    [InlineData("01C", 10_000)]
    [InlineData("68X", 49.9)]
    [InlineData("000", 0)]
    public void DecodeSmd(string code, double ohms) =>
        Assert.Equal(ohms, ElectronicsCalc.DecodeSmd(code)!.Value.Ohms, 6);

    [Fact]
    public void DecodeSmd_Invalid_ReturnsNull() => Assert.Null(ElectronicsCalc.DecodeSmd("ABC"));
}
