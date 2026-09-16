namespace DentalCare.Admin.Models;

public class CampaignBookingContext
{
    public string PatientName { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public List<CampaignBookingDoctor> Doctors { get; set; } = [];
    public List<CampaignBookingServiceOption> Services { get; set; } = [];
    public bool FlexibleSchedule { get; set; } = true;
    public bool AlreadySubmitted { get; set; }
    public CampaignBookingExistingRequest? ExistingRequest { get; set; }
    public string? Note { get; set; }
}

public class CampaignBookingDoctor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
}

public class CampaignBookingServiceOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int DurationMinutes { get; set; }
}

public class CampaignBookingExistingRequest
{
    public int RequestId { get; set; }
    public int? AppointmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime? RequestedAppointmentAt { get; set; }
    public DateTime? ScheduledAppointmentAt { get; set; }
    public string? ServiceName { get; set; }
    public bool AlreadySubmitted { get; set; } = true;
}

public class CampaignBookFormRequest
{
    public string Token { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public int? TreatmentProcedureId { get; set; }
    public string? ServiceKey { get; set; }
    public string? OtherServiceText { get; set; }
}

public class CampaignBookingResult
{
    public int? RequestId { get; set; }
    public int? AppointmentId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? ServiceName { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AlreadySubmitted { get; set; }
}

public class CampaignAppointmentRequestItem
{
    public int Id { get; set; }
    public string? CampaignName { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientPhone { get; set; }
    public string? PatientEmail { get; set; }
    public int RequestedDoctorId { get; set; }
    public string? RequestedDoctorName { get; set; }
    public DateTime RequestedAppointmentAt { get; set; }
    public string? ServiceName { get; set; }
    public bool IsOtherService { get; set; }
    public int? TreatmentProcedureId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }
    public int? ScheduledDoctorId { get; set; }
    public string? ScheduledDoctorName { get; set; }
    public DateTime? ScheduledAppointmentAt { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByName { get; set; }
    public string? StaffNote { get; set; }
    public bool PatientNotified { get; set; }
}

public class CampaignScheduleFormRequest
{
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string? StaffNote { get; set; }
}

public class CampaignBookingNotifySettingsModel
{
    public List<string> Recipients { get; set; } = [];
    public string RecipientsText
    {
        get => string.Join(Environment.NewLine, Recipients);
        set => Recipients = (value ?? string.Empty)
            .Split(['\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
