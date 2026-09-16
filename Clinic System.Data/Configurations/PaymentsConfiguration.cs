namespace Clinic_System.Data.Configurations
{
    /// <summary>
    /// Configuration for Payments Entity
    /// 
    /// هذا الـ Configuration يحدد:
    /// 1. العلاقة مع Appointments (Many-to-One)
    /// 2. Constraints على AmountPaid (يجب أن يكون > 0)
    /// 3. PaymentMethod Enum Configuration
    /// </summary>
    public class PaymentsConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // ============================================
            // Primary Key
            // ============================================
            builder.HasKey(p => p.Id);

            builder.ToTable("Payments", table =>
            {
                // Check Constraint: AmountPaid يجب أن يكون أكبر من 0
                table.HasCheckConstraint("CK_Payments_AmountPaid_Positive", 
                    "[AmountPaid] > 0");
            });

            // ============================================
            // AmountPaid Property
            // ============================================
            builder.Property(p => p.AmountPaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("AmountPaid");
            // HasColumnType: يحدد نوع البيانات في Database
            // decimal(18,2): 18 رقم إجمالي، 2 بعد الفاصلة (مثل: 1500.50)
            // HasColumnName: اسم العمود في Database
            // Check Constraint محدد في ToTable أعلاه

            // ============================================
            // PaymentDate Property
            // ============================================
            builder.Property(p => p.PaymentDate)
                .IsRequired(false)
                .HasColumnName("PaymentDate");

            // Index على PaymentDate للبحث السريع
            builder.HasIndex(p => p.PaymentDate)
                .HasDatabaseName("IX_Payments_PaymentDate");

            // ============================================
            // PaymentMethod Property
            // ============================================
            builder.Property(p => p.PaymentMethod)
                .IsRequired(false)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnName("PaymentMethod");
            // HasConversion<string>: يحول Enum إلى String في Database (بدلاً من int)
            // HasMaxLength(50): الحد الأقصى لطول النص
            // HasColumnName: اسم العمود في Database

            // Index على PaymentMethod للبحث السريع
            builder.HasIndex(p => p.PaymentMethod)
                .HasDatabaseName("IX_Payments_PaymentMethod");


            // ============================================
            // PaymentStatus Property
            // ============================================
            builder.Property(p => p.PaymentStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnName("PaymentStatus");

            // Index على PaymentMethod للبحث السريع
            builder.HasIndex(p => p.PaymentStatus)
                .HasDatabaseName("IX_Payments_PaymentStatus");

            // ============================================
            // AdditionalNotes Property (Optional)
            // ============================================
            builder.Property(p => p.AdditionalNotes)
                .IsRequired(false)
                .HasMaxLength(500)
                .HasColumnName("AdditionalNotes");
            // AdditionalNotes: ملاحظات إضافية (مثل: رقم الفاتورة)

            // ============================================
            // Appointment Relationship (One-to-One)
            // ============================================
            builder.HasOne(p => p.Appointment)
                .WithOne(a => a.Payment)
                .HasForeignKey<Payment>(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            builder.Property(p => p.AppointmentId)
                .HasColumnName("AppointmentId");

            builder.HasIndex(p => p.AppointmentId)
                .IsUnique()
                .HasFilter("[AppointmentId] IS NOT NULL")
                .HasDatabaseName("IX_Payments_AppointmentId");

            builder.Property(p => p.PatientId)
                .IsRequired()
                .HasColumnName("PatientId");

            builder.HasOne(p => p.Patient)
                .WithMany(pt => pt.Payments)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.PatientId)
                .HasDatabaseName("IX_Payments_PatientId");

            // Composite Index على PaymentDate و PaymentMethod
            builder.HasIndex(p => new { p.PaymentDate, p.PaymentMethod })
                .HasDatabaseName("IX_Payments_Date_Method");
            // مفيد للبحث عن المدفوعات في تاريخ معين بطريقة دفع معينة

            // ============================================
            // Soft Delete
            // ============================================
            builder.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IsDeleted");

            builder.Property(p => p.DeletedAt)
                .IsRequired(false)
                .HasColumnName("DeletedAt");

            // ============================================
            // Audit Fields
            // ============================================
            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt");

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false)
                .HasColumnName("UpdatedAt");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Payments_CreatedAt");

            builder.Property(p => p.DiscountAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m)
                .HasColumnName("DiscountAmount");

            builder.Property(p => p.TreatmentPlanId)
                .HasColumnName("TreatmentPlanId");

            builder.HasOne(p => p.TreatmentPlan)
                .WithMany(t => t.Invoices)
                .HasForeignKey(p => p.TreatmentPlanId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasIndex(p => p.TreatmentPlanId)
                .HasDatabaseName("IX_Payments_TreatmentPlanId");

            builder.Ignore(p => p.Subtotal);
            builder.Ignore(p => p.InvoiceTotal);
            builder.Ignore(p => p.AmountCollected);
            builder.Ignore(p => p.Balance);
            builder.Ignore(p => p.CanEditInvoice);
            builder.Ignore(p => p.CanAddInvoiceLines);
            builder.Ignore(p => p.CanReceivePayment);
            builder.Ignore(p => p.CanRefund);
        }
    }
}

