using Clinic_System.Core.Catalog;

namespace Clinic_System.Application.Service.Implemention;

public class PatientMedicalCertificateService : IPatientMedicalCertificateService
{
    private readonly IUnitOfWork unitOfWork;

    public PatientMedicalCertificateService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public IReadOnlyList<MedicalCertificateOptionDTO> ListCertificateTypes() =>
        MedicalCertificateCatalog.CertificateTypes.Select(ToOption).ToList();

    public IReadOnlyList<MedicalCertificateOptionDTO> ListPurposes() =>
        MedicalCertificateCatalog.Purposes.Select(ToOption).ToList();

    public IReadOnlyList<string> ListDiagnosisSuggestions() =>
        MedicalCertificateCatalog.DiagnosisSuggestions;

    public Task<IReadOnlyList<PatientMedicalCertificate>> ListByPatientAsync(
        int patientId,
        CancellationToken cancellationToken = default) =>
        unitOfWork.PatientMedicalCertificatesRepository.GetByPatientAsync(patientId, cancellationToken);

    public async Task<PatientMedicalCertificate> GetAsync(int certificateId, CancellationToken cancellationToken = default)
    {
        var certificate = await unitOfWork.PatientMedicalCertificatesRepository.GetWithDetailsAsync(certificateId, cancellationToken);
        return certificate ?? throw new NotFoundException($"Certificado {certificateId} no encontrado.");
    }

    public async Task<PatientMedicalCertificate> CreateAsync(
        int patientId,
        PatientMedicalCertificateUpsertDTO request,
        int? doctorId,
        string? recordedByUserId,
        CancellationToken cancellationToken = default)
    {
        var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken)
            ?? throw new NotFoundException($"Paciente {patientId} no encontrado.");

        var certificate = new PatientMedicalCertificate
        {
            PatientId = patient.Id,
            DoctorId = request.DoctorId ?? doctorId,
            IssuedAt = request.IssuedAt ?? DateTime.Now,
            RecordedByUserId = recordedByUserId
        };

