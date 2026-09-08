namespace DentalCare.Admin.Models;

public class CampaignBookingContext
{
    public string PatientName { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public List<CampaignBookingDoctor> Doctors { get; set; } = [];
    public bool FlexibleSchedule { get; set; } = true;
    public string? Note { get; set; }
}

public class CampaignBookingDoctor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
}

public class CampaignBookFormRequest
{
    public string Token { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
}

public class CampaignBookingResult
{
    public int AppointmentId { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
