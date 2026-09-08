namespace Clinic_System.Data.Configurations
{
    public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.HasKey(i => i.Id);
            builder.ToTable("InventoryItems");

            builder.Property(i => i.Sku).IsRequired().HasMaxLength(80);
            builder.HasIndex(i => i.Sku).IsUnique();

            builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
            builder.Property(i => i.Category).IsRequired().HasMaxLength(80);
            builder.Property(i => i.Unit).IsRequired().HasMaxLength(20);

            builder.Property(i => i.QuantityOnHand).HasColumnType("decimal(18,3)");
            builder.Property(i => i.MinimumStock).HasColumnType("decimal(18,3)");

            builder.HasCheckConstraint("CK_InventoryItems_QuantityOnHand", "[QuantityOnHand] >= 0");
            builder.HasCheckConstraint("CK_InventoryItems_MinimumStock", "[MinimumStock] >= 0");

            builder.Ignore(i => i.IsLowStock);
        }
    }
}
