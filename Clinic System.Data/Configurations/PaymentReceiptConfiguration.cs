namespace Clinic_System.Data.Configurations
{
    public class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
    {
        public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
        {
            builder.ToTable("PaymentReceipts", table =>
            {
                table.HasCheckConstraint("CK_PaymentReceipts_Amount", "[Amount] > 0");
            });

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(r => r.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.Notes).HasMaxLength(500);
            builder.Property(r => r.PaidAt).IsRequired();

            builder.HasOne(r => r.Payment)
                .WithMany(p => p.Receipts)
                .HasForeignKey(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.PaymentId);
            builder.HasIndex(r => r.PaidAt);
        }
    }
}
