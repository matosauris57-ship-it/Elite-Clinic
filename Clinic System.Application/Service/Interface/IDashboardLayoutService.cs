using Clinic_System.Application.DTOs.Dashboard;
using Clinic_System.Core.Dashboard;

namespace Clinic_System.Application.Service.Interface;

public interface IDashboardLayoutService
{
    Task<DashboardLayoutResponseDTO> GetUserLayoutAsync(CancellationToken cancellationToken = default);
    Task<DashboardLayoutResponseDTO> SaveUserLayoutAsync(DashboardLayoutDocument layout, CancellationToken cancellationToken = default);
    Task<DashboardLayoutResponseDTO> RestoreUserLayoutAsync(CancellationToken cancellationToken = default);
    Task<DashboardClinicConfigDTO> GetClinicConfigAsync(CancellationToken cancellationToken = default);
    Task<DashboardClinicConfigDTO> SaveClinicConfigAsync(DashboardLayoutDocument layout, CancellationToken cancellationToken = default);
    Task<DashboardClinicConfigDTO> RestoreClinicConfigAsync(CancellationToken cancellationToken = default);
    Task<PatientDashboardStatsDTO> GetPatientStatsAsync(CancellationToken cancellationToken = default);
    Task<List<RecentClinicalActivityItemDTO>> GetRecentActivityAsync(DateTime since, int take, CancellationToken cancellationToken = default);
    Task<PeriodontalIncompleteStatsDTO> GetPeriodontalIncompleteAsync(CancellationToken cancellationToken = default);
}
