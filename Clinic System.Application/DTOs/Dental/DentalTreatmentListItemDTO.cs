namespace Clinic_System.Application.DTOs.Dental
{
    public class DentalTreatmentListItemDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public int? AppointmentId { get; set; }
        public int? ToothNumber { get; set; }
        public ToothSurface? ToothSurface { get; set; }
        public int? TreatmentProcedureId { get; set; }
        public string ProcedureName { get; set; } = null!;
        public string? ProcedureDetails { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? PerformedDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
