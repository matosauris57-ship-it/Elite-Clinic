namespace Clinic_System.Data.Configurations
{
    public class DoctorProcedurePriceConfiguration : IEntityTypeConfiguration<DoctorProcedurePrice>
    {
        public void Configure(EntityTypeBuilder<DoctorProcedurePrice> builder)
        {
            builder.ToTable("DoctorProcedurePrices", t =>
            {
                t.HasCheckConstraint("CK_DoctorProcedurePrices_Price", "[Price] >= 0");
            });

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");

            builder.HasIndex(p => new { p.DoctorId, p.TreatmentProcedureId })
                .IsUnique()
                .HasDatabaseName("IX_DoctorProcedurePrices_Doctor_Procedure");

            builder.HasOne(p => p.Doctor)
                .WithMany(d => d.ProcedurePrices)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.TreatmentProcedure)
                .WithMany(t => t.DoctorPrices)
                .HasForeignKey(p => p.TreatmentProcedureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
