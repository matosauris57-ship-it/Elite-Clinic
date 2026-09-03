using Clinic_System.Core.Odontogram;

namespace Clinic_System.Application.Service.Interface;

public interface IOdontogramSymbolConfigService
{
    Task<OdontogramSymbolConfigDocument> GetAsync(string? clinicKey = null, CancellationToken cancellationToken = default);

    Task<OdontogramSymbolConfigDocument> SaveAsync(
        OdontogramSymbolConfigDocument document,
        string? updatedBy,
        CancellationToken cancellationToken = default);

    Task<OdontogramSymbolConfigDocument> RestoreDefaultsAsync(
        string? clinicKey,
        string? updatedBy,
        CancellationToken cancellationToken = default);
}