        Apply(certificate, request);
        await unitOfWork.PatientMedicalCertificatesRepository.AddAsync(certificate, cancellationToken);
        await unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
        {
            PatientId = patientId,
            Type = DentalClinicalEventType.ClinicalNote,
            Title = $"Certificado emitido: {certificate.CertificateType}",
            Description = BuildEventDescription(certificate),
            ReferenceType = nameof(PatientMedicalCertificate),
            RecordedByUserId = recordedByUserId,
            RecordedAt = certificate.IssuedAt
        }, cancellationToken);

        return certificate;
    }

    public async Task<PatientMedicalCertificate> UpdateAsync(
        int certificateId,
        PatientMedicalCertificateUpsertDTO request,
        int? doctorId,
        string? recordedByUserId,
        CancellationToken cancellationToken = default)
    {
        var certificate = await GetAsync(certificateId, cancellationToken);
        certificate.DoctorId = request.DoctorId ?? doctorId ?? certificate.DoctorId;
        if (request.IssuedAt.HasValue)
            certificate.IssuedAt = request.IssuedAt.Value;
        certificate.RecordedByUserId = recordedByUserId ?? certificate.RecordedByUserId;
        Apply(certificate, request);

        unitOfWork.PatientMedicalCertificatesRepository.Update(certificate, cancellationToken);
        await unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
        {
            PatientId = certificate.PatientId,
            Type = DentalClinicalEventType.ClinicalNote,
            Title = $"Certificado actualizado: {certificate.CertificateType}",
            Description = BuildEventDescription(certificate),
            ReferenceType = nameof(PatientMedicalCertificate),
            ReferenceId = certificate.Id.ToString(),
            RecordedByUserId = recordedByUserId,
            RecordedAt = DateTime.Now
        }, cancellationToken);

        return certificate;
    }

    public async Task DeleteAsync(int certificateId, CancellationToken cancellationToken = default)
    {
        var certificate = await GetAsync(certificateId, cancellationToken);
        certificate.SoftDelete();
        unitOfWork.PatientMedicalCertificatesRepository.Update(certificate, cancellationToken);
    }

    public static PatientMedicalCertificateSummaryDTO MapSummary(PatientMedicalCertificate certificate) => new()
    {
        Id = certificate.Id,
        PatientId = certificate.PatientId,
        DoctorId = certificate.DoctorId,
        DoctorName = certificate.Doctor?.FullName,
        IssuedAt = certificate.IssuedAt,
        CertificateType = certificate.CertificateType,
        Diagnosis = certificate.Diagnosis,
        IncludesRest = certificate.IncludesRest,
        RestStartDate = certificate.RestStartDate,
        RestEndDate = certificate.RestEndDate,
        RestDays = certificate.RestDays,
        Purpose = certificate.Purpose
    };

    public static PatientMedicalCertificateDTO MapDetail(PatientMedicalCertificate certificate) => new()
    {
        Id = certificate.Id,
        PatientId = certificate.PatientId,
        PatientName = certificate.Patient?.FullName ?? string.Empty,
        PatientNationalId = certificate.Patient?.NationalId,
        PatientPhone = certificate.Patient?.Phone,
        PatientDateOfBirth = certificate.Patient?.DateOfBirth,
        DoctorId = certificate.DoctorId,
        DoctorName = certificate.Doctor?.FullName,
        DoctorSpecialization = certificate.Doctor?.Specialization,
        IssuedAt = certificate.IssuedAt,
        CertificateType = certificate.CertificateType,
        Diagnosis = certificate.Diagnosis,
        Recommendation = certificate.Recommendation,
        IncludesRest = certificate.IncludesRest,
        RestStartDate = certificate.RestStartDate,
        RestEndDate = certificate.RestEndDate,
        RestDays = certificate.RestDays,
        Observations = certificate.Observations,
        Purpose = certificate.Purpose,
        RecordedByUserId = certificate.RecordedByUserId
    };

    private static void Apply(PatientMedicalCertificate certificate, PatientMedicalCertificateUpsertDTO request)
    {
        certificate.CertificateType = TrimRequired(request.CertificateType, 120, "Tipo de certificado");
        certificate.Diagnosis = TrimRequired(request.Diagnosis, 1000, "Motivo o diagnóstico");
        certificate.Recommendation = TrimRequired(request.Recommendation, 2000, "Recomendación");
        certificate.IncludesRest = request.IncludesRest;
        certificate.Observations = Trim(request.Observations, 2000);
        certificate.Purpose = Trim(request.Purpose, 80);

        if (!request.IncludesRest)
        {
            certificate.RestStartDate = null;
            certificate.RestEndDate = null;
            certificate.RestDays = null;
            return;
        }

        if (!request.RestStartDate.HasValue || !request.RestEndDate.HasValue)
            throw new InvalidOperationException("Indique fecha de inicio y fin del reposo.");
        if (request.RestEndDate.Value.Date < request.RestStartDate.Value.Date)
            throw new InvalidOperationException("La fecha fin del reposo no puede ser anterior a la fecha inicio.");

        certificate.RestStartDate = request.RestStartDate.Value.Date;
        certificate.RestEndDate = request.RestEndDate.Value.Date;
        var calculatedDays = (certificate.RestEndDate.Value - certificate.RestStartDate.Value).Days + 1;
        certificate.RestDays = request.RestDays is > 0 ? request.RestDays : calculatedDays;
    }

    private static MedicalCertificateOptionDTO ToOption(string value) => new()
    {
        Value = value,
        Label = value
    };

    private static string BuildEventDescription(PatientMedicalCertificate certificate)
    {
        var parts = new List<string> { certificate.Diagnosis };
        if (certificate.IncludesRest && certificate.RestDays.HasValue)
            parts.Add($"Reposo por {certificate.RestDays} día(s)");
        if (!string.IsNullOrWhiteSpace(certificate.Purpose))
            parts.Add($"Finalidad: {certificate.Purpose}");
        return string.Join(" · ", parts);
    }

    private static string TrimRequired(string? value, int max, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{label} es requerido.");
        return Trim(value, max)!;
    }

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
