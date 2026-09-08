namespace Clinic_System.Core.Entities
{
    /// <summary>
    /// BOM: materiales que un procedimiento del catálogo propone consumir.
    /// Separado de facturación: el procedimiento facturable no es el ítem de inventario.
    /// </summary>
    public class ProcedureMaterial : IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int TreatmentProcedureId { get; set; }
        public virtual TreatmentProcedure TreatmentProcedure { get; set; } = null!;
        public virtual int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; } = null!;
        public virtual decimal DefaultQuantity { get; set; }
        public virtual bool IsOptional { get; set; }
        public virtual int SortOrder { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
    }
}
