namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IPatientRepository : IGenericRepository<Patient>
    {
        Task<Patient?> GetPatientByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Patient?>> GetPatientsWithAppointmentsAsync(
            Expression<Func<Appointment, bool>> appointmentPredicate);
        Task<Patient?> GetPatientWithAppointmentsByIdAsync(int Id, CancellationToken cancellationToken = default);

        Task<string?> GetPatientUserIdAsync(int patientId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Patient?>> GetPatientsByNameAsync(string FullName, CancellationToken cancellationToken = default);
        Task<Patient?> GetPatientByPhoneAsync(string Phone, CancellationToken cancellationToken = default);
        Task<IEnumerable<Patient?>> GetAllForAdminAsync(bool includeInactive, CancellationToken cancellationToken = default);
        Task<Patient?> GetByIdIncludingDeletedAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Patient>> GetForBirthdayEmailsAsync(int year, CancellationToken cancellationToken = default);
        Task<List<Patient>> GetEmailCampaignAudienceAsync(CancellationToken cancellationToken = default);
        Task<(int WithEmail, int OptedOut, int Invalid, int Eligible)> CountEmailCampaignAudienceAsync(CancellationToken cancellationToken = default);
    }
}
