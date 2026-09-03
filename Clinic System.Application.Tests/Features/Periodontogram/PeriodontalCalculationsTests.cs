using Clinic_System.Application.DTOs.Dental;
using Clinic_System.Application.Features.Periodontogram.Models;
using Clinic_System.Application.Features.Periodontogram.Validators;
using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Tests.Features.Periodontogram;

public class PeriodontalCalculationsTests
{
    [Fact]
    public void CalculateCal_AddsProbingAndRecession() =>
        PeriodontalCalculations.CalculateCal(5, 2).Should().Be(7);

    [Fact]
    public void CalculateCal_ReturnsNullWhenBothMissing() =>
        PeriodontalCalculations.CalculateCal(null, null).Should().BeNull();

    [Theory]
    [InlineData(3, PeriodontalPocketSeverity.Normal)]
    [InlineData(4, PeriodontalPocketSeverity.Alert)]
    [InlineData(6, PeriodontalPocketSeverity.Elevated)]
    [InlineData(null, PeriodontalPocketSeverity.Unrecorded)]
    public void GetPocketSeverity_UsesCentralThresholds(int? pd, PeriodontalPocketSeverity expected) =>
        PeriodontalCalculations.GetPocketSeverity(pd).Should().Be(expected);

    [Fact]
    public void HasFurcation_IsTrueForMolarsAndUpperFirstPremolars()
    {
        FdiToothNumber.HasFurcation(16).Should().BeTrue();
        FdiToothNumber.HasFurcation(14).Should().BeTrue();
        FdiToothNumber.HasFurcation(11).Should().BeFalse();
        FdiToothNumber.HasFurcation(44).Should().BeFalse();
    }

    [Fact]
    public void ComputeIndices_CalculatesBleedingPlaqueAndDeepSites()
    {
        var teeth = new List<PeriodontalTooth>
        {
            new()
            {
                ToothNumber = 16,
                Status = PeriodontalToothStatus.Present,
                Sites =
                [
                    new() { ProbingDepthMm = 3, Bleeding = true, Plaque = true },
                    new() { ProbingDepthMm = 5, Bleeding = true },
                    new() { ProbingDepthMm = 6, Plaque = true },
                    new() { ProbingDepthMm = 2 }
                ]
            },
            new()
            {
                ToothNumber = 11,
                Status = PeriodontalToothStatus.Missing,
                Sites = [new() { ProbingDepthMm = 9, Bleeding = true, Plaque = true }]
            }
        };

        var indices = PeriodontalCalculations.ComputeIndices(teeth);
        indices.RecordedSiteCount.Should().Be(4);
        indices.BleedingPercent.Should().Be(50m);
        indices.PlaquePercent.Should().Be(50m);
        indices.MeanProbingDepthMm.Should().Be(4.0m);
        indices.SitesDeepGe5.Should().Be(2);
        indices.SitesDeepGe6.Should().Be(1);
    }

    [Theory]
    [InlineData(ToothCondition.Missing, PeriodontalToothStatus.Missing)]
    [InlineData(ToothCondition.Extracted, PeriodontalToothStatus.Missing)]
    [InlineData(ToothCondition.Implant, PeriodontalToothStatus.Implant)]
    [InlineData(ToothCondition.Healthy, PeriodontalToothStatus.Present)]
    [InlineData(ToothCondition.Bridge, PeriodontalToothStatus.Present)]
    public void StatusFromOdontogram_MapsClinicalConditions(ToothCondition condition, PeriodontalToothStatus expected) =>
        PeriodontalCalculations.StatusFromOdontogram(condition).Should().Be(expected);

    [Fact]
    public void StatusFromOdontogram_TreatsBridgePonticAsMissing() =>
        PeriodontalCalculations.StatusFromOdontogram(ToothCondition.Bridge, BridgeRole.Pontic)
            .Should().Be(PeriodontalToothStatus.Missing);

    [Fact]
    public void SaveValidator_RejectsNegativeProbingDepth()
    {
        var validator = new SavePeriodontalExamValidator();
        var command = new SavePeriodontalExamCommand
        {
            ExamId = 1,
            Teeth =
            [
                new()
                {
                    ToothNumber = 16,
                    Sites = [new() { Surface = PeriodontalSurface.Facial, Position = PeriodontalSitePosition.Center, ProbingDepthMm = -1 }]
                }
            ]
        };

        validator.TestValidate(command).ShouldHaveValidationErrorFor("Teeth[0].Sites[0].ProbingDepthMm");
    }

    [Fact]
    public void SaveValidator_RejectsKeratinizedGingivaOutOfRange()
    {
        var validator = new SavePeriodontalExamValidator();
        var command = new SavePeriodontalExamCommand
        {
            ExamId = 1,
            Teeth =
            [
                new()
                {
                    ToothNumber = 16,
                    KeratinizedGingivaMm = 99
                }
            ]
        };

        validator.TestValidate(command).ShouldHaveValidationErrorFor("Teeth[0].KeratinizedGingivaMm");
    }

    [Fact]
    public void SaveValidator_RejectsFurcationOnIncisor()
    {
        var validator = new SavePeriodontalExamValidator();
        var command = new SavePeriodontalExamCommand
        {
            ExamId = 1,
            Teeth =
            [
                new()
                {
                    ToothNumber = 11,
                    Status = PeriodontalToothStatus.Present,
                    FacialFurcation = PeriodontalFurcation.Grade2
                }
            ]
        };

        var result = validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("furcación", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void NextTooth_WalksTheClinicalArch() =>
        PeriodontalCalculations.NextTooth(11).Should().Be(21);
}
