namespace Clinic_System.Application.Features.ToothRecords.Commands.Models
{
    public class UpsertToothRecordCommand : IRequest<Response<ToothRecordDTO>>
    {
        public int PatientId { get; set; }
        public int ToothNumber { get; set; }
        public ToothCondition DiagnosisCondition { get; set; }
        public ToothCondition? TreatmentCondition { get; set; }
        public string? Notes { get; set; }
    }
}
