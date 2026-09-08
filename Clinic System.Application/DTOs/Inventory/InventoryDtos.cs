namespace Clinic_System.Application.DTOs.Inventory
{
    public class InventoryItemDTO
    {
        public int Id { get; set; }
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal QuantityOnHand { get; set; }
        public decimal MinimumStock { get; set; }
        public bool IsActive { get; set; }
        public bool IsLowStock { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StockMovementDTO
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public string InventoryItemName { get; set; } = null!;
        public string Type { get; set; } = null!;
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

    public class ProcedureMaterialDTO
    {
        public int Id { get; set; }
        public int TreatmentProcedureId { get; set; }
        public int InventoryItemId { get; set; }
        public string InventoryItemSku { get; set; } = null!;
        public string InventoryItemName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal DefaultQuantity { get; set; }
        public bool IsOptional { get; set; }
        public int SortOrder { get; set; }
        public decimal QuantityOnHand { get; set; }
    }

    public class MaterialConsumptionProposalDTO
    {
        public int DentalTreatmentId { get; set; }
        public int? TreatmentProcedureId { get; set; }
        public string ProcedureName { get; set; } = null!;
        public bool HasBom { get; set; }
        public bool AlreadyConsumed { get; set; }
        public List<MaterialConsumptionLineDTO> Lines { get; set; } = [];
    }

    public class MaterialConsumptionLineDTO
    {
        public int InventoryItemId { get; set; }
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public decimal ProposedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public bool IsIncluded { get; set; } = true;
        public bool IsOptional { get; set; }
        public decimal QuantityOnHand { get; set; }
        public bool InsufficientStock { get; set; }
    }

    public class TreatmentMaterialConsumptionDTO
    {
        public int Id { get; set; }
        public int DentalTreatmentId { get; set; }
        public string? Notes { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public List<MaterialConsumptionLineDTO> Lines { get; set; } = [];
    }

    public class LowStockAlertDTO
    {
        public int Count { get; set; }
        public List<InventoryItemDTO> Items { get; set; } = [];
    }

    public class MaterialConsumptionLineInput
    {
        public int InventoryItemId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsIncluded { get; set; } = true;
        public decimal? ProposedQuantity { get; set; }
    }

    public class ProcedureMaterialInput
    {
        public int InventoryItemId { get; set; }
        public decimal DefaultQuantity { get; set; }
        public bool IsOptional { get; set; }
        public int SortOrder { get; set; }
    }
}
