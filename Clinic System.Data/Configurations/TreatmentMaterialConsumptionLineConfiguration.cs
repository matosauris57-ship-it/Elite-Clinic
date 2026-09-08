namespace Clinic_System.Data.Configurations
{
    public class TreatmentMaterialConsumptionLineConfiguration : IEntityTypeConfiguration<TreatmentMaterialConsumptionLine>
    {
        public void Configure(EntityTypeBuilder<TreatmentMaterialConsumptionLine> builder)
        {
            builder.HasKey(l => l.Id);
            builder.ToTable("TreatmentMaterialConsumptionLines");

            builder.Property(l => l.ProposedQuantity).HasColumnType("decimal(18,3)");
            builder.Property(l => l.ActualQuantity).HasColumnType("decimal(18,3)");

            builder.HasCheckConstraint("CK_TreatmentMaterialConsumptionLines_Proposed", "[ProposedQuantity] >= 0");
            builder.HasCheckConstraint("CK_TreatmentMaterialConsumptionLines_Actual", "[ActualQuantity] >= 0");

            builder.HasOne(l => l.InventoryItem)
                .WithMany()
                .HasForeignKey(l => l.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.StockMovement)
                .WithMany()
                .HasForeignKey(l => l.StockMovementId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(l => new { l.ConsumptionId, l.InventoryItemId }).IsUnique();
        }
    }
}
