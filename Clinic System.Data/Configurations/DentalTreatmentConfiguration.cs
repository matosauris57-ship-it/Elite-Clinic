namespace Clinic_System.Data.Configurations
{
    public class DentalTreatmentConfiguration : IEntityTypeConfiguration<DentalTreatment>
    {
        public void Configure(EntityTypeBuilder<DentalTreatment> builder)
        {
            builder.HasKey(t => t.Id);
            builder.ToTable("DentalTreatments");

            builder.Property(t => t.ProcedureName).IsRequired().HasMaxLength(200);
            builder.Property(t => t.ProcedureDetails).HasMaxLength(2000);
            builder.Property(t => t.MedicalNotes).HasMaxLength(4000);
            builder.Property(t => t.Cost).HasColumnType("decimal(18,2)");
            builder.HasCheckConstraint("CK_DentalTreatments_Cost", "[Cost] >= 0");

            builder.HasOne(t => t.Patient)
                .WithMany(p => p.DentalTreatments)
                .HasForeignKey(t => t.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Appointment)
                .WithMany(a => a.DentalTreatments)
                .HasForeignKey(t => t.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(t => t.ToothRecord)
                .WithMany()
                .HasForeignKey(t => t.ToothRecordId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(t => t.TreatmentProcedure)
                .WithMany()
                .HasForeignKey(t => t.TreatmentProcedureId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
