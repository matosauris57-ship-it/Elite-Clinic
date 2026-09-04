namespace Clinic_System.Application.Features.DentalTreatments.Commands.Models
{
    public class UpdateDentalTreatmentCommand : IRequest<Response<DentalTreatmentDTO>>
    {
        public int Id { get; set; }
        public string ProcedureName { get; set; } = null!;
        public int? TreatmentProcedureId { get; set; }
        public decimal Cost { get; set; }
        public int? ToothNumber { get; set; }
        public ToothSurface? ToothSurface { get; set; }
        public string? ProcedureDetails { get; set; }
        public string? MedicalNotes { get; set; }
    }
}
