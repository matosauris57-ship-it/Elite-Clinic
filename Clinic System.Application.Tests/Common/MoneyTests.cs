using Clinic_System.Core.Finance;

namespace Clinic_System.Application.Tests.Common;

public class MoneyTests
{
    [Theory]
    [InlineData("1500.5", 1500.50)]
    [InlineData("1,500.50", 1500.50)]
    [InlineData("1500,50", 1500.50)]
    [InlineData("RD$ 2500", 2500.00)]
    [InlineData("1.500,75", 1500.75)]
    public void TryParse_AcceptsCommonMoneyInputs(string input, double expected)
    {
        Money.TryParse(input, out var amount).Should().BeTrue();
        amount.Should().Be((decimal)expected);
    }

    [Fact]
    public void Normalize_RoundsAwayFromZeroToTwoDecimals()
    {
        Money.Normalize(10.455m).Should().Be(10.46m);
        Money.Multiply(10.555m, 2).Should().Be(21.12m);
    }

    [Fact]
    public void Format_UsesDominicanCurrency()
    {
        var formatted = Money.Format(1500.5m);
        formatted.Should().Contain("$");
        formatted.Should().MatchRegex(@"1[.,]500[.,]50");
    }
}
