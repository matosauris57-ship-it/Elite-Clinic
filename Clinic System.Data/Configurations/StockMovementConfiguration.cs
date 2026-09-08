namespace Clinic_System.Data.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.HasKey(m => m.Id);
            builder.ToTable("StockMovements");

            builder.Property(m => m.Type).HasConversion<string>().HasMaxLength(20);
            builder.Property(m => m.Quantity).HasColumnType("decimal(18,3)");
            builder.Property(m => m.QuantityBefore).HasColumnType("decimal(18,3)");
            builder.Property(m => m.QuantityAfter).HasColumnType("decimal(18,3)");
            builder.Property(m => m.Reason).HasMaxLength(200);
            builder.Property(m => m.Notes).HasMaxLength(500);
            builder.Property(m => m.ReferenceType).HasMaxLength(80);
            builder.Property(m => m.ReferenceId).HasMaxLength(80);
            builder.Property(m => m.CreatedByUserId).HasMaxLength(450);

            builder.HasCheckConstraint("CK_StockMovements_Quantity", "[Quantity] > 0");

            builder.HasOne(m => m.InventoryItem)
                .WithMany(i => i.Movements)
                .HasForeignKey(m => m.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.ReversesMovement)
                .WithMany()
                .HasForeignKey(m => m.ReversesMovementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId });
            builder.HasIndex(m => m.InventoryItemId);
            builder.Ignore(m => m.SignedDelta);
        }
    }
}
