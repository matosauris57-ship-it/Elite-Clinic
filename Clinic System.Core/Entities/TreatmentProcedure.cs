namespace Clinic_System.Core.Entities
{
    public class TreatmentProcedure : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual string Code { get; set; } = null!;
        public virtual string Category { get; set; } = null!;
        public virtual string Name { get; set; } = null!;
        public virtual decimal Price { get; set; }
        public virtual int DurationMinutes { get; set; }
        public virtual bool IsActive { get; set; } = true;
        public virtual ICollection<DoctorProcedurePrice> DoctorPrices { get; set; } = new List<DoctorProcedurePrice>();

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
    }
}
