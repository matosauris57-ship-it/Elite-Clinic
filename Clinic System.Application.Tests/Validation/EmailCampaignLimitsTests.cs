using Clinic_System.Application.Common;

namespace Clinic_System.Application.Tests.Validation;

public class EmailCampaignLimitsTests
{
    [Fact]
    public void BatchSize_IsFifteen()
    {
        EmailCampaignLimits.BatchSize.Should().Be(15);
    }

    [Fact]
    public void Apply_ReplacesPatientAndClinic()
    {
        var text = EmailCampaignLimits.Apply("Hola {nombre}, escribe {clinica}.", "Los Prados", "Ana");
        text.Should().Be("Hola Ana, escribe Los Prados.");
    }

    [Fact]
    public void Apply_ReplacesBookingLink()
    {
        var text = EmailCampaignLimits.Apply(
            "Agenda aquí: {enlace_cita}",
            "Los Prados",
            "Ana",
            "https://clinic.example/agendar-campana?token=abc");

        text.Should().Be("Agenda aquí: https://clinic.example/agendar-campana?token=abc");
    }

    [Fact]
    public void AppendFooter_AddsClinicNotice()
    {
        var body = EmailCampaignLimits.AppendFooter("Hola", "Los Prados");
        body.Should().Contain("Los Prados");
        body.Should().Contain("pacientes de la clínica");
    }

    [Fact]
    public void AppendFooter_AddsBookingLink_WhenMissingFromBody()
    {
        var body = EmailCampaignLimits.AppendFooter(
            "Promo especial",
            "Los Prados",
            "https://clinic.example/agendar-campana?token=xyz");

        body.Should().Contain("https://clinic.example/agendar-campana?token=xyz");
        body.Should().Contain("cualquier día y hora");
    }

    [Fact]
    public void AppendFooter_DoesNotDuplicateBookingLink()
    {
        var link = "https://clinic.example/agendar-campana?token=xyz";
        var body = EmailCampaignLimits.AppendFooter($"Use {link}", "Los Prados", link);
        body.Should().Contain(link);
        body.Split(link, StringSplitOptions.None).Length.Should().Be(2);
    }

    [Fact]
    public void BuildBookingUrl_ReturnsNull_WhenBaseMissing()
    {
        EmailCampaignLimits.BuildBookingUrl(null, "token").Should().BeNull();
        EmailCampaignLimits.BuildBookingUrl("  ", "token").Should().BeNull();
    }

    [Fact]
    public void BuildBookingUrl_JoinsBaseAndToken()
    {
        var url = EmailCampaignLimits.BuildBookingUrl("https://clinic.example/", "a b");
        url.Should().Be("https://clinic.example/agendar-campana?token=a%20b");
    }

    [Fact]
    public void EstimatedBatches_RoundsUp()
    {
        var eligible = 16;
        var batches = (int)Math.Ceiling(eligible / (double)EmailCampaignLimits.BatchSize);
        batches.Should().Be(2);
    }
}
