namespace Clinic_System.Data.Configurations
{
    public class PatientMedicalConditionConfiguration : IEntityTypeConfiguration<PatientMedicalCondition>
    {
        public void Configure(EntityTypeBuilder<PatientMedicalCondition> builder)
        {
            builder.HasKey(pc => new { pc.PatientId, pc.MedicalConditionId });
            builder.ToTable("PatientMedicalConditions");

            builder.Property(pc => pc.Notes).HasMaxLength(500);

            builder.HasOne(pc => pc.Patient)
                .WithMany(p => p.MedicalConditions)
                .HasForeignKey(pc => pc.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pc => pc.MedicalCondition)
                .WithMany(c => c.PatientConditions)
                .HasForeignKey(pc => pc.MedicalConditionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
