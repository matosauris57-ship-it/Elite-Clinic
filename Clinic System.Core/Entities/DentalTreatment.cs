namespace Clinic_System.Core.Entities
{
    public class DentalTreatment : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;
        public virtual int? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }
        public virtual int? ToothRecordId { get; set; }
        public virtual ToothRecord? ToothRecord { get; set; }

        public virtual string ProcedureName { get; set; } = null!;
        public virtual int? TreatmentProcedureId { get; set; }
        public virtual TreatmentProcedure? TreatmentProcedure { get; set; }
        public virtual string? ProcedureDetails { get; set; }
        public virtual string? MedicalNotes { get; set; }
        public virtual int? ToothNumber { get; set; }
        public virtual ToothSurface? ToothSurface { get; set; }
        public virtual decimal Cost { get; set; }
        public virtual DentalTreatmentStatus Status { get; set; } = DentalTreatmentStatus.Planned;
        public virtual DateTime? PerformedDate { get; set; }

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public void Start()
        {
            if (Status != DentalTreatmentStatus.Planned)
                throw new InvalidOperationException("Only planned treatments can be started.");

            Status = DentalTreatmentStatus.InProgress;
            UpdatedAt = DateTime.Now;
        }

        public void Complete(DateTime? performedDate = null)
        {
            if (Status != DentalTreatmentStatus.InProgress)
                throw new InvalidOperationException("Only treatments in progress can be completed.");

            Status = DentalTreatmentStatus.Completed;
            PerformedDate = performedDate ?? DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public void Cancel(string? reason = null)
        {
            if (Status is not (DentalTreatmentStatus.Planned or DentalTreatmentStatus.InProgress))
                throw new InvalidOperationException("Only planned or in-progress treatments can be cancelled.");

            Status = DentalTreatmentStatus.Cancelled;
            if (reason != null) ProcedureDetails = reason;
            UpdatedAt = DateTime.Now;
        }
    }
}
