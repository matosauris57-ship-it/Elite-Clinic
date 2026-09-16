using Clinic_System.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic_System.Data.Configurations;

public class CampaignAppointmentRequestConfiguration : IEntityTypeConfiguration<CampaignAppointmentRequest>
{
    public void Configure(EntityTypeBuilder<CampaignAppointmentRequest> builder)
    {
        builder.ToTable("CampaignAppointmentRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.OtherServiceText).HasMaxLength(200);
        builder.Property(x => x.ResolvedByUserId).HasMaxLength(450);
        builder.Property(x => x.ResolvedByName).HasMaxLength(256);
        builder.Property(x => x.StaffNote).HasMaxLength(500);

        builder.HasOne(x => x.EmailCampaign)
            .WithMany()
            .HasForeignKey(x => x.EmailCampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmailCampaignRecipient)
            .WithMany()
            .HasForeignKey(x => x.EmailCampaignRecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedDoctor)
            .WithMany()
            .HasForeignKey(x => x.RequestedDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ScheduledDoctor)
            .WithMany()
            .HasForeignKey(x => x.ScheduledDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TreatmentProcedure)
            .WithMany()
            .HasForeignKey(x => x.TreatmentProcedureId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Appointment)
            .WithMany()
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.EmailCampaignRecipientId)
            .IsUnique()
            .HasDatabaseName("IX_CampaignAppointmentRequests_Recipient");

        builder.HasIndex(x => new { x.Status, x.RequestedAt })
            .HasDatabaseName("IX_CampaignAppointmentRequests_Status_Requested");
    }
}
