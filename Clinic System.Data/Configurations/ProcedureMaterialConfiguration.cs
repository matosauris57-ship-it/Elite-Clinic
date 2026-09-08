namespace Clinic_System.Data.Configurations
{
    public class ProcedureMaterialConfiguration : IEntityTypeConfiguration<ProcedureMaterial>
    {
        public void Configure(EntityTypeBuilder<ProcedureMaterial> builder)
        {
            builder.HasKey(p => p.Id);
            builder.ToTable("ProcedureMaterials");

            builder.Property(p => p.DefaultQuantity).HasColumnType("decimal(18,3)");
            builder.HasCheckConstraint("CK_ProcedureMaterials_DefaultQuantity", "[DefaultQuantity] > 0");

            builder.HasIndex(p => new { p.TreatmentProcedureId, p.InventoryItemId }).IsUnique();

            builder.HasOne(p => p.TreatmentProcedure)
                .WithMany()
                .HasForeignKey(p => p.TreatmentProcedureId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.InventoryItem)
                .WithMany(i => i.ProcedureMaterials)
                .HasForeignKey(p => p.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
