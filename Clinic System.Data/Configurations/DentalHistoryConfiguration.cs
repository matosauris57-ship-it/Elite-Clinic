namespace Clinic_System.Data.Configurations
{
    public class DentalHistoryConfiguration : IEntityTypeConfiguration<DentalHistory>
    {
        public void Configure(EntityTypeBuilder<DentalHistory> builder)
        {
            builder.HasKey(h => h.Id);
            builder.ToTable("DentalHistories");

            builder.Property(h => h.Allergies).HasMaxLength(500);
            builder.Property(h => h.CurrentMedication).HasMaxLength(500);
            builder.Property(h => h.SystemicDiseases).HasMaxLength(1000);
            builder.Property(h => h.PreviousDentalTreatments).HasMaxLength(2000);
            builder.Property(h => h.BloodPressure).HasMaxLength(50);
            builder.Property(h => h.OtherDiseases).HasMaxLength(1000);
            builder.Property(h => h.ReasonForConsultation).HasMaxLength(1000);
            builder.Property(h => h.Diagnosis).HasMaxLength(1000);
            builder.Property(h => h.ClinicalObservations).HasMaxLength(2000);
            builder.Property(h => h.AdditionalNotes).HasMaxLength(1000);

            builder.HasOne(h => h.Patient)
                .WithOne(p => p.DentalHistory)
                .HasForeignKey<DentalHistory>(h => h.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(h => h.PatientId).IsUnique();
        }
    }
}
