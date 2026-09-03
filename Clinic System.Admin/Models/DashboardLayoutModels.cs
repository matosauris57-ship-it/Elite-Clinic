using Clinic_System.Core.Dashboard;

namespace DentalCare.Admin.Models;


public class DashboardLayoutResponse
{
    public DashboardLayoutDocument Layout { get; set; } = new();
    public List<string> ClinicEnabledKeys { get; set; } = [];
    public bool IsUserLayout { get; set; }
}

public class DashboardClinicConfigResponse
{
    public DashboardLayoutDocument Layout { get; set; } = new();
    public List<DashboardCatalogItem> Widgets { get; set; } = [];
}

public class DashboardCatalogItem
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool Enabled { get; set; }
    public bool AllowedForUser { get; set; }
}

public class PatientDashboardStats
{
    public int TotalPatients { get; set; }
    public int NewThisMonth { get; set; }
    public int NewLastMonth { get; set; }
}

public class RecentClinicalActivityItem
{
    public long Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}

public class PeriodontalIncompleteStats
{
    public int IncompleteExams { get; set; }
}

public class TreatmentMixSlice
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Percent { get; set; }
}

public class GridNodeChange
{
    public string Id { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
}
