namespace Clinic_System.Data.Configurations;

public class DashboardLayoutConfiguration : IEntityTypeConfiguration<DashboardLayout>
{
    public void Configure(EntityTypeBuilder<DashboardLayout> builder)
    {
        builder.ToTable("DashboardLayouts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Scope).IsRequired().HasMaxLength(20);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.LayoutJson).IsRequired();
        builder.Property(x => x.UpdatedByUserId).HasMaxLength(450);

        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL")
            .HasDatabaseName("IX_DashboardLayouts_UserId");

        builder.HasIndex(x => x.Scope)
            .IsUnique()
            .HasFilter("[Scope] = 'Clinic'")
            .HasDatabaseName("IX_DashboardLayouts_ClinicScope");
    }
}
