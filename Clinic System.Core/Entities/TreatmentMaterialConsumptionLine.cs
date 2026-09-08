namespace Clinic_System.Core.Entities
{
    public class TreatmentMaterialConsumptionLine : IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int ConsumptionId { get; set; }
        public virtual TreatmentMaterialConsumption Consumption { get; set; } = null!;
        public virtual int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; } = null!;
        public virtual decimal ProposedQuantity { get; set; }
        public virtual decimal ActualQuantity { get; set; }
        public virtual bool IsIncluded { get; set; } = true;
        public virtual int? StockMovementId { get; set; }
        public virtual StockMovement? StockMovement { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
    }
}
