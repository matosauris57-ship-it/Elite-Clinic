namespace Clinic_System.Application.DTOs.Dental
{
    public class DentalTreatmentDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public int? ToothNumber { get; set; }
        public ToothSurface? ToothSurface { get; set; }
        public string ProcedureName { get; set; } = null!;
        public int? TreatmentProcedureId { get; set; }
        public string? ProcedureDetails { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? PerformedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
