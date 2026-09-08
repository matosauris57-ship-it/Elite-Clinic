namespace Clinic_System.Core.Entities
{
    public class InventoryItem : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual string Sku { get; set; } = null!;
        public virtual string Name { get; set; } = null!;
        public virtual string Category { get; set; } = null!;
        /// <summary>Unidad de medida: u, ml, g, etc.</summary>
        public virtual string Unit { get; set; } = "u";
        public virtual decimal QuantityOnHand { get; set; }
        public virtual decimal MinimumStock { get; set; }
        public virtual bool IsActive { get; set; } = true;

        public virtual ICollection<ProcedureMaterial> ProcedureMaterials { get; set; } = new List<ProcedureMaterial>();
        public virtual ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public bool IsLowStock => QuantityOnHand <= MinimumStock;
    }
}
