namespace Clinic_System.Application.Service.Implemention
{
    public class DentalHistoryService : IDentalHistoryService
    {
        private readonly IUnitOfWork unitOfWork;

        public DentalHistoryService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<DentalHistory> CreateOrUpdateAsync(
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
            CancellationToken cancellationToken = default)
        {
            var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient == null)
                throw new NotFoundException($"Patient with ID {patientId} not found.");

            var existing = await unitOfWork.DentalHistoriesRepository.GetByPatientIdAsync(patientId, cancellationToken);
            DentalHistory history;

            if (existing == null)
            {
                history = new DentalHistory
                {
                    PatientId = patientId,
                    Allergies = allergies,
                    CurrentMedication = currentMedication,
                    SystemicDiseases = systemicDiseases,
                    PreviousDentalTreatments = previousDentalTreatments,
                    BloodPressure = bloodPressure,
                    OtherDiseases = otherDiseases,
                    ReasonForConsultation = reasonForConsultation,
                    Diagnosis = diagnosis,
                    ClinicalObservations = clinicalObservations,
                    HasBleedingGums = hasBleedingGums,
                    HasSensitiveTeeth = hasSensitiveTeeth,
                    HasBruxism = hasBruxism,
                    IsSmoker = isSmoker,
                    AdditionalNotes = additionalNotes
                };
                await unitOfWork.DentalHistoriesRepository.AddAsync(history, cancellationToken);
            }
            else
            {
                existing.Update(allergies, currentMedication, systemicDiseases, previousDentalTreatments,
                    bloodPressure, otherDiseases, reasonForConsultation, diagnosis, clinicalObservations,
                    hasBleedingGums, hasSensitiveTeeth, hasBruxism, isSmoker, additionalNotes);
                unitOfWork.DentalHistoriesRepository.Update(existing, cancellationToken);
                history = existing;
            }

            if (selectedConditionIds != null)
            {
                await unitOfWork.PatientMedicalConditionsRepository.SyncPatientConditionsAsync(
                    patientId, selectedConditionIds, cancellationToken);
            }

            return history;
        }

        public Task<DentalHistory?> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
            => unitOfWork.DentalHistoriesRepository.GetByPatientIdAsync(patientId, cancellationToken);
    }
}
