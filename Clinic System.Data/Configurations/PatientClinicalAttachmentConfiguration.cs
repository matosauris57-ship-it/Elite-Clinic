using Clinic_System.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic_System.Data.Configurations;

public class PatientClinicalAttachmentConfiguration : IEntityTypeConfiguration<PatientClinicalAttachment>
{
    public void Configure(EntityTypeBuilder<PatientClinicalAttachment> builder)
    {
        builder.ToTable("PatientClinicalAttachments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Subtype).HasMaxLength(80);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(80);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(120);
        builder.Property(x => x.RecordedByUserId).HasMaxLength(450);

        builder.HasIndex(x => x.PatientId)
            .HasDatabaseName("IX_PatientClinicalAttachments_PatientId");
        builder.HasIndex(x => new { x.PatientId, x.Kind, x.CreatedAt })
            .HasDatabaseName("IX_PatientClinicalAttachments_Patient_Kind_Created");

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.ClinicalAttachments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
