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
            builder.HasCheckConstraint("CK_PlanItems_Quantity", "[Quantity] > 0");
            builder.HasCheckConstraint("CK_PlanItems_UnitPrice", "[UnitPrice] >= 0");
            builder.HasOne(i => i.TreatmentProcedure)
                .WithMany()
                .HasForeignKey(i => i.TreatmentProcedureId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
