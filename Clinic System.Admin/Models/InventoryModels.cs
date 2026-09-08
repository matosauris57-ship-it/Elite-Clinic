namespace DentalCare.Admin.Models;

public class InventoryItemListItem
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = "u";
    public decimal QuantityOnHand { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInventoryItemRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "CONSUMIBLES";
    public string Unit { get; set; } = "u";
    public decimal MinimumStock { get; set; }
    public decimal InitialQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateInventoryItemRequest
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = "u";
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
}

public class StockEntryRequest
{
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class StockAdjustmentRequest
{
    public decimal NewQuantityOnHand { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class StockMovementListItem
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public string InventoryItemName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public decimal SignedDelta { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProcedureMaterialItem
{
    public int Id { get; set; }
    public int TreatmentProcedureId { get; set; }
    public int InventoryItemId { get; set; }
    public string InventoryItemSku { get; set; } = string.Empty;
    public string InventoryItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = "u";
    public decimal DefaultQuantity { get; set; }
    public bool IsOptional { get; set; }
    public int SortOrder { get; set; }
    public decimal QuantityOnHand { get; set; }
}

public class ProcedureMaterialRequest
{
    public int InventoryItemId { get; set; }
    public decimal DefaultQuantity { get; set; }
    public bool IsOptional { get; set; }
    public int SortOrder { get; set; }
}

public class ReplaceProcedureBomRequest
{
    public List<ProcedureMaterialRequest> Materials { get; set; } = [];
}

public class MaterialConsumptionLineItem
{
    public int InventoryItemId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "u";
    public decimal ProposedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public bool IsIncluded { get; set; } = true;
    public bool IsOptional { get; set; }
    public decimal QuantityOnHand { get; set; }
    public bool InsufficientStock { get; set; }
}

public class MaterialConsumptionProposal
{
    public int DentalTreatmentId { get; set; }
    public int? TreatmentProcedureId { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public bool HasBom { get; set; }
    public bool AlreadyConsumed { get; set; }
    public List<MaterialConsumptionLineItem> Lines { get; set; } = [];
}

public class MaterialConsumptionLineRequest
{
    public int InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsIncluded { get; set; } = true;
    public decimal? ProposedQuantity { get; set; }
}

public class CompleteDentalTreatmentRequest
{
    public DentalTreatmentClinicalResultRequest? ClinicalResult { get; set; }
    public List<MaterialConsumptionLineRequest>? MaterialConsumption { get; set; }
    public bool SkipMaterialConsumption { get; set; }
    public string? MaterialConsumptionNotes { get; set; }
    public bool AllowInsufficientStock { get; set; }
}

public class LowStockAlert
{
    public int Count { get; set; }
    public List<InventoryItemListItem> Items { get; set; } = [];
}
