using Clinic_System.Application.Common;

namespace Clinic_System.Application.Tests.Validation;

public class PatientNotificationSettingsTests
{
    [Fact]
    public void DayBefore_Sends_AfterConfiguredHour_TheDayBefore()
    {
        var settings = new PatientNotificationSettings
        {
            DayBeforeEnabled = true,
            SameDayEnabled = false,
            DayBeforeSendTime = new TimeSpan(9, 0, 0)
        };
        var appointment = new DateTime(2026, 9, 4, 15, 0, 0);

        settings.ShouldSendDayBefore(new DateTime(2026, 9, 3, 8, 59, 0), appointment).Should().BeFalse();
        settings.ShouldSendDayBefore(new DateTime(2026, 9, 3, 9, 0, 0), appointment).Should().BeTrue();
        settings.ShouldSendDayBefore(new DateTime(2026, 9, 4, 9, 0, 0), appointment).Should().BeFalse();
    }

    [Fact]
    public void SameDay_Sends_AtHour_BeforeAppointment()
    {
        var settings = new PatientNotificationSettings
        {
            DayBeforeEnabled = false,
            SameDayEnabled = true,
            SameDaySendTime = new TimeSpan(8, 0, 0)
        };
        var appointment = new DateTime(2026, 9, 3, 15, 0, 0);

        settings.ShouldSendSameDay(new DateTime(2026, 9, 3, 7, 59, 0), appointment).Should().BeFalse();
        settings.ShouldSendSameDay(new DateTime(2026, 9, 3, 8, 0, 0), appointment).Should().BeTrue();
        settings.ShouldSendSameDay(new DateTime(2026, 9, 3, 16, 0, 0), appointment).Should().BeFalse();
    }

    [Fact]
    public void Both_CanBeEnabledIndependently()
    {
        var settings = new PatientNotificationSettings
        {
            DayBeforeEnabled = true,
            SameDayEnabled = true,
            DayBeforeSendTime = new TimeSpan(9, 0, 0),
            SameDaySendTime = new TimeSpan(8, 0, 0)
        };
        var appointment = new DateTime(2026, 9, 4, 11, 0, 0);

        settings.ShouldSendDayBefore(new DateTime(2026, 9, 3, 9, 0, 0), appointment).Should().BeTrue();
        settings.ShouldSendSameDay(new DateTime(2026, 9, 4, 8, 0, 0), appointment).Should().BeTrue();
    }

    [Fact]
    public void Birthday_OncePerYear_AfterHour()
    {
        var settings = new PatientNotificationSettings
        {
            BirthdayEnabled = true,
            BirthdaySendTime = new TimeSpan(8, 0, 0)
        };
        var dob = new DateTime(1990, 9, 3);

        settings.ShouldSendBirthday(new DateTime(2026, 9, 3, 7, 0, 0), dob, null).Should().BeFalse();
        settings.ShouldSendBirthday(new DateTime(2026, 9, 3, 8, 0, 0), dob, null).Should().BeTrue();
        settings.ShouldSendBirthday(new DateTime(2026, 9, 3, 8, 0, 0), dob, 2026).Should().BeFalse();
        settings.ShouldSendBirthday(new DateTime(2026, 9, 4, 8, 0, 0), dob, null).Should().BeFalse();
    }

    [Fact]
    public void Feb29_UsesFeb28_InNonLeapYear()
    {
        PatientNotificationSettings.IsBirthdayOn(new DateTime(2026, 2, 28), new DateTime(2000, 2, 29)).Should().BeTrue();
        PatientNotificationSettings.IsBirthdayOn(new DateTime(2028, 2, 28), new DateTime(2000, 2, 29)).Should().BeFalse();
        PatientNotificationSettings.IsBirthdayOn(new DateTime(2028, 2, 29), new DateTime(2000, 2, 29)).Should().BeTrue();
    }
}
