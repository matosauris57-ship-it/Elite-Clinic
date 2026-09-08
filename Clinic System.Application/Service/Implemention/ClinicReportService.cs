using Clinic_System.Application.DTOs.Reports;
using Clinic_System.Application.Service.Interface;
using Clinic_System.Core.Interfaces.UnitOfWork;

namespace Clinic_System.Application.Service.Implemention;

public class ClinicReportService : IClinicReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClinicReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClinicReportsDTO> GetReportsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        if (toDate.Date < fromDate.Date)
            throw new ValidationException("La fecha hasta no puede ser anterior a la fecha desde.");

        var maxRange = fromDate.Date.AddYears(1);
        if (toDate.Date > maxRange)
            throw new ValidationException("El rango máximo de reportes es de 12 meses.");

        var snapshot = await _unitOfWork.ClinicReportsRepository
            .GetSnapshotAsync(fromDate.Date, toDate.Date, cancellationToken);

        return ClinicReportsDTO.FromSnapshot(snapshot);
    }
}
