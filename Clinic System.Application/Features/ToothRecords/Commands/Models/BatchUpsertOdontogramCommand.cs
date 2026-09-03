namespace Clinic_System.Application.Features.ToothRecords.Commands.Models
{
    public class BatchUpsertOdontogramCommand : IRequest<Response<string>>
    {
        public int PatientId { get; set; }
        public List<OdontogramToothItem> Teeth { get; set; } = [];
    }

    public class OdontogramToothItem
    {
        public int ToothNumber { get; set; }
        public ToothCondition DiagnosisCondition { get; set; }
        public ToothCondition? TreatmentCondition { get; set; }
        public string? Notes { get; set; }
    }
}
