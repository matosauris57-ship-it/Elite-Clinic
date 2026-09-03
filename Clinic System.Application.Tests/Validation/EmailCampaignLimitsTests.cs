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
    public void AppendFooter_AddsClinicNotice()
    {
        var body = EmailCampaignLimits.AppendFooter("Hola", "Los Prados");
        body.Should().Contain("Los Prados");
        body.Should().Contain("pacientes de la clínica");
    }

    [Fact]
    public void EstimatedBatches_RoundsUp()
    {
        var eligible = 16;
        var batches = (int)Math.Ceiling(eligible / (double)EmailCampaignLimits.BatchSize);
        batches.Should().Be(2);
    }
}
