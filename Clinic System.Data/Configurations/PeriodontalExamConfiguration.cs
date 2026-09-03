namespace Clinic_System.Data.Configurations;

public class PeriodontalExamConfiguration : IEntityTypeConfiguration<PeriodontalExam>
{
    public void Configure(EntityTypeBuilder<PeriodontalExam> builder)
    {
        builder.ToTable("PeriodontalExams");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.RecordedByUserId).HasMaxLength(450);
        builder.Property(x => x.BleedingPercent).HasPrecision(5, 1);
        builder.Property(x => x.PlaquePercent).HasPrecision(5, 1);
        builder.Property(x => x.MeanProbingDepthMm).HasPrecision(4, 1);
        builder.HasIndex(x => new { x.PatientId, x.ExaminedAt });

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.PeriodontalExams)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PeriodontalToothConfiguration : IEntityTypeConfiguration<PeriodontalTooth>
{
    public void Configure(EntityTypeBuilder<PeriodontalTooth> builder)
    {
        builder.ToTable("PeriodontalTeeth");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Mobility).HasConversion<int>();
        builder.Property(x => x.FacialFurcation).HasConversion<int>();
        builder.Property(x => x.LingualFurcation).HasConversion<int>();
        builder.HasIndex(x => new { x.PeriodontalExamId, x.ToothNumber }).IsUnique();
        builder.HasCheckConstraint("CK_PeriodontalTeeth_FDI",
            "(([ToothNumber] BETWEEN 11 AND 18) OR ([ToothNumber] BETWEEN 21 AND 28) OR ([ToothNumber] BETWEEN 31 AND 38) OR ([ToothNumber] BETWEEN 41 AND 48))");
        builder.HasCheckConstraint("CK_PeriodontalTeeth_KG",
            "[KeratinizedGingivaMm] IS NULL OR ([KeratinizedGingivaMm] >= 0 AND [KeratinizedGingivaMm] <= 15)");

        builder.HasOne(x => x.Exam)
            .WithMany(x => x.Teeth)
            .HasForeignKey(x => x.PeriodontalExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PeriodontalSiteConfiguration : IEntityTypeConfiguration<PeriodontalSite>
{
    public void Configure(EntityTypeBuilder<PeriodontalSite> builder)
    {
        builder.ToTable("PeriodontalSites", t =>
        {
            t.HasCheckConstraint("CK_PeriodontalSites_ProbingDepth",
                "[ProbingDepthMm] IS NULL OR ([ProbingDepthMm] >= 0 AND [ProbingDepthMm] <= 15)");
            t.HasCheckConstraint("CK_PeriodontalSites_Recession",
                "[RecessionMm] IS NULL OR ([RecessionMm] >= 0 AND [RecessionMm] <= 15)");
            t.HasCheckConstraint("CK_PeriodontalSites_Cal",
                "[ClinicalAttachmentLevelMm] IS NULL OR ([ClinicalAttachmentLevelMm] >= 0 AND [ClinicalAttachmentLevelMm] <= 30)");
        });
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PeriodontalToothId, x.Surface, x.Position }).IsUnique();

        builder.HasOne(x => x.Tooth)
            .WithMany(x => x.Sites)
            .HasForeignKey(x => x.PeriodontalToothId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
