namespace Clinic_System.Data.Configurations
{
    public class PlanItemConfiguration : IEntityTypeConfiguration<PlanItem>
    {
        public void Configure(EntityTypeBuilder<PlanItem> builder)
        {
            builder.HasKey(i => i.Id);
            builder.ToTable("PlanItems");

            builder.Property(i => i.ProcedureName).IsRequired().HasMaxLength(200);
            builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(i => i.Notes).HasMaxLength(500);
            builder.Property(i => i.AcceptanceStatus).HasConversion<string>().HasMaxLength(40);
            builder.Property(i => i.ExecutionStatus).HasConversion<string>().HasMaxLength(40);

            builder.HasCheckConstraint("CK_PlanItems_Quantity", "[Quantity] > 0");
            builder.HasCheckConstraint("CK_PlanItems_UnitPrice", "[UnitPrice] >= 0");
            builder.HasOne(i => i.TreatmentProcedure)
                .WithMany()
                .HasForeignKey(i => i.TreatmentProcedureId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(i => i.DentalTreatment)
                .WithMany()
                .HasForeignKey(i => i.DentalTreatmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(i => i.ScheduledAppointment)
                .WithMany()
                .HasForeignKey(i => i.ScheduledAppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.InvoicedPayment)
                .WithMany()
                .HasForeignKey(i => i.InvoicedPaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Ignore(i => i.LineTotal);
            builder.Ignore(i => i.CanAccept);
            builder.Ignore(i => i.CanReject);
            builder.Ignore(i => i.CanSchedule);
            builder.Ignore(i => i.CanInvoice);
        }
    }
}
