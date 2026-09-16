using Clinic_System.Core.Entities;
using Clinic_System.Core.Enums;
using Clinic_System.Core.Exceptions;
using FluentAssertions;

namespace Clinic_System.Application.Tests.Features.AppointmentsTests;

public class AppointmentAttendanceLinkTests
{
    [Fact]
    public void RespondViaAttendanceLink_Accept_ConfirmsAndStoresComment()
    {
        var appointment = PendingAppointment();

        appointment.RespondViaAttendanceLink(true, "  Llegaré 10 minutos tarde.  ");

        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.AttendanceLinkAccepted.Should().BeTrue();
        appointment.AttendanceLinkRespondedAt.Should().NotBeNull();
        appointment.AttendanceLinkComment.Should().Be("Llegaré 10 minutos tarde.");
        appointment.CancellationChannel.Should().BeNull();
    }

    [Fact]
    public void RespondViaAttendanceLink_Decline_CancelsAsPatientWithComment()
    {
        var appointment = PendingAppointment();

        appointment.RespondViaAttendanceLink(false, "No puedo asistir por trabajo");

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.AttendanceLinkAccepted.Should().BeFalse();
        appointment.CancellationChannel.Should().Be(AppointmentCancellationChannel.Patient);
        appointment.CancellationComment.Should().Be("No puedo asistir por trabajo");
        appointment.AttendanceLinkComment.Should().Be("No puedo asistir por trabajo");
        appointment.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void RespondViaAttendanceLink_SecondResponse_Throws()
    {
        var appointment = PendingAppointment();
        appointment.RespondViaAttendanceLink(true, null);

        var act = () => appointment.RespondViaAttendanceLink(false, "cambio de opinión");

        act.Should().Throw<InvalidAppointmentStateException>();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    private static Appointment PendingAppointment() => new()
    {
        Id = 1,
        PatientId = 10,
        DoctorId = 3,
        AppointmentDate = DateTime.Today.AddDays(1).AddHours(10),
        Status = AppointmentStatus.Pending
    };
}
