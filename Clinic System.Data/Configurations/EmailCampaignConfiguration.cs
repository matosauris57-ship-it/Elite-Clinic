namespace Clinic_System.Data.Configurations;

public class EmailCampaignConfiguration : IEntityTypeConfiguration<EmailCampaign>
{
    public void Configure(EntityTypeBuilder<EmailCampaign> builder)
    {
        builder.ToTable("EmailCampaigns");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(80);
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Body).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.BatchSize).IsRequired().HasDefaultValue(15);

        builder.HasMany(x => x.Recipients)
            .WithOne(x => x.EmailCampaign)
            .HasForeignKey(x => x.EmailCampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Status).HasDatabaseName("IX_EmailCampaigns_Status");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_EmailCampaigns_CreatedAt");
    }
}

public class EmailCampaignRecipientConfiguration : IEntityTypeConfiguration<EmailCampaignRecipient>
{
    public void Configure(EntityTypeBuilder<EmailCampaignRecipient> builder)
    {
        builder.ToTable("EmailCampaignRecipients");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(120);
        builder.Property(x => x.PatientName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Error).HasMaxLength(500);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.EmailCampaignRecipients)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmailCampaignId, x.PatientId })
            .IsUnique()
            .HasDatabaseName("IX_EmailCampaignRecipients_Campaign_Patient");

        builder.HasIndex(x => new { x.Status, x.EmailCampaignId })
            .HasDatabaseName("IX_EmailCampaignRecipients_Status_Campaign");
    }
}
