namespace DentalCare.Admin.Models;

public class PrescriptionTemplate
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Indication { get; set; } = string.Empty;
    public List<PatientPrescriptionItemModel> Lines { get; set; } = [];
}

public class PatientPrescriptionItemModel
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

public class PatientPrescriptionSummary
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

public class PatientPrescriptionDetail : PatientPrescriptionSummary
{
    public string PatientName { get; set; } = string.Empty;
    public string? PatientNationalId { get; set; }
    public string? PatientPhone { get; set; }
    public DateTime? PatientDateOfBirth { get; set; }
    public string? DoctorSpecialization { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedication { get; set; }
    public string? Notes { get; set; }
    public List<PatientPrescriptionItemModel> Items { get; set; } = [];
}

public class SavePatientPrescriptionRequest
{
    public int? DoctorId { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public List<string> TemplateKeys { get; set; } = [];
    public List<PatientPrescriptionItemModel> Items { get; set; } = [];
}
