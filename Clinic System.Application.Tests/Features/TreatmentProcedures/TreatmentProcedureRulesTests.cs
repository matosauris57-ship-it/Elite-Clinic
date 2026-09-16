using Clinic_System.Core.Enums;
using Clinic_System.Core.Validation;
using FluentAssertions;
using Xunit;

namespace Clinic_System.Application.Tests.Features.TreatmentProcedures;

public class TreatmentProcedureRulesTests
{
    [Fact]
    public void Extraction_cannot_be_assigned_to_whole_mouth()
    {
        var error = TreatmentProcedureRules.ValidateAssignment(
            TreatmentProcedureTarget.PerTooth,
            TreatmentToothKindFilter.Any,
            toothNumber: null,
            surface: null);

        error.Should().Contain("odontograma");
    }

    [Fact]
    public void Cleaning_rejects_a_specific_tooth()
    {
        var error = TreatmentProcedureRules.ValidateAssignment(
            TreatmentProcedureTarget.WholeMouth,
            TreatmentToothKindFilter.Any,
            toothNumber: 16,
            surface: null);

        error.Should().Contain("toda la boca");
    }

    [Fact]
    public void Molar_extraction_rejects_an_incisor()
    {
        var error = TreatmentProcedureRules.ValidateAssignment(
            TreatmentProcedureTarget.PerTooth,
            TreatmentToothKindFilter.Molar,
            toothNumber: 11,
            surface: ToothSurface.WholeTooth);

        error.Should().Contain("molares");
    }

    [Fact]
    public void Sealant_requires_a_surface()
    {
        var error = TreatmentProcedureRules.ValidateAssignment(
            TreatmentProcedureTarget.PerSurface,
            TreatmentToothKindFilter.Any,
            toothNumber: 16,
            ToothSurface.WholeTooth);

        error.Should().Contain("superficie");
    }

    [Fact]
    public void Sealant_on_occlusal_molar_is_valid()
    {
        var error = TreatmentProcedureRules.ValidateAssignment(
            TreatmentProcedureTarget.PerSurface,
            TreatmentToothKindFilter.Any,
            toothNumber: 16,
            ToothSurface.OcclusalIncisal);

        error.Should().BeNull();
    }

    [Theory]
    [InlineData("limpieza", TreatmentProcedureTarget.WholeMouth)]
    [InlineData("extraccion-simple", TreatmentProcedureTarget.PerTooth)]
    [InlineData("sellantes", TreatmentProcedureTarget.PerSurface)]
    public void Seed_codes_map_to_expected_targets(string code, TreatmentProcedureTarget expected)
    {
        TreatmentProcedureRules.DefaultsForCode(code).Target.Should().Be(expected);
    }
}
