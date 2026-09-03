namespace Clinic_System.Data.Configurations;

public class DentalClinicalEventConfiguration : IEntityTypeConfiguration<DentalClinicalEvent>
{
    public void Configure(EntityTypeBuilder<DentalClinicalEvent> builder)
    {
        builder.ToTable("DentalClinicalEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ReferenceType).HasMaxLength(100);
        builder.Property(x => x.ReferenceId).HasMaxLength(100);
        builder.Property(x => x.RecordedByUserId).HasMaxLength(450);
        builder.HasIndex(x => new { x.PatientId, x.RecordedAt });
        builder.HasIndex(x => new { x.PatientId, x.ToothNumber, x.RecordedAt });

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.DentalClinicalEvents)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
