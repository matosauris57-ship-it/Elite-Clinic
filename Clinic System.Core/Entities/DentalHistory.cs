namespace Clinic_System.Core.Entities
{
    public class DentalHistory : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public virtual string? Allergies { get; set; }
        public virtual string? CurrentMedication { get; set; }
        public virtual string? SystemicDiseases { get; set; }
        public virtual string? PreviousDentalTreatments { get; set; }
        public virtual string? BloodPressure { get; set; }
        public virtual string? OtherDiseases { get; set; }
        public virtual string? ReasonForConsultation { get; set; }
        public virtual string? Diagnosis { get; set; }
        public virtual string? ClinicalObservations { get; set; }
        public virtual bool HasBleedingGums { get; set; }
        public virtual bool HasSensitiveTeeth { get; set; }
        public virtual bool HasBruxism { get; set; }
        public virtual bool IsSmoker { get; set; }
        public virtual string? AdditionalNotes { get; set; }

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public void Update(
            string? allergies,
            string? currentMedication,
            string? systemicDiseases,
            string? previousDentalTreatments,
            string? bloodPressure,
            string? otherDiseases,
            string? reasonForConsultation,
            string? diagnosis,
            string? clinicalObservations,
            bool? hasBleedingGums,
            bool? hasSensitiveTeeth,
            bool? hasBruxism,
            bool? isSmoker,
            string? additionalNotes)
        {
            if (allergies != null) Allergies = allergies;
            if (currentMedication != null) CurrentMedication = currentMedication;
            if (systemicDiseases != null) SystemicDiseases = systemicDiseases;
            if (previousDentalTreatments != null) PreviousDentalTreatments = previousDentalTreatments;
            if (bloodPressure != null) BloodPressure = bloodPressure;
            if (otherDiseases != null) OtherDiseases = otherDiseases;
            if (reasonForConsultation != null) ReasonForConsultation = reasonForConsultation;
            if (diagnosis != null) Diagnosis = diagnosis;
            if (clinicalObservations != null) ClinicalObservations = clinicalObservations;
            if (hasBleedingGums.HasValue) HasBleedingGums = hasBleedingGums.Value;
            if (hasSensitiveTeeth.HasValue) HasSensitiveTeeth = hasSensitiveTeeth.Value;
            if (hasBruxism.HasValue) HasBruxism = hasBruxism.Value;
            if (isSmoker.HasValue) IsSmoker = isSmoker.Value;
            if (additionalNotes != null) AdditionalNotes = additionalNotes;
            UpdatedAt = DateTime.Now;
        }
    }
}
