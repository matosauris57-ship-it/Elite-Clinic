namespace Clinic_System.Application.Service.Interface
{
    public interface IDentalHistoryService
    {
        Task<DentalHistory> CreateOrUpdateAsync(
            int patientId,
            string? allergies,
            string? currentMedication,
            string? systemicDiseases,
            string? previousDentalTreatments,
            string? bloodPressure,
            string? otherDiseases,
            string? reasonForConsultation,
            string? diagnosis,
            string? clinicalObservations,
            bool hasBleedingGums,
            bool hasSensitiveTeeth,
            bool hasBruxism,
            bool isSmoker,
            string? additionalNotes,
            IEnumerable<int>? selectedConditionIds,
            CancellationToken cancellationToken = default);

        Task<DentalHistory?> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    }
}
