using Clinic_System.Core.Reports;

namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IClinicReportRepository
{
    Task<ClinicReportSnapshot> GetSnapshotAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}
