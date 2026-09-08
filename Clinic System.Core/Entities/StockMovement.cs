namespace Clinic_System.Core.Entities
{
    /// <summary>Kardex inmutable de inventario. Quantity siempre es positiva; el signo lo da Type.</summary>
    public class StockMovement : IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; } = null!;
        public virtual StockMovementType Type { get; set; }
        public virtual decimal Quantity { get; set; }
        public virtual decimal QuantityBefore { get; set; }
        public virtual decimal QuantityAfter { get; set; }
        public virtual string? Reason { get; set; }
        public virtual string? Notes { get; set; }
        public virtual string? ReferenceType { get; set; }
        public virtual string? ReferenceId { get; set; }
        public virtual int? ReversesMovementId { get; set; }
        public virtual StockMovement? ReversesMovement { get; set; }
        public virtual string? CreatedByUserId { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal SignedDelta => QuantityAfter - QuantityBefore;
    }
}
