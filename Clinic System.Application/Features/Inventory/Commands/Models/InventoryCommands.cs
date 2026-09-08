using Clinic_System.Application.DTOs.Inventory;

namespace Clinic_System.Application.Features.Inventory.Commands.Models
{
    public class CreateInventoryItemCommand : IRequest<Response<InventoryItemDTO>>
    {
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Unit { get; set; } = "u";
        public decimal MinimumStock { get; set; }
        public decimal InitialQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateInventoryItemCommand : IRequest<Response<InventoryItemDTO>>
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Unit { get; set; } = "u";
        public decimal MinimumStock { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SoftDeleteInventoryItemCommand : IRequest<Response<string>>
    {
        public int Id { get; set; }
    }

    public class RegisterStockEntryCommand : IRequest<Response<StockMovementDTO>>
    {
        [JsonIgnore]
        public int InventoryItemId { get; set; }
        public decimal Quantity { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class RegisterStockAdjustmentCommand : IRequest<Response<StockMovementDTO>>
    {
        [JsonIgnore]
        public int InventoryItemId { get; set; }
        public decimal NewQuantityOnHand { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class ReplaceProcedureBomCommand : IRequest<Response<List<ProcedureMaterialDTO>>>
    {
        [JsonIgnore]
        public int TreatmentProcedureId { get; set; }
        public List<ProcedureMaterialInput> Materials { get; set; } = [];
    }

    public class ConfirmMaterialConsumptionCommand : IRequest<Response<TreatmentMaterialConsumptionDTO>>
    {
        [JsonIgnore]
        public int DentalTreatmentId { get; set; }
        public List<MaterialConsumptionLineInput>? Lines { get; set; }
        public string? Notes { get; set; }
        public bool AllowInsufficientStock { get; set; }
    }

    public class ReplaceMaterialConsumptionCommand : IRequest<Response<TreatmentMaterialConsumptionDTO>>
    {
        [JsonIgnore]
        public int DentalTreatmentId { get; set; }
        public List<MaterialConsumptionLineInput> Lines { get; set; } = [];
        public string? Notes { get; set; }
        public bool AllowInsufficientStock { get; set; }
    }
}
