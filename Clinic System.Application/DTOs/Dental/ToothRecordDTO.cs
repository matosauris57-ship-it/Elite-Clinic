namespace Clinic_System.Application.DTOs.Dental
{
    public class ToothRecordDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ToothNumber { get; set; }
        public string DiagnosisCondition { get; set; } = null!;
        public string? TreatmentCondition { get; set; }
        public string? Notes { get; set; }
    }
}
