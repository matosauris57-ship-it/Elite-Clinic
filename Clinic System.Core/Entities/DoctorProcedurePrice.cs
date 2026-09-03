namespace Clinic_System.Core.Entities
{
    public class DoctorProcedurePrice : IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual int TreatmentProcedureId { get; set; }
        public virtual TreatmentProcedure TreatmentProcedure { get; set; } = null!;
        public virtual decimal Price { get; set; }

        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
    }
}
