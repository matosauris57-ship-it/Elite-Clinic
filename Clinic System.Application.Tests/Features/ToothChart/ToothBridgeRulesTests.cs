using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Tests.Features.ToothChart;

public class ToothBridgeRulesTests
{
    [Fact]
    public void InferRoles_MarksMissingIntermediateAsPontic()
    {
        var units = ToothBridgeRules.InferRoles([24, 26], t => t == 25);
        units.Select(u => u.ToothNumber).Should().Equal(24, 25, 26);
        units.Single(u => u.ToothNumber == 25).Role.Should().Be(BridgeRole.Pontic);
        units.Where(u => u.ToothNumber != 25).Should().OnlyContain(u => u.Role == BridgeRole.Abutment);
        ToothBridgeRules.Validate(units).Should().BeNull();
    }

    [Fact]
    public void ExpandSpan_IncludesUnselectedMissingBetweenEndpoints() =>
        ToothBridgeRules.ExpandSpan([24, 26]).Should().Equal(24, 25, 26);

    [Fact]
    public void Validate_RejectsMixedArches()
    {
        var units = new List<BridgeUnit>
        {
            new(16, BridgeRole.Abutment),
            new(46, BridgeRole.Pontic)
        };
        ToothBridgeRules.Validate(units).Should().Contain("arcada");
    }

    [Fact]
    public void Validate_RejectsSingleTooth() =>
        ToothBridgeRules.Validate([new BridgeUnit(24, BridgeRole.Abutment)])
            .Should().Contain("dos piezas");

    [Fact]
    public void Validate_RejectsSpanWithoutPontic()
    {
        var units = ToothBridgeRules.InferRoles([24, 25, 26], _ => false);
        ToothBridgeRules.Validate(units).Should().Contain("póntico");
    }
}
