namespace Clinic_System.Core.Entities;

public class CampaignAppointmentRequest
{
    public int Id { get; set; }
    public int EmailCampaignId { get; set; }
    public int EmailCampaignRecipientId { get; set; }
    public int PatientId { get; set; }
    public int RequestedDoctorId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime RequestedAppointmentAt { get; set; }
    public int? TreatmentProcedureId { get; set; }
    public string? OtherServiceText { get; set; }
    public CampaignAppointmentRequestStatus Status { get; set; } = CampaignAppointmentRequestStatus.Pending;
    public int? AppointmentId { get; set; }
    public int? ScheduledDoctorId { get; set; }
    public DateTime? ScheduledAppointmentAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
    public string? ResolvedByName { get; set; }
    public string? StaffNote { get; set; }
    public bool PatientNotified { get; set; }

    public virtual EmailCampaign EmailCampaign { get; set; } = null!;
    public virtual EmailCampaignRecipient EmailCampaignRecipient { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor RequestedDoctor { get; set; } = null!;
    public virtual Doctor? ScheduledDoctor { get; set; }
    public virtual TreatmentProcedure? TreatmentProcedure { get; set; }
    public virtual Appointment? Appointment { get; set; }
}
