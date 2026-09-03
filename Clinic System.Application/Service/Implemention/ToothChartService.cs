namespace Clinic_System.Application.Service.Implemention;

public class ToothChartService : IToothChartService
{
    private readonly IUnitOfWork unitOfWork;

    public ToothChartService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public async Task<ToothChartEntry> CreateEntryAsync(
        int patientId,
        int toothNumber,
        ToothSurface surface,
        ToothChartPhase phase,
        ToothCondition condition,
        ToothSeverity? severity,
        string? notes,
        int? appointmentId,
        string? recordedByUserId,
        RestorationMaterial? restorationMaterial = null,
        CariesType? cariesType = null,
        IcdasCode? icdas = null,
        string? clinicalDiagnosis = null,
        string? proposedTreatment = null,
        Guid? bridgeSpanId = null,
        BridgeRole? bridgeRole = null,
        CancellationToken cancellationToken = default)
    {
        if (!FdiToothNumber.IsValid(toothNumber))
            throw new InvalidOperationException("El diente debe usar una notación FDI válida.");

        var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken);
        if (patient == null)
            throw new NotFoundException($"Patient with ID {patientId} not found.");

        if (appointmentId.HasValue)
        {
            var appointment = await unitOfWork.AppointmentsRepository.GetByIdAsync(appointmentId.Value, cancellationToken);
            if (appointment == null)
                throw new NotFoundException($"Appointment with ID {appointmentId} not found.");
            if (appointment.PatientId != patientId)
                throw new InvalidOperationException("La cita no pertenece al paciente indicado.");
        }

        var recordedAt = DateTime.UtcNow;
        var entry = new ToothChartEntry
        {
            PatientId = patientId,
            ToothNumber = toothNumber,
            Surface = surface,
            Phase = phase,
            Condition = condition,
            RestorationMaterial = restorationMaterial,
            CariesType = condition == ToothCondition.Caries ? cariesType : null,
            Icdas = condition == ToothCondition.Caries ? icdas : null,
            Severity = severity,
            ClinicalDiagnosis = clinicalDiagnosis,
            ProposedTreatment = proposedTreatment,
            Notes = notes,
            AppointmentId = appointmentId,
            BridgeSpanId = condition == ToothCondition.Bridge ? bridgeSpanId : null,
            BridgeRole = condition == ToothCondition.Bridge ? bridgeRole : null,
            RecordedByUserId = recordedByUserId,
            RecordedAt = recordedAt
        };
        await unitOfWork.ToothChartEntriesRepository.AddAsync(entry, cancellationToken);

        var summary = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(patientId, toothNumber, cancellationToken);
        if (summary == null)
        {
            summary = new ToothRecord
            {
                PatientId = patientId,
                ToothNumber = toothNumber,
                DiagnosisCondition = phase == ToothChartPhase.Diagnosis ? condition : ToothCondition.Healthy,
                TreatmentCondition = phase == ToothChartPhase.Diagnosis ? null : condition,
                Notes = notes
            };
            await unitOfWork.ToothRecordsRepository.AddAsync(summary, cancellationToken);
        }
        else
        {
            if (phase == ToothChartPhase.Diagnosis)
                summary.UpdateDiagnosis(condition, notes);
            else
                summary.UpdateTreatment(condition);
            unitOfWork.ToothRecordsRepository.Update(summary, cancellationToken);
        }

        await unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
        {
            PatientId = patientId,
            ToothNumber = toothNumber,
            Type = DentalClinicalEventType.OdontogramEntry,
            Phase = phase,
            Title = ToothChartEventText.BuildTitle(phase, toothNumber, surface, condition, restorationMaterial, cariesType, icdas),
            Description = BuildDescription(clinicalDiagnosis, proposedTreatment, notes),
            ReferenceType = nameof(ToothChartEntry),
            ReferenceId = $"{patientId}:{toothNumber}:{recordedAt:O}",
            RecordedByUserId = recordedByUserId,
            RecordedAt = recordedAt
        }, cancellationToken);

        return entry;
    }

    public async Task<IEnumerable<ToothChartEntry>> GetCurrentAsync(
        int patientId,
        string? dentition,
        int? quadrant,
        CancellationToken cancellationToken = default)
    {
        var entries = await unitOfWork.ToothChartEntriesRepository.GetByPatientAsync(patientId, cancellationToken);
        var filtered = entries.Where(x =>
            (!quadrant.HasValue || FdiToothNumber.Quadrant(x.ToothNumber) == quadrant) &&
            (string.IsNullOrWhiteSpace(dentition) ||
             (dentition.Equals("permanent", StringComparison.OrdinalIgnoreCase) && FdiToothNumber.IsPermanent(x.ToothNumber)) ||
             (dentition.Equals("deciduous", StringComparison.OrdinalIgnoreCase) && !FdiToothNumber.IsPermanent(x.ToothNumber))));

        return filtered
            .GroupBy(x => new { x.ToothNumber, x.Surface, x.Phase })
            .Select(x => x.OrderByDescending(e => e.RecordedAt).ThenByDescending(e => e.Id).First())
            .OrderBy(x => x.ToothNumber)
            .ThenBy(x => x.Surface)
            .ThenBy(x => x.Phase)
            .ToList();
    }

    public Task<IEnumerable<DentalClinicalEvent>> GetTimelineAsync(
        int patientId,
        int? toothNumber,
        CancellationToken cancellationToken = default) =>
        unitOfWork.DentalClinicalEventsRepository.GetTimelineAsync(patientId, toothNumber, cancellationToken);

    private static string? BuildDescription(string? diagnosis, string? treatment, string? notes)
    {
        var lines = new[]
        {
            string.IsNullOrWhiteSpace(diagnosis) ? null : $"Diagnóstico: {diagnosis.Trim()}",
            string.IsNullOrWhiteSpace(treatment) ? null : $"Tratamiento propuesto: {treatment.Trim()}",
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        }.Where(x => x != null);
        var text = string.Join(" | ", lines);
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }
}
