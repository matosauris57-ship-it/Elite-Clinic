namespace Clinic_System.Core.Entities
{
    public class EmergencyContact
    {
        public virtual int Id { get; set; }
        public virtual int PatientId { get; set; }
        public virtual string FullName { get; set; } = null!;
        public virtual string Phone { get; set; } = null!;
        public virtual string Relationship { get; set; } = null!;
        public virtual string? Notes { get; set; }
        public virtual Patient Patient { get; set; } = null!;
    }
}
