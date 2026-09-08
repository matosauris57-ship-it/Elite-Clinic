using Clinic_System.Application.Common;

namespace Clinic_System.Application.Tests.Common;

public class LowStockEmailAlertSettingsTests
{
    [Fact]
    public void Normalize_ParsesDistinctRecipientsFromMixedSeparators()
    {
        var settings = new LowStockEmailAlertSettings
        {
            Recipients = [" a@clinica.com; b@clinica.com ", "b@clinica.com\nc@clinica.com"]
        };

        var normalized = settings.Normalize();

        normalized.Recipients.Should().Equal("a@clinica.com", "b@clinica.com", "c@clinica.com");
    }

    [Fact]
    public void Validate_RequiresRecipientsWhenEnabled()
    {
        var error = LowStockEmailAlertSettings.Validate(new LowStockEmailAlertSettings
        {
            Enabled = true,
            Recipients = []
        });

        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ShouldSend_RespectsEnabledTimeAndOncePerDay()
    {
        var settings = new LowStockEmailAlertSettings
        {
            Enabled = true,
            Recipients = ["stock@clinica.com"],
            SendTime = new TimeSpan(8, 0, 0),
            LastSentDate = null
        }.Normalize();

        settings.ShouldSend(new DateTime(2026, 9, 8, 7, 59, 0)).Should().BeFalse();
        settings.ShouldSend(new DateTime(2026, 9, 8, 8, 0, 0)).Should().BeTrue();

        settings.LastSentDate = new DateOnly(2026, 9, 8);
        settings.ShouldSend(new DateTime(2026, 9, 8, 10, 0, 0)).Should().BeFalse();
        settings.ShouldSend(new DateTime(2026, 9, 9, 8, 0, 0)).Should().BeTrue();
    }
}
