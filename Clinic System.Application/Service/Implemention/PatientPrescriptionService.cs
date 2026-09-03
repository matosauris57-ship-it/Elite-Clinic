using Clinic_System.Core.Catalog;

namespace Clinic_System.Application.Service.Implemention;

public class PatientPrescriptionService : IPatientPrescriptionService
{
    private readonly IUnitOfWork unitOfWork;

    public PatientPrescriptionService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public IReadOnlyList<PrescriptionTemplateDTO> ListTemplates() =>
        DentalPrescriptionCatalog.All.Select(MapTemplate).ToList();

    public Task<IReadOnlyList<PatientPrescription>> ListByPatientAsync(int patientId, CancellationToken cancellationToken = default) =>
        unitOfWork.PatientPrescriptionsRepository.GetByPatientAsync(patientId, cancellationToken);

    public async Task<PatientPrescription> GetAsync(int prescriptionId, CancellationToken cancellationToken = default)
    {
        var prescription = await unitOfWork.PatientPrescriptionsRepository.GetWithItemsAsync(prescriptionId, cancellationToken);
        return prescription ?? throw new NotFoundException($"Receta {prescriptionId} no encontrada.");
    }

    public async Task<PatientPrescription> CreateAsync(
        int patientId,
        PatientPrescriptionUpsertDTO request,
        int? doctorId,
        string? recordedByUserId,
        CancellationToken cancellationToken = default)
    {
        var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken)
            ?? throw new NotFoundException($"Paciente {patientId} no encontrado.");

        var items = BuildItems(request);
        if (items.Count == 0)
            throw new InvalidOperationException("La receta debe incluir al menos un medicamento.");

        var prescription = new PatientPrescription
        {
            PatientId = patient.Id,
            DoctorId = request.DoctorId ?? doctorId,
            IssuedAt = request.IssuedAt ?? DateTime.Now,
            Diagnosis = Trim(request.Diagnosis, 500),
            Notes = Trim(request.Notes, 2000),
            RecordedByUserId = recordedByUserId
        };
        foreach (var item in items)
            prescription.Items.Add(item);

