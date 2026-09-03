using Clinic_System.Application.Common;

namespace Clinic_System.Application.Service.Interface
{
    public interface IClinicOperatingHoursService
    {
        Task<ClinicOperatingHours> GetAsync(CancellationToken cancellationToken = default);
        Task SaveAsync(ClinicOperatingHours hours, CancellationToken cancellationToken = default);
    }
}
