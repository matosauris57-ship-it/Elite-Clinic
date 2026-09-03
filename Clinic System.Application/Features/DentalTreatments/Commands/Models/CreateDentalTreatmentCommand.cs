namespace Clinic_System.Application.Features.DentalTreatments.Commands.Models
{
    public class CreateDentalTreatmentCommand : IRequest<Response<DentalTreatmentDTO>>
    {
        public int PatientId { get; set; }
        public string ProcedureName { get; set; } = null!;
        public int? TreatmentProcedureId { get; set; }
        public decimal Cost { get; set; }
        public int? AppointmentId { get; set; }
        public int? ToothNumber { get; set; }
        public ToothSurface? ToothSurface { get; set; }
        public string? ProcedureDetails { get; set; }
    }
}
