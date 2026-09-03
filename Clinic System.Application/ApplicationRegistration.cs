namespace Clinic_System.Application
{
    public static class ApplicationRegistration
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            var assembly = typeof(ApplicationAssemblyReference).Assembly;

            // AutoMapper & MediatR & FluentValidation
            services.AddAutoMapper(assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));
            services.AddValidatorsFromAssembly(assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Core Business Services
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            services.AddScoped<IDentalHistoryService, DentalHistoryService>();
            services.AddScoped<IToothRecordService, ToothRecordService>();
            services.AddScoped<IToothChartService, ToothChartService>();
            services.AddScoped<IPeriodontalExamService, PeriodontalExamService>();
            services.AddScoped<IPatientPrescriptionService, PatientPrescriptionService>();
            services.AddScoped<IDentalTreatmentService, DentalTreatmentService>();
            services.AddScoped<ITreatmentPlanService, TreatmentPlanService>();
            services.AddScoped<IInvoiceLineService, InvoiceLineService>();
            services.AddScoped<ITreatmentProcedureService, TreatmentProcedureService>();
            services.AddScoped<IMedicalConditionService, MedicalConditionService>();
            services.AddScoped<IDashboardLayoutService, DashboardLayoutService>();

            return services;
        }
    }
}
