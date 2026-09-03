namespace Clinic_System.Application.DTOs.Dental;

public class PrescriptionTemplateDTO
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Indication { get; set; } = string.Empty;
    public List<PatientPrescriptionItemDTO> Lines { get; set; } = [];
}

public class PatientPrescriptionItemDTO
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string? TemplateKey { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; } = 7;
    public string? SpecialInstructions { get; set; }
}

public class PatientPrescriptionSummaryDTO
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime IssuedAt { get; set; }
    public string? Diagnosis { get; set; }
    public int ItemCount { get; set; }
    public string MedicationPreview { get; set; } = string.Empty;
}

public class PatientPrescriptionDTO : PatientPrescriptionSummaryDTO
{
    public string PatientName { get; set; } = string.Empty;
    public string? PatientNationalId { get; set; }
    public string? PatientPhone { get; set; }
    public DateTime? PatientDateOfBirth { get; set; }
    public string? DoctorSpecialization { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedication { get; set; }
    public string? Notes { get; set; }
    public List<PatientPrescriptionItemDTO> Items { get; set; } = [];
}

public class PatientPrescriptionUpsertDTO
{
    public int? DoctorId { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public List<string> TemplateKeys { get; set; } = [];
    public List<PatientPrescriptionItemDTO> Items { get; set; } = [];
}
