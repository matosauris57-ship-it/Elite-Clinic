namespace Clinic_System.Application.Service.Interface
{
    public interface ITreatmentPlanService
    {
        Task<TreatmentPlan> CreateAsync(int patientId, string title, string? notes, DateTime? validUntil, decimal discountAmount, List<PlanItemInput> items, string? recordedByUserId, CancellationToken cancellationToken = default);
        Task<TreatmentPlan> ApproveAsync(int planId, string? recordedByUserId, CancellationToken cancellationToken = default);
        Task<TreatmentPlan> RejectAsync(int planId, string? reason, string? recordedByUserId, CancellationToken cancellationToken = default);
        Task<TreatmentPlan> CompleteAsync(int planId, string? recordedByUserId, CancellationToken cancellationToken = default);
        Task<TreatmentPlan?> GetByIdAsync(int planId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TreatmentPlan>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    }

    public class PlanItemInput
    {
        public string ProcedureName { get; set; } = null!;
        public int? TreatmentProcedureId { get; set; }
        public int? ToothNumber { get; set; }
        public ToothSurface? ToothSurface { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }
}
