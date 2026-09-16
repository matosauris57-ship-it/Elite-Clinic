using Clinic_System.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic_System.Data.Configurations;

public class PasswordRecoveryRequestConfiguration : IEntityTypeConfiguration<PasswordRecoveryRequest>
{
    public void Configure(EntityTypeBuilder<PasswordRecoveryRequest> builder)
    {
        builder.ToTable("PasswordRecoveryRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Identifier).IsRequired().HasMaxLength(256);
        builder.Property(x => x.NormalizedIdentifier).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Comment).HasMaxLength(500);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.UserEmail).HasMaxLength(256);
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.Property(x => x.UserDisplayName).HasMaxLength(256);
        builder.Property(x => x.UserType).HasMaxLength(40);
        builder.Property(x => x.ResolvedByUserId).HasMaxLength(450);
        builder.Property(x => x.ResolvedByName).HasMaxLength(256);
        builder.Property(x => x.ResolutionNote).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);

        builder.HasIndex(x => x.Status).HasDatabaseName("IX_PasswordRecoveryRequests_Status");
        builder.HasIndex(x => new { x.NormalizedIdentifier, x.Status, x.RequestedAt })
            .HasDatabaseName("IX_PasswordRecoveryRequests_Identifier_Status_Requested");
    }
}
