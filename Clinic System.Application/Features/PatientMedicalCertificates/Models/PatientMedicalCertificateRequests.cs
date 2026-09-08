namespace Clinic_System.Application.Features.PatientMedicalCertificates.Models;

public class ListMedicalCertificateOptionsQuery : IRequest<Response<MedicalCertificateOptionsDTO>>
{
}

public class ListPatientMedicalCertificatesQuery : IRequest<Response<List<PatientMedicalCertificateSummaryDTO>>>
{
    public int PatientId { get; set; }
}

public class GetPatientMedicalCertificateQuery : IRequest<Response<PatientMedicalCertificateDTO>>
{
    public int CertificateId { get; set; }
}

public class CreatePatientMedicalCertificateCommand : IRequest<Response<PatientMedicalCertificateDTO>>
{
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string CertificateType { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public bool IncludesRest { get; set; }
    public DateTime? RestStartDate { get; set; }
    public DateTime? RestEndDate { get; set; }
    public int? RestDays { get; set; }
    public string? Observations { get; set; }
    public string? Purpose { get; set; }
}

public class UpdatePatientMedicalCertificateCommand : IRequest<Response<PatientMedicalCertificateDTO>>
{
    public int CertificateId { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string CertificateType { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public bool IncludesRest { get; set; }
    public DateTime? RestStartDate { get; set; }
    public DateTime? RestEndDate { get; set; }
    public int? RestDays { get; set; }
    public string? Observations { get; set; }
    public string? Purpose { get; set; }
}

public class DeletePatientMedicalCertificateCommand : IRequest<Response<string>>
{
    public int CertificateId { get; set; }
}

public class MedicalCertificateOptionsDTO
{
    public List<MedicalCertificateOptionDTO> CertificateTypes { get; set; } = [];
    public List<MedicalCertificateOptionDTO> Purposes { get; set; } = [];
    public List<string> DiagnosisSuggestions { get; set; } = [];
}
