using Clinic_System.Core.Enums;

namespace DentalCare.Admin.Models;

public class TreatmentProcedureListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PricingMode { get; set; } = "AtBooking";
    public string PriceDisplay { get; set; } = string.Empty;
    public string PriceRaw { get; set; } = string.Empty;
    public string PriceRangeDisplay { get; set; } = string.Empty;
    public string PricingModeDisplay { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public TreatmentProcedureTarget Target { get; set; } = TreatmentProcedureTarget.PerTooth;
    public TreatmentToothKindFilter ToothKindFilter { get; set; } = TreatmentToothKindFilter.Any;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DoctorProcedurePriceItem> DoctorPrices { get; set; } = [];

    public bool IsFixedPrice =>
        string.Equals(PricingMode, "Fixed", StringComparison.OrdinalIgnoreCase);

    public bool RequiresQuoteAtBooking =>
        string.Equals(PricingMode, "AtBooking", StringComparison.OrdinalIgnoreCase)
        || string.IsNullOrWhiteSpace(PricingMode);

    public bool RequiresPriceAtBilling =>
        string.Equals(PricingMode, "AtBilling", StringComparison.OrdinalIgnoreCase);

    public decimal ResolvePrice(int? doctorId)
    {
        if (!IsFixedPrice)
            return 0;

        if (doctorId.HasValue)
        {
            var match = DoctorPrices.FirstOrDefault(p => p.DoctorId == doctorId.Value);
            if (match != null)
                return match.Price;
        }

        return Price;
    }
}

public class DoctorProcedurePriceItem
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceDisplay { get; set; } = string.Empty;
    public string PriceRaw { get; set; } = string.Empty;
}

public class DoctorProcedurePriceRequest
{
    public int DoctorId { get; set; }
    public decimal Price { get; set; }
}

public class CreateTreatmentProcedureRequest
{
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PricingMode { get; set; } = "AtBooking";
    public string PriceInput { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public TreatmentProcedureTarget Target { get; set; } = TreatmentProcedureTarget.PerTooth;
    public TreatmentToothKindFilter ToothKindFilter { get; set; } = TreatmentToothKindFilter.Any;
    public List<DoctorProcedurePriceRequest> DoctorPrices { get; set; } = [];
}

public class UpdateTreatmentProcedureRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PricingMode { get; set; } = "AtBooking";
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public TreatmentProcedureTarget Target { get; set; } = TreatmentProcedureTarget.PerTooth;
    public TreatmentToothKindFilter ToothKindFilter { get; set; } = TreatmentToothKindFilter.Any;
    public List<DoctorProcedurePriceRequest> DoctorPrices { get; set; } = [];
}

public class DentalTreatmentListItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }
    public int? ToothNumber { get; set; }
    public ToothSurface? ToothSurface { get; set; }
    public int? TreatmentProcedureId { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? ProcedureDetails { get; set; }
    public string? MedicalNotes { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PerformedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DentalTreatmentsAdminPage
{
    public List<DentalTreatmentListItem> Items { get; set; } = [];
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; } = 1;
    public int PlannedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }

    public int ActiveCount => PlannedCount + InProgressCount;
    public int AllCount => PlannedCount + InProgressCount + CompletedCount + CancelledCount;
}

public class CreateDentalTreatmentRequest
{
    public int PatientId { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public int? TreatmentProcedureId { get; set; }
    public decimal Cost { get; set; }
    public int? AppointmentId { get; set; }
    public int? ToothNumber { get; set; }
    public List<int> ToothNumbers { get; set; } = [];
    public ToothSurface? ToothSurface { get; set; }
    public string? ProcedureDetails { get; set; }
    public string? MedicalNotes { get; set; }
}

public class UpdateDentalTreatmentRequest
{
    public int Id { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int? ToothNumber { get; set; }
    public ToothSurface? ToothSurface { get; set; }
    public int? TreatmentProcedureId { get; set; }
    public string? ProcedureDetails { get; set; }
    public string? MedicalNotes { get; set; }
}

public class CancelDentalTreatmentRequest
{
    public string? Reason { get; set; }
}

public class DentalTreatmentClinicalResultRequest
{
    public ToothSurface Surface { get; set; }
    public ToothCondition Condition { get; set; }
    public ToothSeverity? Severity { get; set; }
    public string? Notes { get; set; }
}

public enum QuotePanelMode
{
    List,
    Compose
}

public class TreatmentPlanListItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public string? PatientNationalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string TotalAmountDisplay { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public string DiscountAmountDisplay { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public string FinalAmountDisplay { get; set; } = string.Empty;
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedByName { get; set; }
    public string? RejectionReason { get; set; }
    public int? InvoicePaymentId { get; set; }
    public List<int> InvoicePaymentIds { get; set; } = [];
    public bool CanInvoice { get; set; }
    public decimal AmountBilled { get; set; }
    public string AmountBilledDisplay { get; set; } = string.Empty;
    public decimal AmountCollectedOnAccount { get; set; }
    public string AmountCollectedOnAccountDisplay { get; set; } = string.Empty;
    public decimal RemainingToBill { get; set; }
    public string RemainingToBillDisplay { get; set; } = string.Empty;
    public decimal RemainingToCollect { get; set; }
    public string RemainingToCollectDisplay { get; set; } = string.Empty;
    public decimal UnbilledCompletedAmount { get; set; }
    public string UnbilledCompletedAmountDisplay { get; set; } = string.Empty;
    public List<TreatmentPlanItem> Items { get; set; } = [];
}

public class TreatmentPlanItem
{
    public int Id { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public int? TreatmentProcedureId { get; set; }
    public int? ToothNumber { get; set; }
    public Clinic_System.Core.Enums.ToothSurface? ToothSurface { get; set; }
    public int Quantity { get; set; } = 1;
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

public class CreateTreatmentPlanRequest
{
    public int PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<CreateTreatmentPlanItemRequest> Items { get; set; } = [];
}

public class CreateTreatmentPlanItemRequest
{
    public string ProcedureName { get; set; } = string.Empty;
    public int? TreatmentProcedureId { get; set; }
    public int? ToothNumber { get; set; }
    public Clinic_System.Core.Enums.ToothSurface? ToothSurface { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string UnitPriceInput { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ApproveTreatmentPlanRequest
{
    public string? AcceptedByName { get; set; }
}

public class RejectTreatmentPlanRequest
{
    public string? Reason { get; set; }
}
