namespace Clinic_System.Application.DTOs.Dashboard;

public class DashboardLayoutResponseDTO
{
    public DashboardLayoutDocument Layout { get; set; } = new();
    public List<string> ClinicEnabledKeys { get; set; } = [];
    public bool IsUserLayout { get; set; }
}

public class DashboardCatalogItemDTO
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool Enabled { get; set; }
    public bool AllowedForUser { get; set; }
}

public class DashboardClinicConfigDTO
{
    public DashboardLayoutDocument Layout { get; set; } = new();
    public List<DashboardCatalogItemDTO> Widgets { get; set; } = [];
}

public class PatientDashboardStatsDTO
{
    public int TotalPatients { get; set; }
    public int NewThisMonth { get; set; }
    public int NewLastMonth { get; set; }
}

public class RecentClinicalActivityItemDTO
{
    public long Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}

public class PeriodontalIncompleteStatsDTO
{
    public int IncompleteExams { get; set; }
}

public class AttendanceLinkAlertDTO
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Accepted { get; set; }
    public DateTime RespondedAt { get; set; }
    public string? Comment { get; set; }
    public string? ProcedureName { get; set; }
    public int? ToothNumber { get; set; }
}
