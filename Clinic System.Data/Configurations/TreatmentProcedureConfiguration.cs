namespace Clinic_System.Data.Configurations
{
    public class TreatmentProcedureConfiguration : IEntityTypeConfiguration<TreatmentProcedure>
    {
        public void Configure(EntityTypeBuilder<TreatmentProcedure> builder)
        {
            builder.HasKey(p => p.Id);
            builder.ToTable("TreatmentProcedures");

            builder.Property(p => p.Code).IsRequired().HasMaxLength(80);
            builder.HasIndex(p => p.Code).IsUnique();

            builder.Property(p => p.Category).IsRequired().HasMaxLength(80);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.HasCheckConstraint("CK_TreatmentProcedures_Price", "[Price] >= 0");
            builder.HasCheckConstraint("CK_TreatmentProcedures_Duration", "[DurationMinutes] > 0");
        }
    }
}
