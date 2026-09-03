namespace Clinic_System.Core.Entities
{
    public class MedicalCondition : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual string? Category { get; set; }
        public virtual bool IsActive { get; set; } = true;
        public virtual int SortOrder { get; set; }

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public virtual ICollection<PatientMedicalCondition> PatientConditions { get; set; } = new List<PatientMedicalCondition>();
    }
}
