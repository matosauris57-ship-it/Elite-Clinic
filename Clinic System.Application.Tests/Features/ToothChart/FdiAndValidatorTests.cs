using Clinic_System.Application.Features.ToothChart.Models;
using Clinic_System.Application.Features.ToothChart.Validators;
using Clinic_System.Application.Features.ToothRecords.Commands.Models;
using Clinic_System.Application.Features.ToothRecords.Commands.Validators;
using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Tests.Features.ToothChart;

public class FdiAndValidatorTests
{
    [Theory]
    [InlineData(11)]
    [InlineData(48)]
    [InlineData(51)]
    [InlineData(85)]
    public void IsValid_AcceptsSupportedFdiNumbers(int toothNumber) =>
        FdiToothNumber.IsValid(toothNumber).Should().BeTrue();

    [Theory]
    [InlineData(10)]
    [InlineData(19)]
    [InlineData(49)]
    [InlineData(56)]
    [InlineData(80)]
    [InlineData(86)]
    public void IsValid_RejectsGapsAndOutOfRangeNumbers(int toothNumber) =>
        FdiToothNumber.IsValid(toothNumber).Should().BeFalse();

    [Fact]
    public void CreateEntryValidator_RejectsInvalidFdiNumber()
    {
        var validator = new CreateToothChartEntryValidator();
        var command = new CreateToothChartEntryCommand
        {
            PatientId = 1,
            ToothNumber = 49,
            Surface = ToothSurface.WholeTooth,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Healthy
        };

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ToothNumber);
    }

    [Fact]
    public void BatchValidator_RejectsDuplicateTeeth()
    {
        var validator = new BatchUpsertOdontogramValidator();
        var command = new BatchUpsertOdontogramCommand
        {
            PatientId = 1,
            Teeth =
            [
                new() { ToothNumber = 11 },
                new() { ToothNumber = 11 }
            ]
        };

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Teeth);
    }

    [Fact]
    public void Describe_ReturnsSpanishAnatomyForCanine() =>
        FdiToothNumber.Describe(23).Should().Be("Canino superior izquierdo");

    [Theory]
    [InlineData(11, true)]
    [InlineData(16, false)]
    [InlineData(41, true)]
    public void IsAnterior_MatchesIncisorsAndCanines(int tooth, bool expected) =>
        FdiToothNumber.IsAnterior(tooth).Should().Be(expected);

    [Fact]
    public void CreateEntryValidator_RejectsVeneerOnMolar()
    {
        var validator = new CreateToothChartEntryValidator();
        var command = new CreateToothChartEntryCommand
        {
            PatientId = 1,
            ToothNumber = 26,
            Surface = ToothSurface.OcclusalIncisal,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Filling,
            RestorationMaterial = RestorationMaterial.Veneer
        };

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RestorationMaterial);
    }

    [Fact]
    public void CreateEntryValidator_AcceptsAmalgamOnPosteriorFilling()
    {
        var validator = new CreateToothChartEntryValidator();
        var command = new CreateToothChartEntryCommand
        {
            PatientId = 1,
            ToothNumber = 36,
            Surface = ToothSurface.OcclusalIncisal,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Filling,
            RestorationMaterial = RestorationMaterial.Amalgam
        };

        validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.RestorationMaterial);
    }

    [Fact]
    public void CreateEntryValidator_RequiresIcdasForCaries()
    {
        var validator = new CreateToothChartEntryValidator();
        var command = new CreateToothChartEntryCommand
        {
            PatientId = 1,
            ToothNumber = 26,
            Surface = ToothSurface.OcclusalIncisal,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Caries,
            CariesType = CariesType.PitAndFissure
        };

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Icdas);
    }

    [Fact]
    public void CreateEntryValidator_RejectsCariesOnWholeTooth()
    {
        var validator = new CreateToothChartEntryValidator();
        var command = new CreateToothChartEntryCommand
        {
            PatientId = 1,
            ToothNumber = 26,
            Surface = ToothSurface.WholeTooth,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Caries,
            CariesType = CariesType.PitAndFissure,
            Icdas = IcdasCode.LocalizedEnamelBreakdown
        };

        validator.TestValidate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateEntryValidator_AcceptsCrownOnWholeToothWithPorcelain()
    {
        var validator = new CreateToothChartEntryValidator();
        var command = new CreateToothChartEntryCommand
        {
            PatientId = 1,
            ToothNumber = 24,
            Surface = ToothSurface.WholeTooth,
            Phase = ToothChartPhase.Completed,
            Condition = ToothCondition.Crown,
            RestorationMaterial = RestorationMaterial.Porcelain
        };

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateEntryValidator_AcceptsMultipleToothNumbers()
    {
        var validator = new CreateToothChartEntryValidator();
        var command = new CreateToothChartEntryCommand
        {
            PatientId = 1,
            ToothNumber = 14,
            ToothNumbers = [14, 15, 16, 17],
            Surface = ToothSurface.WholeTooth,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Healthy
        };
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ToothFindingRules_DistinguishesScope()
    {
        ToothFindingRules.MatchesScope(ToothCondition.Missing, ToothSurface.WholeTooth).Should().BeTrue();
        ToothFindingRules.MatchesScope(ToothCondition.Missing, ToothSurface.OcclusalIncisal).Should().BeFalse();
        ToothFindingRules.MatchesScope(ToothCondition.Caries, ToothSurface.OcclusalIncisal).Should().BeTrue();
    }

    [Fact]
    public void BatchValidator_RequiresToothNumbers()
    {
        var validator = new CreateToothChartEntriesBatchValidator();
        var command = new CreateToothChartEntriesBatchCommand
        {
            PatientId = 1,
            Surface = ToothSurface.WholeTooth,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Healthy
        };
        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ToothNumbers);
    }

    [Fact]
    public void BatchValidator_AcceptsMultipleHealthyTeeth()
    {
        var validator = new CreateToothChartEntriesBatchValidator();
        var command = new CreateToothChartEntriesBatchCommand
        {
            PatientId = 1,
            ToothNumbers = [14, 15, 16, 17],
            Surface = ToothSurface.WholeTooth,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Healthy
        };
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }
}
