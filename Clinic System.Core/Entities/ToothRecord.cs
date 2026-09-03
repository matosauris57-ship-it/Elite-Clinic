namespace Clinic_System.Core.Entities
{
    public class ToothRecord : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        /// <summary>FDI notation (11-48 permanent, 51-85 deciduous)</summary>
        public virtual int ToothNumber { get; set; }
        public virtual ToothCondition DiagnosisCondition { get; set; } = ToothCondition.Healthy;
        public virtual ToothCondition? TreatmentCondition { get; set; }
        public virtual string? Notes { get; set; }

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public void UpdateDiagnosis(ToothCondition condition, string? notes = null)
        {
            DiagnosisCondition = condition;
            if (notes != null) Notes = notes;
            UpdatedAt = DateTime.Now;
        }

        public void UpdateTreatment(ToothCondition? condition)
        {
            TreatmentCondition = condition;
            UpdatedAt = DateTime.Now;
        }
    }
}
