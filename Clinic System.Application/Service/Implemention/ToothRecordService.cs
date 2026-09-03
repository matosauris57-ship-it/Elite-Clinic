namespace Clinic_System.Application.Service.Implemention
{
    public class ToothRecordService : IToothRecordService
    {
        private readonly IUnitOfWork unitOfWork;

        public ToothRecordService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ToothRecord> UpsertDiagnosisAsync(int patientId, int toothNumber, ToothCondition condition, string? notes, CancellationToken cancellationToken = default)
        {
            ValidateToothNumber(toothNumber);
            await EnsurePatientExists(patientId, cancellationToken);

            var existing = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(patientId, toothNumber, cancellationToken);
            if (existing == null)
            {
                var record = new ToothRecord
                {
                    PatientId = patientId,
                    ToothNumber = toothNumber,
                    DiagnosisCondition = condition,
                    Notes = notes
                };
                await unitOfWork.ToothRecordsRepository.AddAsync(record, cancellationToken);
                return record;
            }

            existing.UpdateDiagnosis(condition, notes);
            unitOfWork.ToothRecordsRepository.Update(existing, cancellationToken);
            return existing;
        }

        public async Task<ToothRecord> UpsertTreatmentAsync(int patientId, int toothNumber, ToothCondition? condition, CancellationToken cancellationToken = default)
        {
            ValidateToothNumber(toothNumber);
            await EnsurePatientExists(patientId, cancellationToken);

            var existing = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(patientId, toothNumber, cancellationToken);
            if (existing == null)
            {
                var record = new ToothRecord
                {
                    PatientId = patientId,
                    ToothNumber = toothNumber,
                    DiagnosisCondition = ToothCondition.Healthy,
                    TreatmentCondition = condition
                };
                await unitOfWork.ToothRecordsRepository.AddAsync(record, cancellationToken);
                return record;
            }

            existing.UpdateTreatment(condition);
            unitOfWork.ToothRecordsRepository.Update(existing, cancellationToken);
            return existing;
        }

        public async Task BatchUpsertAsync(int patientId, IEnumerable<OdontogramToothInput> teeth, CancellationToken cancellationToken = default)
        {
            await EnsurePatientExists(patientId, cancellationToken);

            foreach (var tooth in teeth)
            {
                ValidateToothNumber(tooth.ToothNumber);
                var existing = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(patientId, tooth.ToothNumber, cancellationToken);
                if (existing == null)
                {
                    await unitOfWork.ToothRecordsRepository.AddAsync(new ToothRecord
                    {
                        PatientId = patientId,
                        ToothNumber = tooth.ToothNumber,
                        DiagnosisCondition = tooth.DiagnosisCondition,
                        TreatmentCondition = tooth.TreatmentCondition,
                        Notes = tooth.Notes
                    }, cancellationToken);
                }
                else
                {
                    existing.DiagnosisCondition = tooth.DiagnosisCondition;
                    existing.TreatmentCondition = tooth.TreatmentCondition;
                    if (tooth.Notes != null) existing.Notes = tooth.Notes;
                    existing.UpdatedAt = DateTime.Now;
                    unitOfWork.ToothRecordsRepository.Update(existing, cancellationToken);
                }
            }
        }

        public Task<IEnumerable<ToothRecord>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
            => unitOfWork.ToothRecordsRepository.GetByPatientIdAsync(patientId, cancellationToken);

        private static void ValidateToothNumber(int toothNumber)
        {
            if (!FdiToothNumber.IsValid(toothNumber))
                throw new InvalidOperationException("El diente debe usar una notación FDI válida.");
        }

        private async Task EnsurePatientExists(int patientId, CancellationToken cancellationToken)
        {
            var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient == null)
                throw new NotFoundException($"Patient with ID {patientId} not found.");
        }
    }
}
