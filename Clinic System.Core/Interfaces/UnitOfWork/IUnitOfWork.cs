namespace Clinic_System.Core.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IPatientRepository PatientsRepository { get; }
        IDoctorRepository DoctorsRepository { get; }
        IAppointmentRepository AppointmentsRepository { get; }
        IMedicalRecordRepository MedicalRecordsRepository { get; }
        IPaymentRepository PaymentsRepository { get; }
        IPrescriptionRepository PrescriptionsRepository { get; }
        IRefreshTokenRepository RefreshTokensRepository { get; }
        IDentalHistoryRepository DentalHistoriesRepository { get; }
        IToothRecordRepository ToothRecordsRepository { get; }
        IToothChartEntryRepository ToothChartEntriesRepository { get; }
        IDentalClinicalEventRepository DentalClinicalEventsRepository { get; }
        IDentalTreatmentRepository DentalTreatmentsRepository { get; }
        ITreatmentPlanRepository TreatmentPlansRepository { get; }
        IInvoiceLineRepository InvoiceLinesRepository { get; }
        ITreatmentProcedureRepository TreatmentProceduresRepository { get; }
        IDoctorProcedurePriceRepository DoctorProcedurePricesRepository { get; }
        IMedicalConditionRepository MedicalConditionsRepository { get; }
        IPatientMedicalConditionRepository PatientMedicalConditionsRepository { get; }
        IPeriodontalExamRepository PeriodontalExamsRepository { get; }
        IPatientPrescriptionRepository PatientPrescriptionsRepository { get; }
        IDashboardLayoutRepository DashboardLayoutsRepository { get; }
        IEmailCampaignRepository EmailCampaignsRepository { get; }
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }
}
