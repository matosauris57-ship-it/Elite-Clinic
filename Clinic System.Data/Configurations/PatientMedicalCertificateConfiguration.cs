namespace Clinic_System.Data.Configurations;

public class PatientMedicalCertificateConfiguration : IEntityTypeConfiguration<PatientMedicalCertificate>
{
    public void Configure(EntityTypeBuilder<PatientMedicalCertificate> builder)
    {
        builder.ToTable("PatientMedicalCertificates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CertificateType).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Diagnosis).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Recommendation).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Observations).HasMaxLength(2000);
        builder.Property(x => x.Purpose).HasMaxLength(80);
        builder.Property(x => x.RecordedByUserId).HasMaxLength(450);

        builder.HasIndex(x => new { x.PatientId, x.IssuedAt });
        builder.HasIndex(x => x.DoctorId);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.MedicalCertificates)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
