namespace DentalCare.Admin.Models;

public class TreatmentProcedureListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceDisplay { get; set; } = string.Empty;
    public string PriceRaw { get; set; } = string.Empty;
    public string PriceRangeDisplay { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DoctorProcedurePriceItem> DoctorPrices { get; set; } = [];

    public decimal ResolvePrice(int? doctorId)
    {
        if (doctorId.HasValue)
        {
            var match = DoctorPrices.FirstOrDefault(p => p.DoctorId == doctorId.Value);
            if (match != null)
                return match.Price;
            return 0;
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
    public int DurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public List<DoctorProcedurePriceRequest> DoctorPrices { get; set; } = [];
}

public class UpdateTreatmentProcedureRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
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

public class CompleteDentalTreatmentRequest
{
    public DentalTreatmentClinicalResultRequest? ClinicalResult { get; set; }
}

public class DentalTreatmentClinicalResultRequest
{
    public ToothSurface Surface { get; set; }
    public ToothCondition Condition { get; set; }
    public ToothSeverity? Severity { get; set; }
    public string? Notes { get; set; }
}

public class TreatmentPlanListItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime? ValidUntil { get; set; }
    public List<TreatmentPlanItem> Items { get; set; } = [];
}

public class TreatmentPlanItem
{
    public int Id { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public int? TreatmentProcedureId { get; set; }
    public int? ToothNumber { get; set; }
    public ToothSurface? ToothSurface { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
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
    public ToothSurface? ToothSurface { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public class RejectTreatmentPlanRequest
{
    public string? Reason { get; set; }
}
