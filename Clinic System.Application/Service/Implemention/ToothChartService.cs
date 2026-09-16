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
            !x.IsVoided &&
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

    public Task<ToothChartEntry?> GetEntryAsync(long id, CancellationToken cancellationToken = default) =>
        unitOfWork.ToothChartEntriesRepository.GetByCondition(x => x.Id == id, cancellationToken);

    public async Task<ToothChartEntry> UpdateEntryAsync(
        long id,
        ToothSurface surface,
        ToothChartPhase phase,
        ToothCondition condition,
        ToothSeverity? severity,
        string? notes,
        RestorationMaterial? restorationMaterial,
        CariesType? cariesType,
        IcdasCode? icdas,
        string? clinicalDiagnosis,
        string? proposedTreatment,
        CancellationToken cancellationToken = default)
    {
        var entry = await unitOfWork.ToothChartEntriesRepository.GetByCondition(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el hallazgo {id}.");
        if (entry.IsVoided)
            throw new InvalidOperationException("No se puede editar un hallazgo anulado.");

        entry.Surface = surface;
        entry.Phase = phase;
        entry.Condition = condition;
        entry.RestorationMaterial = restorationMaterial;
        entry.CariesType = condition == ToothCondition.Caries ? cariesType : null;
        entry.Icdas = condition == ToothCondition.Caries ? icdas : null;
        entry.Severity = severity;
        entry.ClinicalDiagnosis = clinicalDiagnosis;
        entry.ProposedTreatment = proposedTreatment;
        entry.Notes = notes;
        if (condition != ToothCondition.Bridge)
        {
            entry.BridgeSpanId = null;
            entry.BridgeRole = null;
        }

        unitOfWork.ToothChartEntriesRepository.Update(entry, cancellationToken);

        var summary = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(entry.PatientId, entry.ToothNumber, cancellationToken);
        if (summary != null)
        {
            if (phase == ToothChartPhase.Diagnosis)
                summary.UpdateDiagnosis(condition, notes);
            else
                summary.UpdateTreatment(condition);
            unitOfWork.ToothRecordsRepository.Update(summary, cancellationToken);
        }

        var linked = await FindLinkedEventAsync(entry, cancellationToken);
        if (linked != null)
        {
            linked.Phase = phase;
            linked.Title = ToothChartEventText.BuildTitle(phase, entry.ToothNumber, surface, condition, restorationMaterial, cariesType, icdas);
            linked.Description = BuildDescription(clinicalDiagnosis, proposedTreatment, notes);
            linked.ReferenceId = entry.Id.ToString();
            unitOfWork.DentalClinicalEventsRepository.Update(linked, cancellationToken);
        }

        return entry;
    }

    public async Task VoidEntryAsync(
        long id,
        string? voidedByUserId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var entry = await unitOfWork.ToothChartEntriesRepository.GetByCondition(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No se encontró el hallazgo {id}.");

        entry.Void(voidedByUserId, reason);
        unitOfWork.ToothChartEntriesRepository.Update(entry, cancellationToken);

        var linked = await FindLinkedEventAsync(entry, cancellationToken);
        if (linked != null)
        {
            linked.Void(voidedByUserId);
            linked.ReferenceId = entry.Id.ToString();
            unitOfWork.DentalClinicalEventsRepository.Update(linked, cancellationToken);
        }

        await RebuildToothSummaryAsync(entry.PatientId, entry.ToothNumber, entry.Id, cancellationToken);
    }

    private async Task RebuildToothSummaryAsync(
        int patientId,
        int toothNumber,
        long excludedEntryId,
        CancellationToken cancellationToken)
    {
        var remaining = (await unitOfWork.ToothChartEntriesRepository.FindAsync(
            x => x.PatientId == patientId
                 && x.ToothNumber == toothNumber
                 && !x.IsVoided
                 && x.Id != excludedEntryId,
            cancellationToken)).ToList();

        var summary = await unitOfWork.ToothRecordsRepository.GetByPatientAndToothAsync(patientId, toothNumber, cancellationToken);
        if (summary == null)
            return;

        var latestDiagnosis = remaining
            .Where(x => x.Phase == ToothChartPhase.Diagnosis)
            .OrderByDescending(x => x.RecordedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
        var latestTreatment = remaining
            .Where(x => x.Phase != ToothChartPhase.Diagnosis)
            .OrderByDescending(x => x.RecordedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();

        summary.UpdateDiagnosis(latestDiagnosis?.Condition ?? ToothCondition.Healthy, latestDiagnosis?.Notes);
        summary.UpdateTreatment(latestTreatment?.Condition);
        unitOfWork.ToothRecordsRepository.Update(summary, cancellationToken);
    }

    private async Task<DentalClinicalEvent?> FindLinkedEventAsync(ToothChartEntry entry, CancellationToken cancellationToken)
    {
        var byId = await unitOfWork.DentalClinicalEventsRepository.GetByCondition(
            e => e.PatientId == entry.PatientId
                 && e.Type == DentalClinicalEventType.OdontogramEntry
                 && e.ReferenceId == entry.Id.ToString(),
            cancellationToken);
        if (byId != null)
            return byId;

        var events = await unitOfWork.DentalClinicalEventsRepository.FindAsync(
            e => e.PatientId == entry.PatientId
                 && e.ToothNumber == entry.ToothNumber
                 && e.Type == DentalClinicalEventType.OdontogramEntry,
            cancellationToken);
        return events
            .OrderBy(e => Math.Abs((e.RecordedAt - entry.RecordedAt).TotalMilliseconds))
            .FirstOrDefault(e => Math.Abs((e.RecordedAt - entry.RecordedAt).TotalSeconds) < 3);
    }
}
