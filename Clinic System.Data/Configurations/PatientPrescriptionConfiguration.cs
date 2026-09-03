namespace Clinic_System.Data.Configurations;

public class PatientPrescriptionConfiguration : IEntityTypeConfiguration<PatientPrescription>
{
    public void Configure(EntityTypeBuilder<PatientPrescription> builder)
    {
        builder.ToTable("PatientPrescriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Diagnosis).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.RecordedByUserId).HasMaxLength(450);
        builder.HasIndex(x => new { x.PatientId, x.IssuedAt });

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.PatientPrescriptions)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PatientPrescriptionItemConfiguration : IEntityTypeConfiguration<PatientPrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PatientPrescriptionItem> builder)
    {
        builder.ToTable("PatientPrescriptionItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TemplateKey).HasMaxLength(80);
        builder.Property(x => x.MedicationName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Dosage).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Frequency).IsRequired().HasMaxLength(120);
        builder.Property(x => x.SpecialInstructions).HasMaxLength(500);
        builder.HasIndex(x => new { x.PatientPrescriptionId, x.SortOrder });

        builder.HasOne(x => x.Prescription)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PatientPrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