        await unitOfWork.PatientPrescriptionsRepository.AddAsync(prescription, cancellationToken);
        await unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
        {
            PatientId = patientId,
            Type = DentalClinicalEventType.Prescription,
            Title = "Receta médica emitida",
            Description = Preview(items),
            ReferenceType = nameof(PatientPrescription),
            RecordedByUserId = recordedByUserId,
            RecordedAt = prescription.IssuedAt
        }, cancellationToken);

        return prescription;
    }

    public async Task<PatientPrescription> UpdateAsync(
        int prescriptionId,
        PatientPrescriptionUpsertDTO request,
        int? doctorId,
        string? recordedByUserId,
        CancellationToken cancellationToken = default)
    {
        var prescription = await GetAsync(prescriptionId, cancellationToken);
        var items = BuildItems(request);
        if (items.Count == 0)
            throw new InvalidOperationException("La receta debe incluir al menos un medicamento.");

        prescription.DoctorId = request.DoctorId ?? doctorId ?? prescription.DoctorId;
        if (request.IssuedAt.HasValue)
            prescription.IssuedAt = request.IssuedAt.Value;
        prescription.Diagnosis = Trim(request.Diagnosis, 500);
        prescription.Notes = Trim(request.Notes, 2000);
        prescription.RecordedByUserId = recordedByUserId ?? prescription.RecordedByUserId;

        foreach (var existing in prescription.Items.ToList())
            unitOfWork.PatientPrescriptionsRepository.RemoveItem(existing);
        prescription.Items.Clear();
        foreach (var item in items)
            prescription.Items.Add(item);

        unitOfWork.PatientPrescriptionsRepository.Update(prescription, cancellationToken);
        await unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
        {
            PatientId = prescription.PatientId,
            Type = DentalClinicalEventType.Prescription,
            Title = $"Receta actualizada ({prescription.IssuedAt:yyyy-MM-dd})",
            Description = Preview(items),
            ReferenceType = nameof(PatientPrescription),
            ReferenceId = prescription.Id.ToString(),
            RecordedByUserId = recordedByUserId,
            RecordedAt = DateTime.Now
        }, cancellationToken);

        return prescription;
    }

    public async Task DeleteAsync(int prescriptionId, CancellationToken cancellationToken = default)
    {
        var prescription = await GetAsync(prescriptionId, cancellationToken);
        prescription.SoftDelete();
        unitOfWork.PatientPrescriptionsRepository.Update(prescription, cancellationToken);
    }

    public static PatientPrescriptionSummaryDTO MapSummary(PatientPrescription prescription)
    {
        var items = prescription.Items.OrderBy(x => x.SortOrder).ToList();
        return new PatientPrescriptionSummaryDTO
        {
            Id = prescription.Id,
            PatientId = prescription.PatientId,
            DoctorId = prescription.DoctorId,
            DoctorName = prescription.Doctor?.FullName,
            IssuedAt = prescription.IssuedAt,
            Diagnosis = prescription.Diagnosis,
            ItemCount = items.Count,
            MedicationPreview = Preview(items)
        };
    }

    public static async Task<PatientPrescriptionDTO> MapDetailAsync(
        PatientPrescription prescription,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var history = await unitOfWork.DentalHistoriesRepository.GetByPatientIdAsync(prescription.PatientId, cancellationToken);
        var dto = new PatientPrescriptionDTO
        {
            Id = prescription.Id,
            PatientId = prescription.PatientId,
            PatientName = prescription.Patient?.FullName ?? string.Empty,
            PatientNationalId = prescription.Patient?.NationalId,
            PatientPhone = prescription.Patient?.Phone,
            PatientDateOfBirth = prescription.Patient?.DateOfBirth,
            DoctorId = prescription.DoctorId,
            DoctorName = prescription.Doctor?.FullName,
            DoctorSpecialization = prescription.Doctor?.Specialization,
            IssuedAt = prescription.IssuedAt,
            Diagnosis = prescription.Diagnosis,
            Notes = prescription.Notes,
            Allergies = history?.Allergies,
            CurrentMedication = history?.CurrentMedication,
            ItemCount = prescription.Items.Count,
            MedicationPreview = Preview(prescription.Items),
            Items = prescription.Items.OrderBy(x => x.SortOrder).Select(MapItem).ToList()
        };
        return dto;
    }

    public static PrescriptionTemplateDTO MapTemplate(PrescriptionTemplate template) => new()
    {
        Key = template.Key,
        Name = template.Name,
        Category = template.Category,
        Indication = template.Indication,
        Lines = template.Lines.Select((line, index) => new PatientPrescriptionItemDTO
        {
            SortOrder = index,
            TemplateKey = template.Key,
            MedicationName = line.MedicationName,
            Dosage = line.Dosage,
            Frequency = line.Frequency,
            DurationDays = line.DurationDays,
            SpecialInstructions = line.SpecialInstructions
        }).ToList()
    };

    private static List<PatientPrescriptionItem> BuildItems(PatientPrescriptionUpsertDTO request)
    {
        var lines = new List<PatientPrescriptionItemDTO>();
        foreach (var key in request.TemplateKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var template = DentalPrescriptionCatalog.Find(key);
            if (template == null)
                continue;
            lines.AddRange(MapTemplate(template).Lines);
        }

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.MedicationName))
                continue;
            lines.Add(item);
        }

        return lines.Select((item, index) => new PatientPrescriptionItem
        {
            SortOrder = index,
            TemplateKey = Trim(item.TemplateKey, 80),
            MedicationName = item.MedicationName.Trim(),
            Dosage = string.IsNullOrWhiteSpace(item.Dosage) ? "Según indicación" : item.Dosage.Trim(),
            Frequency = string.IsNullOrWhiteSpace(item.Frequency) ? "Según indicación" : item.Frequency.Trim(),
            DurationDays = item.DurationDays is < 1 or > 60 ? 7 : item.DurationDays,
            SpecialInstructions = Trim(item.SpecialInstructions, 500)
        }).ToList();
    }

    private static PatientPrescriptionItemDTO MapItem(PatientPrescriptionItem item) => new()
    {
        Id = item.Id,
        SortOrder = item.SortOrder,
        TemplateKey = item.TemplateKey,
        MedicationName = item.MedicationName,
        Dosage = item.Dosage,
        Frequency = item.Frequency,
        DurationDays = item.DurationDays,
        SpecialInstructions = item.SpecialInstructions
    };

    private static string Preview(IEnumerable<PatientPrescriptionItem> items) =>
        string.Join(" · ", items.OrderBy(x => x.SortOrder).Select(x => x.MedicationName).Where(x => !string.IsNullOrWhiteSpace(x)).Take(4));

    private static string Preview(IEnumerable<PatientPrescriptionItemDTO> items) =>
        string.Join(" · ", items.Select(x => x.MedicationName).Where(x => !string.IsNullOrWhiteSpace(x)).Take(4));

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
