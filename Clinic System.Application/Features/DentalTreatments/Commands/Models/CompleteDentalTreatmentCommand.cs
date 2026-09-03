namespace Clinic_System.Application.Features.DentalTreatments.Commands.Models
{
    public class CompleteDentalTreatmentCommand : IRequest<Response<DentalTreatmentDTO>>
    {
        [JsonIgnore]
        public int TreatmentId { get; set; }
        public DentalTreatmentClinicalResultInput? ClinicalResult { get; set; }
    }

    public class DentalTreatmentClinicalResultInput
    {
        public ToothSurface Surface { get; set; }
        public ToothCondition Condition { get; set; }
        public ToothSeverity? Severity { get; set; }
        public string? Notes { get; set; }
    }
}
