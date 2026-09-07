namespace DentalCare.Admin.Models;

public class MedicalCertificateOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class MedicalCertificateOptions
{
    public List<MedicalCertificateOption> CertificateTypes { get; set; } = [];
    public List<MedicalCertificateOption> Purposes { get; set; } = [];
    public List<string> DiagnosisSuggestions { get; set; } = [];
}

public class PatientMedicalCertificateSummary
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime IssuedAt { get; set; }
    public string CertificateType { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public bool IncludesRest { get; set; }
    public DateTime? RestStartDate { get; set; }
    public DateTime? RestEndDate { get; set; }
    public int? RestDays { get; set; }
    public string? Purpose { get; set; }
}

public class PatientMedicalCertificateDetail : PatientMedicalCertificateSummary
{
    public string PatientName { get; set; } = string.Empty;
    public string? PatientNationalId { get; set; }
    public string? PatientPhone { get; set; }
    public DateTime? PatientDateOfBirth { get; set; }
    public string? DoctorSpecialization { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public string? Observations { get; set; }
    public string? RecordedByUserId { get; set; }
}

public class SavePatientMedicalCertificateRequest
{
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
