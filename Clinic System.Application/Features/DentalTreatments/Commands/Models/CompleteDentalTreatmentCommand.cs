using Clinic_System.Application.DTOs.Inventory;

namespace Clinic_System.Application.Features.DentalTreatments.Commands.Models
{
    public class CompleteDentalTreatmentCommand : IRequest<Response<DentalTreatmentDTO>>
    {
        [JsonIgnore]
        public int TreatmentId { get; set; }
        public DentalTreatmentClinicalResultInput? ClinicalResult { get; set; }
        /// <summary>
        /// Consumo de materiales a confirmar. Null = aplicar BOM por defecto si existe.
        /// Lista vacía = completar sin descontar inventario.
        /// </summary>
        public List<MaterialConsumptionLineInput>? MaterialConsumption { get; set; }
        public bool SkipMaterialConsumption { get; set; }
        public string? MaterialConsumptionNotes { get; set; }
        public bool AllowInsufficientStock { get; set; }
    }

    public class DentalTreatmentClinicalResultInput
    {
        public ToothSurface Surface { get; set; }
        public ToothCondition Condition { get; set; }
        public ToothSeverity? Severity { get; set; }
        public string? Notes { get; set; }
    }
}
