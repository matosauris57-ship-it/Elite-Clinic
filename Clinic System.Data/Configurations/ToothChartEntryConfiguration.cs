namespace Clinic_System.Data.Configurations;

public class ToothChartEntryConfiguration : IEntityTypeConfiguration<ToothChartEntry>
{
    public void Configure(EntityTypeBuilder<ToothChartEntry> builder)
    {
        builder.ToTable("ToothChartEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ClinicalDiagnosis).HasMaxLength(200);
        builder.Property(x => x.ProposedTreatment).HasMaxLength(500);
        builder.Property(x => x.RecordedByUserId).HasMaxLength(450);
        builder.Property(x => x.RestorationMaterial);
        builder.Property(x => x.CariesType);
        builder.Property(x => x.Icdas);
        builder.Property(x => x.BridgeRole).HasConversion<int?>();
        builder.HasIndex(x => new { x.PatientId, x.ToothNumber, x.Surface, x.Phase, x.RecordedAt });
        builder.HasIndex(x => x.BridgeSpanId);
        builder.HasCheckConstraint("CK_ToothChartEntries_FDI",
            "(([ToothNumber] BETWEEN 11 AND 18) OR ([ToothNumber] BETWEEN 21 AND 28) OR ([ToothNumber] BETWEEN 31 AND 38) OR ([ToothNumber] BETWEEN 41 AND 48) OR ([ToothNumber] BETWEEN 51 AND 55) OR ([ToothNumber] BETWEEN 61 AND 65) OR ([ToothNumber] BETWEEN 71 AND 75) OR ([ToothNumber] BETWEEN 81 AND 85))");

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.ToothChartEntries)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Appointment)
            .WithMany()
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
