using Clinic_System.Core.Exceptions;
using Microsoft.Data.SqlClient;

namespace Clinic_System.Data.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly AppDbContext context;

        public UnitOfWork(AppDbContext context)
        {
            this.context = context;
        }
        IPatientRepository PatientsRepo;

        IDoctorRepository DoctorsRepo;

        IAppointmentRepository AppointmentsRepo;

        IMedicalRecordRepository MedicalRecordsRepo;

        IPaymentRepository PaymentsRepo;

        IPrescriptionRepository PrescriptionsRepo;

        IRefreshTokenRepository RefreshTokensRepo;

        IDentalHistoryRepository DentalHistoriesRepo;
        IToothRecordRepository ToothRecordsRepo;
        IToothChartEntryRepository ToothChartEntriesRepo;
        IDentalClinicalEventRepository DentalClinicalEventsRepo;
        IDentalTreatmentRepository DentalTreatmentsRepo;
        ITreatmentPlanRepository TreatmentPlansRepo;
        IInvoiceLineRepository InvoiceLinesRepo;
        ITreatmentProcedureRepository TreatmentProceduresRepo;
        IDoctorProcedurePriceRepository DoctorProcedurePricesRepo;
        IMedicalConditionRepository MedicalConditionsRepo;
        IPatientMedicalConditionRepository PatientMedicalConditionsRepo;
        IPeriodontalExamRepository PeriodontalExamsRepo;
        IPatientPrescriptionRepository PatientPrescriptionsRepo;
        IDashboardLayoutRepository DashboardLayoutsRepo;
        IEmailCampaignRepository EmailCampaignsRepo;

        public IRefreshTokenRepository RefreshTokensRepository
        {
            get
            {
                if (RefreshTokensRepo == null)
                {
                    RefreshTokensRepo = new RefreshTokenRepository(context);
                }
                return RefreshTokensRepo;
            }
        }

        public IPrescriptionRepository PrescriptionsRepository
        {
            get
            {
                if (PrescriptionsRepo == null)
                {
                    PrescriptionsRepo = new PrescriptionRepository(context);
                }
                return PrescriptionsRepo;
            }
        }

        public IPatientRepository PatientsRepository
        {
            get
            {
                if (PatientsRepo == null)
                {
                    PatientsRepo = new PatientRepository(context);
                }
                return PatientsRepo;
            }
        }

        public IDoctorRepository DoctorsRepository
        {
            get
            {
                if (DoctorsRepo == null)
                {
                    DoctorsRepo = new DoctorRepository(context);
                }
                return DoctorsRepo;
            }
        }

        public IAppointmentRepository AppointmentsRepository
        {
            get
            {
                if (AppointmentsRepo == null)
                {
                    AppointmentsRepo = new AppointmentRepository(context);
                }
                return AppointmentsRepo;
            }
        }

        public IMedicalRecordRepository MedicalRecordsRepository
        {
            get
            {
                if (MedicalRecordsRepo == null)
                {
                    MedicalRecordsRepo = new MedicalRecordRepository(context);
                }
                return MedicalRecordsRepo;
            }
        }

        public IPaymentRepository PaymentsRepository
        {
            get
            {
                if (PaymentsRepo == null)
                {
                    PaymentsRepo = new PaymentRepository(context);
                }
                return PaymentsRepo;
            }
        }

        public IDentalHistoryRepository DentalHistoriesRepository
        {
            get
            {
                DentalHistoriesRepo ??= new DentalHistoryRepository(context);
                return DentalHistoriesRepo;
            }
        }

        public IToothRecordRepository ToothRecordsRepository
        {
            get
            {
                ToothRecordsRepo ??= new ToothRecordRepository(context);
                return ToothRecordsRepo;
            }
        }

        public IToothChartEntryRepository ToothChartEntriesRepository
        {
            get
            {
                ToothChartEntriesRepo ??= new ToothChartEntryRepository(context);
                return ToothChartEntriesRepo;
            }
        }

        public IDentalClinicalEventRepository DentalClinicalEventsRepository
        {
            get
            {
                DentalClinicalEventsRepo ??= new DentalClinicalEventRepository(context);
                return DentalClinicalEventsRepo;
            }
        }

        public IDentalTreatmentRepository DentalTreatmentsRepository
        {
            get
            {
                DentalTreatmentsRepo ??= new DentalTreatmentRepository(context);
                return DentalTreatmentsRepo;
            }
        }

        public ITreatmentPlanRepository TreatmentPlansRepository
        {
            get
            {
                TreatmentPlansRepo ??= new TreatmentPlanRepository(context);
                return TreatmentPlansRepo;
            }
        }

        public IInvoiceLineRepository InvoiceLinesRepository
        {
            get
            {
                InvoiceLinesRepo ??= new InvoiceLineRepository(context);
                return InvoiceLinesRepo;
            }
        }

        public ITreatmentProcedureRepository TreatmentProceduresRepository
        {
            get
            {
                TreatmentProceduresRepo ??= new TreatmentProcedureRepository(context);
                return TreatmentProceduresRepo;
            }
        }

        public IDoctorProcedurePriceRepository DoctorProcedurePricesRepository
        {
            get
            {
                DoctorProcedurePricesRepo ??= new DoctorProcedurePriceRepository(context);
                return DoctorProcedurePricesRepo;
            }
        }

        public IMedicalConditionRepository MedicalConditionsRepository
        {
            get
            {
                MedicalConditionsRepo ??= new MedicalConditionRepository(context);
                return MedicalConditionsRepo;
            }
        }

        public IPatientMedicalConditionRepository PatientMedicalConditionsRepository
        {
            get
            {
                PatientMedicalConditionsRepo ??= new PatientMedicalConditionRepository(context);
                return PatientMedicalConditionsRepo;
            }
        }

        public IPeriodontalExamRepository PeriodontalExamsRepository
        {
            get
            {
                PeriodontalExamsRepo ??= new PeriodontalExamRepository(context);
                return PeriodontalExamsRepo;
            }
        }

        public IPatientPrescriptionRepository PatientPrescriptionsRepository
        {
            get
            {
                PatientPrescriptionsRepo ??= new PatientPrescriptionRepository(context);
                return PatientPrescriptionsRepo;
            }
        }

        public IDashboardLayoutRepository DashboardLayoutsRepository
        {
            get
            {
                DashboardLayoutsRepo ??= new DashboardLayoutRepository(context);
                return DashboardLayoutsRepo;
            }
        }

        public IEmailCampaignRepository EmailCampaignsRepository
        {
            get
            {
                EmailCampaignsRepo ??= new EmailCampaignRepository(context);
                return EmailCampaignsRepo;
            }
        }

        public void Dispose() => context.Dispose();

        //public Task<int> SaveAsync() => context.SaveChangesAsync();

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // بنتأكد إن الإيرور جاي من SQL Server
                if (ex.InnerException is SqlException sqlEx)
                {
                    // أرقام 2601 و 2627 و 1505 دي أرقام الـ Unique Constraint Violation في SQL Server
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627 || sqlEx.Number == 1505)
                    {
                        // هنا بنترجم إيرور الـ EF Core لإيرور يفهمه الـ Application
                        throw new UniqueConstraintViolationException("A database unique constraint was violated.", ex);
                    }
                }

                // لو إيرور داتابيز تاني، ارميه زي ما هو (أو اعمله ترجمة لـ Exception تاني لو حابب)
                throw;
            }
        }
    }
}
