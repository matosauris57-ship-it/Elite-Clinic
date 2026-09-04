using Clinic_System.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic_System.Data.Configurations
{
    public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
    {
        public void Configure(EntityTypeBuilder<EmergencyContact> builder)
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("EmergencyContacts");

            builder.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Relationship).IsRequired().HasMaxLength(50);
            builder.Property(e => e.Notes).HasMaxLength(250);

            builder.HasOne(e => e.Patient)
                .WithMany(p => p.EmergencyContacts)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.PatientId)
                .HasDatabaseName("IX_EmergencyContacts_PatientId");
        }
    }
}
