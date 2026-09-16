using Clinic_System.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic_System.Data.Configurations;

public class PatientInformedConsentConfiguration : IEntityTypeConfiguration<PatientInformedConsent>
{
    public void Configure(EntityTypeBuilder<PatientInformedConsent> builder)
    {
        builder.ToTable("PatientInformedConsents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProcedureExplanation).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.Benefits).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Risks).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Alternatives).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.AuthorizationText).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(80);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(120);
        builder.Property(x => x.RecordedByUserId).HasMaxLength(450);

        builder.HasIndex(x => x.PatientId)
            .HasDatabaseName("IX_PatientInformedConsents_PatientId");
        builder.HasIndex(x => new { x.PatientId, x.ConsentType, x.SignedOn })
            .HasDatabaseName("IX_PatientInformedConsents_Patient_Type_Signed");

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.InformedConsents)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
