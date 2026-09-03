namespace Clinic_System.Data.Configurations
{
    public class MedicalConditionConfiguration : IEntityTypeConfiguration<MedicalCondition>
    {
        public void Configure(EntityTypeBuilder<MedicalCondition> builder)
        {
            builder.HasKey(c => c.Id);
            builder.ToTable("MedicalConditions");

            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Category).HasMaxLength(80);
            builder.HasIndex(c => c.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}
