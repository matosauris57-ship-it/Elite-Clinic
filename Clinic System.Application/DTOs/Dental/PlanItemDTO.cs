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
        public string UnitPriceDisplay { get; set; } = string.Empty;
        public string LineTotalDisplay { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string AcceptanceStatus { get; set; } = string.Empty;
        public string ExecutionStatus { get; set; } = string.Empty;
        public int? DentalTreatmentId { get; set; }
        public int? ScheduledAppointmentId { get; set; }
        public int? InvoicedPaymentId { get; set; }
        public bool CanAccept { get; set; }
        public bool CanReject { get; set; }
        public bool CanSchedule { get; set; }
        public bool CanInvoice { get; set; }
    }
}
