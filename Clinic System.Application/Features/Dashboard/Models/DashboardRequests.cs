using Clinic_System.Application.DTOs.Dashboard;
using Clinic_System.Core.Dashboard;

namespace Clinic_System.Application.Features.Dashboard.Models;

public class GetDashboardLayoutQuery : IRequest<Response<DashboardLayoutResponseDTO>>;

public class SaveDashboardLayoutCommand : IRequest<Response<DashboardLayoutResponseDTO>>
{
    public DashboardLayoutDocument Layout { get; set; } = new();
}

public class RestoreDashboardLayoutCommand : IRequest<Response<DashboardLayoutResponseDTO>>;

public class GetClinicDashboardConfigQuery : IRequest<Response<DashboardClinicConfigDTO>>;

public class SaveClinicDashboardConfigCommand : IRequest<Response<DashboardClinicConfigDTO>>
{
    public DashboardLayoutDocument Layout { get; set; } = new();
}

public class RestoreClinicDashboardConfigCommand : IRequest<Response<DashboardClinicConfigDTO>>;

public class GetPatientDashboardStatsQuery : IRequest<Response<PatientDashboardStatsDTO>>;

public class GetRecentClinicalActivityQuery : IRequest<Response<List<RecentClinicalActivityItemDTO>>>
{
    public string Period { get; set; } = "7d";
    public int Take { get; set; } = 10;
}

public class GetPeriodontalIncompleteStatsQuery : IRequest<Response<PeriodontalIncompleteStatsDTO>>;
