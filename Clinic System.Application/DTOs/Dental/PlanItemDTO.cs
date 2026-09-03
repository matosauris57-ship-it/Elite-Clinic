namespace Clinic_System.Application.DTOs.Dental
{
    public class PlanItemDTO
    {
        public int Id { get; set; }
        public string ProcedureName { get; set; } = null!;
        public int? TreatmentProcedureId { get; set; }
        public int? ToothNumber { get; set; }
        public ToothSurface? ToothSurface { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }
}
