namespace Clinic_System.Core.Entities
{
    public class PatientMedicalCondition
    {
        public virtual int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public virtual int MedicalConditionId { get; set; }
        public virtual MedicalCondition MedicalCondition { get; set; } = null!;

        public virtual string? Notes { get; set; }
    }
}
