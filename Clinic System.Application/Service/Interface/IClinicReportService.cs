using Clinic_System.Application.DTOs.Reports;

namespace Clinic_System.Application.Service.Interface;

public interface IClinicReportService
{
    Task<ClinicReportsDTO> GetReportsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
