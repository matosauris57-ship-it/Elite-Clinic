namespace Clinic_System.Application.Service.Implemention
{
    public class DentalTreatmentService : IDentalTreatmentService
    {
        private readonly IUnitOfWork unitOfWork;

        public DentalTreatmentService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<DentalTreatment> CreateAsync(
            int patientId,
            string procedureName,
            decimal cost,
            int? appointmentId,
            int? toothNumber,
            ToothSurface? toothSurface,
            int? treatmentProcedureId,
            string? procedureDetails,
            string? recordedByUserId,
            CancellationToken cancellationToken = default)
        {
            var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient == null)
                throw new NotFoundException($"Patient with ID {patientId} not found.");

            if (appointmentId.HasValue)
            {
                var appointment = await unitOfWork.AppointmentsRepository.GetByIdAsync(appointmentId.Value, cancellationToken);
                if (appointment == null)
                    throw new NotFoundException($"Appointment with ID {appointmentId} not found.");
                if (appointment.PatientId != patientId)
                    throw new InvalidOperationException("Appointment does not belong to the specified patient.");
            }

            if (toothNumber.HasValue && !FdiToothNumber.IsValid(toothNumber.Value))
                throw new InvalidOperationException("El diente debe usar una notación FDI válida.");

            if (treatmentProcedureId.HasValue &&
                await unitOfWork.TreatmentProceduresRepository.GetByIdAsync(treatmentProcedureId.Value, cancellationToken) == null)
                throw new NotFoundException($"Treatment procedure with ID {treatmentProcedureId} not found.");

            int? toothRecordId = null;
            if (toothNumber.HasValue)
            {
                var tooth = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(patientId, toothNumber.Value, cancellationToken);
                toothRecordId = tooth?.Id;
            }

            var treatment = new DentalTreatment
            {
                PatientId = patientId,
                AppointmentId = appointmentId,
                ToothRecordId = toothRecordId,
                ToothNumber = toothNumber,
                ToothSurface = toothSurface,
                TreatmentProcedureId = treatmentProcedureId,
                ProcedureName = procedureName,
                ProcedureDetails = procedureDetails,
                Cost = cost,
                Status = DentalTreatmentStatus.Planned
            };

            await unitOfWork.DentalTreatmentsRepository.AddAsync(treatment, cancellationToken);
            await AddEventAsync(treatment, "Tratamiento creado", "Estado: planificado.", recordedByUserId, cancellationToken);
            return treatment;
        }

        public async Task<DentalTreatment> StartAsync(
            int treatmentId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var treatment = await GetByIdAsync(treatmentId, cancellationToken);
            treatment.Start();
            unitOfWork.DentalTreatmentsRepository.Update(treatment, cancellationToken);
            await AddEventAsync(treatment, "Tratamiento iniciado", treatment.ProcedureDetails, recordedByUserId, cancellationToken);
            return treatment;
        }

        public async Task<DentalTreatment> CompleteAsync(
            int treatmentId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var treatment = await unitOfWork.DentalTreatmentsRepository.GetByIdAsync(treatmentId, cancellationToken);
            if (treatment == null)
                throw new NotFoundException($"Dental treatment with ID {treatmentId} not found.");

            treatment.Complete();
            unitOfWork.DentalTreatmentsRepository.Update(treatment, cancellationToken);
            await AddEventAsync(treatment, "Tratamiento completado", treatment.ProcedureDetails, recordedByUserId, cancellationToken);
            return treatment;
        }

        public Task<IEnumerable<DentalTreatment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
            => unitOfWork.DentalTreatmentsRepository.GetByPatientIdAsync(patientId, cancellationToken);

        public Task<IEnumerable<DentalTreatment>> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
            => unitOfWork.DentalTreatmentsRepository.GetByAppointmentIdAsync(appointmentId, cancellationToken);

        public Task<(List<DentalTreatment> Items, int TotalCount, Dictionary<DentalTreatmentStatus, int> StatusCounts)> GetAllForAdminAsync(
            string? search,
            IReadOnlyCollection<DentalTreatmentStatus>? statuses,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            => unitOfWork.DentalTreatmentsRepository.GetAllForAdminAsync(
                search, statuses, fromDate, toDate, pageNumber, pageSize, cancellationToken);

        public async Task<DentalTreatment> GetByIdAsync(int treatmentId, CancellationToken cancellationToken = default)
        {
            var treatment = await unitOfWork.DentalTreatmentsRepository.GetByIdWithPatientAsync(treatmentId, cancellationToken);
            if (treatment == null)
                throw new NotFoundException($"Dental treatment with ID {treatmentId} not found.");
            return treatment;
        }

        public async Task<DentalTreatment> UpdateAsync(
            int treatmentId,
            string procedureName,
            decimal cost,
            int? toothNumber,
            ToothSurface? toothSurface,
            int? treatmentProcedureId,
            string? procedureDetails,
            CancellationToken cancellationToken = default)
        {
            var treatment = await GetByIdAsync(treatmentId, cancellationToken);

            if (treatment.Status is DentalTreatmentStatus.Completed or DentalTreatmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot update a completed or cancelled treatment.");

            if (toothNumber.HasValue && !FdiToothNumber.IsValid(toothNumber.Value))
                throw new InvalidOperationException("El diente debe usar una notación FDI válida.");

            if (treatmentProcedureId.HasValue &&
                await unitOfWork.TreatmentProceduresRepository.GetByIdAsync(treatmentProcedureId.Value, cancellationToken) == null)
                throw new NotFoundException($"Treatment procedure with ID {treatmentProcedureId} not found.");

            int? toothRecordId = null;
            if (toothNumber.HasValue)
            {
                var tooth = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(treatment.PatientId, toothNumber.Value, cancellationToken);
                toothRecordId = tooth?.Id;
            }

            treatment.ProcedureName = procedureName.Trim();
            treatment.Cost = cost;
            treatment.ToothNumber = toothNumber;
            treatment.ToothSurface = toothSurface;
            treatment.TreatmentProcedureId = treatmentProcedureId;
            treatment.ToothRecordId = toothRecordId;
            treatment.ProcedureDetails = procedureDetails;

            unitOfWork.DentalTreatmentsRepository.Update(treatment, cancellationToken);
            return treatment;
        }

        public async Task<DentalTreatment> CancelAsync(
            int treatmentId, string? reason, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var treatment = await GetByIdAsync(treatmentId, cancellationToken);
            treatment.Cancel(reason);
            unitOfWork.DentalTreatmentsRepository.Update(treatment, cancellationToken);
            await AddEventAsync(treatment, "Tratamiento cancelado", reason, recordedByUserId, cancellationToken);
            return treatment;
        }

        public async Task SoftDeleteAsync(int treatmentId, CancellationToken cancellationToken = default)
        {
            var treatment = await GetByIdAsync(treatmentId, cancellationToken);
            treatment.IsDeleted = true;
            treatment.DeletedAt = DateTime.Now;
            unitOfWork.DentalTreatmentsRepository.Update(treatment, cancellationToken);
        }

        private Task AddEventAsync(
            DentalTreatment treatment, string title, string? description, string? recordedByUserId,
            CancellationToken cancellationToken)
        {
            var recordedAt = DateTime.UtcNow;
            return unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
            {
                PatientId = treatment.PatientId,
                ToothNumber = treatment.ToothNumber,
                Type = DentalClinicalEventType.Treatment,
                Phase = treatment.Status == DentalTreatmentStatus.Completed
                    ? ToothChartPhase.Completed
                    : ToothChartPhase.Planned,
                Title = title,
                Description = string.IsNullOrWhiteSpace(description)
                    ? treatment.ProcedureName
                    : $"{treatment.ProcedureName}. {description}",
                ReferenceType = nameof(DentalTreatment),
                ReferenceId = treatment.Id > 0 ? treatment.Id.ToString() : $"patient:{treatment.PatientId}:{recordedAt:O}",
                RecordedByUserId = recordedByUserId,
                RecordedAt = recordedAt
            }, cancellationToken);
        }
    }
}
