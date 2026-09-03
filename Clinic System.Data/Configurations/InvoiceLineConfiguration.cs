namespace Clinic_System.Data.Configurations
{
    public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
    {
        public void Configure(EntityTypeBuilder<InvoiceLine> builder)
        {
            builder.HasKey(l => l.Id);
            builder.ToTable("InvoiceLines");

            builder.Property(l => l.Description).IsRequired().HasMaxLength(300);
            builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
            builder.HasCheckConstraint("CK_InvoiceLines_Quantity", "[Quantity] > 0");
            builder.HasCheckConstraint("CK_InvoiceLines_UnitPrice", "[UnitPrice] >= 0");

            builder.HasOne(l => l.Payment)
                .WithMany(p => p.InvoiceLines)
                .HasForeignKey(l => l.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.DentalTreatment)
                .WithMany()
                .HasForeignKey(l => l.DentalTreatmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
