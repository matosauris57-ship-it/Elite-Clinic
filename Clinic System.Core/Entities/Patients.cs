namespace Clinic_System.Core.Entities
{
    public class Patient : Person
    {
        public virtual string? NationalId { get; set; }
        public virtual string? MobilePhone { get; set; }
        public virtual string? Email { get; set; }
        public virtual int? BirthdayEmailLastSentYear { get; set; }
        public virtual bool OptOutEmailCampaigns { get; set; }
        public virtual bool EmailInvalid { get; set; }
        public virtual ICollection<EmailCampaignRecipient> EmailCampaignRecipients { get; set; } = new List<EmailCampaignRecipient>();
        public virtual string? ApplicationUserId { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual DentalHistory? DentalHistory { get; set; }
        public virtual ICollection<ToothRecord> ToothRecords { get; set; } = new List<ToothRecord>();
        public virtual ICollection<ToothChartEntry> ToothChartEntries { get; set; } = new List<ToothChartEntry>();
        public virtual ICollection<DentalClinicalEvent> DentalClinicalEvents { get; set; } = new List<DentalClinicalEvent>();
        public virtual ICollection<DentalTreatment> DentalTreatments { get; set; } = new List<DentalTreatment>();
        public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
        public virtual ICollection<PatientMedicalCondition> MedicalConditions { get; set; } = new List<PatientMedicalCondition>();
        public virtual ICollection<PeriodontalExam> PeriodontalExams { get; set; } = new List<PeriodontalExam>();
        public virtual ICollection<PatientPrescription> PatientPrescriptions { get; set; } = new List<PatientPrescription>();
    }
}
