using Clinic_System.Core.Finance;

namespace Clinic_System.Core.Entities
{
    public class InvoiceLine : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int PaymentId { get; set; }
        public virtual Payment Payment { get; set; } = null!;

        public virtual string Description { get; set; } = null!;
        public virtual int? ToothNumber { get; set; }
        public virtual int Quantity { get; set; } = 1;
        public virtual decimal UnitPrice { get; set; }
        public virtual int? DentalTreatmentId { get; set; }
        public virtual DentalTreatment? DentalTreatment { get; set; }

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal LineTotal => Money.Multiply(UnitPrice, Quantity);
    }
}
