namespace Clinic_System.Application.Features.DentalHistory.Commands.Models
{
    public class UpsertDentalHistoryCommand : IRequest<Response<DentalHistoryDTO>>
    {
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
}
