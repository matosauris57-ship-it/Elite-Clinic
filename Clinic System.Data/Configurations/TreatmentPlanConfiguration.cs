namespace Clinic_System.Data.Configurations
{
    public class TreatmentPlanConfiguration : IEntityTypeConfiguration<TreatmentPlan>
    {
        public void Configure(EntityTypeBuilder<TreatmentPlan> builder)
        {
            builder.HasKey(p => p.Id);
            builder.ToTable("TreatmentPlans");

            builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Notes).HasMaxLength(1000);
            builder.Property(p => p.DiscountAmount).HasColumnType("decimal(18,2)");

            builder.HasOne(p => p.Patient)
                .WithMany(pt => pt.TreatmentPlans)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Items)
                .WithOne(i => i.TreatmentPlan)
                .HasForeignKey(i => i.TreatmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
