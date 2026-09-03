namespace Clinic_System.Data.Configurations
{
    public class ToothRecordConfiguration : IEntityTypeConfiguration<ToothRecord>
    {
        public void Configure(EntityTypeBuilder<ToothRecord> builder)
        {
            builder.HasKey(t => t.Id);
            builder.ToTable("ToothRecords");

            builder.Property(t => t.ToothNumber).IsRequired();
            builder.Property(t => t.DiagnosisCondition).IsRequired();
            builder.Property(t => t.TreatmentCondition).IsRequired(false);
            builder.Property(t => t.Notes).HasMaxLength(500);

            builder.HasOne(t => t.Patient)
                .WithMany(p => p.ToothRecords)
                .HasForeignKey(t => t.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => new { t.PatientId, t.ToothNumber }).IsUnique();
        }
    }
}
