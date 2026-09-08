namespace Clinic_System.Data.Configurations
{
    public class TreatmentMaterialConsumptionConfiguration : IEntityTypeConfiguration<TreatmentMaterialConsumption>
    {
        public void Configure(EntityTypeBuilder<TreatmentMaterialConsumption> builder)
        {
            builder.HasKey(c => c.Id);
            builder.ToTable("TreatmentMaterialConsumptions");

            builder.Property(c => c.Notes).HasMaxLength(500);
            builder.Property(c => c.ConfirmedByUserId).HasMaxLength(450);

            builder.HasIndex(c => c.DentalTreatmentId).IsUnique();

            builder.HasOne(c => c.DentalTreatment)
                .WithMany()
                .HasForeignKey(c => c.DentalTreatmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Lines)
                .WithOne(l => l.Consumption)
                .HasForeignKey(l => l.ConsumptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
