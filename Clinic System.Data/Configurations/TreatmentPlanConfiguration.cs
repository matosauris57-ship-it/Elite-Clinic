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
            builder.Property(p => p.AcceptedByName).HasMaxLength(120);
            builder.Property(p => p.RejectionReason).HasMaxLength(500);

            builder.HasOne(p => p.Patient)
                .WithMany(pt => pt.TreatmentPlans)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.InvoicePayment)
                .WithMany()
                .HasForeignKey(p => p.InvoicePaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.Items)
                .WithOne(i => i.TreatmentPlan)
                .HasForeignKey(i => i.TreatmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(p => p.TotalAmount);
            builder.Ignore(p => p.FinalAmount);
            builder.Ignore(p => p.OpenInvoices);
            builder.Ignore(p => p.AmountBilled);
            builder.Ignore(p => p.AmountCollectedOnAccount);
            builder.Ignore(p => p.RemainingToBill);
            builder.Ignore(p => p.RemainingToCollect);
            builder.Ignore(p => p.BillableCompletedItems);
            builder.Ignore(p => p.UnbilledCompletedAmount);
            builder.Ignore(p => p.InvoicePaymentIds);
            builder.Ignore(p => p.CanIssue);
            builder.Ignore(p => p.CanAccept);
            builder.Ignore(p => p.CanReject);
            builder.Ignore(p => p.CanInvoice);
            builder.Ignore(p => p.IsExpired);
        }
    }
}
