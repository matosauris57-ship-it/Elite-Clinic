namespace Clinic_System.Core.Entities
{
    /// <summary>Consumo de materiales confirmado al completar un tratamiento clínico.</summary>
    public class TreatmentMaterialConsumption : IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int DentalTreatmentId { get; set; }
        public virtual DentalTreatment DentalTreatment { get; set; } = null!;
        public virtual string? Notes { get; set; }
        public virtual string? ConfirmedByUserId { get; set; }
        public virtual DateTime ConfirmedAt { get; set; }

        public virtual ICollection<TreatmentMaterialConsumptionLine> Lines { get; set; } = new List<TreatmentMaterialConsumptionLine>();

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
    }
}
