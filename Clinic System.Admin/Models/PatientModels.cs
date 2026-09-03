namespace DentalCare.Admin.Models;

public class UpdatePatientRequest
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool? OptOutEmailCampaigns { get; set; }
}

public class PatientClinicalProfile
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string DateOfBirthIso { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? MobilePhone { get; set; }
    public string? Email { get; set; }
    public bool OptOutEmailCampaigns { get; set; }
    public bool IsActive { get; set; } = true;
    public DentalHistoryForm? DentalHistory { get; set; }
}

public class DentalHistoryForm
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedication { get; set; }
    public string? SystemicDiseases { get; set; }
    public string? PreviousDentalTreatments { get; set; }
    public string? BloodPressure { get; set; }
    public string? OtherDiseases { get; set; }
    public string? ReasonForConsultation { get; set; }
    public string? Diagnosis { get; set; }
    public string? ClinicalObservations { get; set; }
    public bool HasBleedingGums { get; set; }
    public bool HasSensitiveTeeth { get; set; }
    public bool HasBruxism { get; set; }
    public bool IsSmoker { get; set; }
    public string? AdditionalNotes { get; set; }
    public List<int> SelectedConditionIds { get; set; } = [];
}

public class ToothRecordItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ToothNumber { get; set; }
    public string DiagnosisCondition { get; set; } = "Healthy";
    public string? TreatmentCondition { get; set; }
    public string? Notes { get; set; }
}

public class OdontogramToothUpdate
{
    public int ToothNumber { get; set; }
    public string DiagnosisCondition { get; set; } = "Healthy";
    public string? TreatmentCondition { get; set; }
    public string? Notes { get; set; }
}

public class BatchOdontogramRequest
{
    public int PatientId { get; set; }
    public List<OdontogramToothUpdate> Teeth { get; set; } = [];
}

public class EmergencyContactItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class EmergencyContactForm
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class OdontogramToothState
{
    public string DiagnosisCondition { get; set; } = "Healthy";
    public string? TreatmentCondition { get; set; }
}
