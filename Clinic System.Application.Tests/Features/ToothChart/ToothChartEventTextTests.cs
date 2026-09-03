using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Tests.Features.ToothChart;

public class ToothChartEventTextTests
{
    [Fact]
    public void BuildTitle_UsesSpanishClinicalTerms()
    {
        var title = ToothChartEventText.BuildTitle(
            ToothChartPhase.Diagnosis,
            42,
            ToothSurface.Mesial,
            ToothCondition.Caries,
            null,
            CariesType.PitAndFissure,
            IcdasCode.InitialVisual);

        title.Should().Be("Diagnóstico · pieza 42 · Mesial · Caries · Fosas y fisuras · ICDAS 1");
    }

    [Fact]
    public void LocalizeTitle_TranslatesStoredEnglishTokens()
    {
        var localized = ToothChartEventText.LocalizeTitle(
            "Diagnosis · diente 42 · BuccalFacial · Caries · PitAndFissure · ICDAS 1");

        localized.Should().Be("Diagnóstico · pieza 42 · Vestibular / Facial · Caries · Fosas y fisuras · ICDAS 1");
    }

    [Fact]
    public void LocalizeTitle_TranslatesLegacyColonFormat()
    {
        var localized = ToothChartEventText.LocalizeTitle("Planned: diente 42 (BuccalFacial)");

        localized.Should().Be("Planificado: pieza 42 (Vestibular / Facial)");
    }
}
