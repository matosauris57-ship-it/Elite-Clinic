namespace Clinic_System.Application.Service.Implemention;

public class PeriodontalExamService : IPeriodontalExamService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IToothChartService toothChartService;

    public PeriodontalExamService(IUnitOfWork unitOfWork, IToothChartService toothChartService)
    {
        this.unitOfWork = unitOfWork;
        this.toothChartService = toothChartService;
    }

    public async Task<IReadOnlyList<PeriodontalExam>> ListByPatientAsync(int patientId, CancellationToken cancellationToken = default) =>
        await unitOfWork.PeriodontalExamsRepository.GetSummariesByPatientAsync(patientId, cancellationToken);

    public async Task<PeriodontalExam> GetChartAsync(int examId, CancellationToken cancellationToken = default)
    {
        var exam = await unitOfWork.PeriodontalExamsRepository.GetWithChartAsync(examId, cancellationToken);
        return exam ?? throw new NotFoundException($"Periodontal exam {examId} was not found.");
    }

    public async Task<PeriodontalExam> CreateAsync(
        int patientId,
        bool copyLatest,
        int? doctorId,
        string? recordedByUserId,
        CancellationToken cancellationToken = default)
    {
        var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken);
        if (patient == null)
            throw new NotFoundException($"Patient with ID {patientId} not found.");

        var latest = copyLatest
            ? await unitOfWork.PeriodontalExamsRepository.GetLatestByPatientAsync(patientId, cancellationToken)
            : null;

        var exam = new PeriodontalExam
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ExaminedAt = DateTime.UtcNow,
            RecordedByUserId = recordedByUserId,
            Notes = latest?.Notes
        };

        if (latest != null)
        {
            foreach (var tooth in latest.Teeth)
                exam.Teeth.Add(CloneTooth(tooth));
        }

        await ApplyOdontogramStatusAsync(exam, patientId, cancellationToken);
        ApplyIndices(exam);

        await unitOfWork.PeriodontalExamsRepository.AddAsync(exam, cancellationToken);
        await unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
        {
            PatientId = patientId,
            Type = DentalClinicalEventType.PeriodontalExam,
            Title = "Nueva evaluación periodontal",
            Description = copyLatest ? "Copia de la evaluación anterior como punto de partida." : "Evaluación periodontal en blanco.",
            ReferenceType = nameof(PeriodontalExam),
            RecordedByUserId = recordedByUserId,
            RecordedAt = exam.ExaminedAt
        }, cancellationToken);

        return exam;
    }

    public async Task<PeriodontalExam> SaveAsync(
        int examId,
        PeriodontalExamUpsertDTO chart,
        string? recordedByUserId,
        CancellationToken cancellationToken = default)
    {
        var exam = await unitOfWork.PeriodontalExamsRepository.GetWithChartAsync(examId, cancellationToken)
            ?? throw new NotFoundException($"Periodontal exam {examId} was not found.");

        var latest = (await unitOfWork.PeriodontalExamsRepository.GetSummariesByPatientAsync(exam.PatientId, cancellationToken)).FirstOrDefault();
        if (latest == null || latest.Id != exam.Id)
            throw new InvalidOperationException("Solo la evaluación periodontal más reciente se puede editar.");

        exam.Notes = chart.Notes;
        exam.RecordedByUserId = recordedByUserId ?? exam.RecordedByUserId;
        if (chart.ExaminedAt.HasValue)
            exam.ExaminedAt = chart.ExaminedAt.Value;

        foreach (var existing in exam.Teeth.ToList())
            unitOfWork.PeriodontalExamsRepository.RemoveTooth(existing);

        exam.Teeth.Clear();
        foreach (var toothDto in chart.Teeth.Where(x => FdiToothNumber.IsPermanent(x.ToothNumber)))
            exam.Teeth.Add(MapTooth(toothDto));

        ApplyIndices(exam);

        await unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
        {
            PatientId = exam.PatientId,
            Type = DentalClinicalEventType.PeriodontalExam,
            Title = $"Periodontograma actualizado ({exam.ExaminedAt:yyyy-MM-dd})",
            Description = exam.Notes,
            ReferenceType = nameof(PeriodontalExam),
            ReferenceId = exam.Id.ToString(),
            RecordedByUserId = recordedByUserId,
            RecordedAt = DateTime.UtcNow
        }, cancellationToken);

        return exam;
    }

    public async Task DeleteAsync(int examId, CancellationToken cancellationToken = default)
    {
        var exam = await unitOfWork.PeriodontalExamsRepository.GetByIdAsync(examId, cancellationToken)
            ?? throw new NotFoundException($"Periodontal exam {examId} was not found.");
        exam.IsDeleted = true;
        exam.DeletedAt = DateTime.UtcNow;
        unitOfWork.PeriodontalExamsRepository.Update(exam, cancellationToken);
    }

    public async Task<PeriodontalCompareDTO> CompareAsync(
        int previousExamId,
        int currentExamId,
        CancellationToken cancellationToken = default)
    {
        var previous = await GetChartAsync(previousExamId, cancellationToken);
        var current = await GetChartAsync(currentExamId, cancellationToken);
        if (previous.PatientId != current.PatientId)
            throw new InvalidOperationException("Las evaluaciones deben pertenecer al mismo paciente.");

        var previousByTooth = previous.Teeth.ToDictionary(x => x.ToothNumber);
        var currentByTooth = current.Teeth.ToDictionary(x => x.ToothNumber);
        var teeth = PeriodontalCalculations.PermanentArchOrder
            .Where(n => previousByTooth.ContainsKey(n) || currentByTooth.ContainsKey(n))
            .Select(n =>
            {
                previousByTooth.TryGetValue(n, out var prevTooth);
                currentByTooth.TryGetValue(n, out var currTooth);
                var prevPd = MaxPd(prevTooth);
                var currPd = MaxPd(currTooth);
                var prevCal = MaxCal(prevTooth);
                var currCal = MaxCal(currTooth);
                return new PeriodontalToothCompareDTO
                {
                    ToothNumber = n,
                    PreviousMaxProbingDepth = prevPd,
                    CurrentMaxProbingDepth = currPd,
                    ProbingDepthChange = currPd.HasValue && prevPd.HasValue ? currPd - prevPd : null,
                    PreviousMaxCal = prevCal,
                    CurrentMaxCal = currCal,
                    CalChange = currCal.HasValue && prevCal.HasValue ? currCal - prevCal : null
                };
            })
            .ToList();

        return new PeriodontalCompareDTO
        {
            Previous = MapSummary(previous, isLatest: false),
            Current = MapSummary(current, isLatest: true),
            BleedingPercentChange = current.BleedingPercent.HasValue && previous.BleedingPercent.HasValue
                ? current.BleedingPercent - previous.BleedingPercent
                : null,
            SitesDeepGe5Change = current.SitesDeepGe5 - previous.SitesDeepGe5,
            Teeth = teeth
        };
    }

    public static PeriodontalExamSummaryDTO MapSummary(PeriodontalExam exam, bool isLatest) => new()
    {
        Id = exam.Id,
        PatientId = exam.PatientId,
        DoctorId = exam.DoctorId,
        DoctorName = exam.Doctor?.FullName,
        ExaminedAt = exam.ExaminedAt,
        Notes = exam.Notes,
        RecordedByUserId = exam.RecordedByUserId,
        IsLatest = isLatest,
        RecordedSiteCount = exam.RecordedSiteCount,
        BleedingPercent = exam.BleedingPercent,
        PlaquePercent = exam.PlaquePercent,
        MeanProbingDepthMm = exam.MeanProbingDepthMm,
        SitesDeepGe5 = exam.SitesDeepGe5,
        SitesDeepGe6 = exam.SitesDeepGe6
    };

    public static PeriodontalExamDTO MapChart(PeriodontalExam exam, bool isLatest)
    {
        var summary = MapSummary(exam, isLatest);
        return new PeriodontalExamDTO
        {
            Id = summary.Id,
            PatientId = summary.PatientId,
            DoctorId = summary.DoctorId,
            DoctorName = summary.DoctorName,
            ExaminedAt = summary.ExaminedAt,
            Notes = summary.Notes,
            RecordedByUserId = summary.RecordedByUserId,
            IsLatest = summary.IsLatest,
            RecordedSiteCount = summary.RecordedSiteCount,
            BleedingPercent = summary.BleedingPercent,
            PlaquePercent = summary.PlaquePercent,
            MeanProbingDepthMm = summary.MeanProbingDepthMm,
            SitesDeepGe5 = summary.SitesDeepGe5,
            SitesDeepGe6 = summary.SitesDeepGe6,
            Teeth = exam.Teeth.Select(MapToothDto).OrderBy(x => x.ToothNumber).ToList()
        };
    }

    private async Task ApplyOdontogramStatusAsync(PeriodontalExam exam, int patientId, CancellationToken cancellationToken)
    {
        var entries = await toothChartService.GetCurrentAsync(patientId, "permanent", null, cancellationToken);
        var byTooth = entries
            .GroupBy(x => x.ToothNumber)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(e => e.RecordedAt)
                    .ThenBy(e => e.Surface == ToothSurface.WholeTooth ? 0 : 1)
                    .First());

        foreach (var number in PeriodontalCalculations.PermanentArchOrder)
        {
            if (!byTooth.TryGetValue(number, out var latest))
                continue;
            var status = PeriodontalCalculations.StatusFromOdontogram(latest.Condition, latest.BridgeRole);
            if (status == PeriodontalToothStatus.Present)
                continue;

            var tooth = exam.Teeth.FirstOrDefault(t => t.ToothNumber == number);
            if (tooth == null)
            {
                tooth = new PeriodontalTooth { ToothNumber = number };
                exam.Teeth.Add(tooth);
            }

            tooth.Status = status;
            tooth.Mobility = PeriodontalMobility.Grade0;
            tooth.FacialFurcation = PeriodontalFurcation.Grade0;
            tooth.LingualFurcation = PeriodontalFurcation.Grade0;
            tooth.KeratinizedGingivaMm = null;
            tooth.Sites.Clear();
        }
    }

    private static void ApplyIndices(PeriodontalExam exam)
    {
        var indices = PeriodontalCalculations.ComputeIndices(exam.Teeth);
        exam.RecordedSiteCount = indices.RecordedSiteCount;
        exam.BleedingPercent = indices.RecordedSiteCount == 0 && indices.BleedingPercent == 0 ? null : indices.BleedingPercent;
        exam.PlaquePercent = indices.RecordedSiteCount == 0 && indices.PlaquePercent == 0 ? null : indices.PlaquePercent;
        exam.MeanProbingDepthMm = indices.MeanProbingDepthMm;
        exam.SitesDeepGe5 = indices.SitesDeepGe5;
        exam.SitesDeepGe6 = indices.SitesDeepGe6;
    }

    private static PeriodontalToothDTO MapToothDto(PeriodontalTooth tooth) => new()
    {
        ToothNumber = tooth.ToothNumber,
        Status = tooth.Status,
        Mobility = tooth.Status == PeriodontalToothStatus.Present ? tooth.Mobility : PeriodontalMobility.Grade0,
        FacialFurcation = HasFurcation(tooth) ? tooth.FacialFurcation : PeriodontalFurcation.Grade0,
        LingualFurcation = HasFurcation(tooth) ? tooth.LingualFurcation : PeriodontalFurcation.Grade0,
        Furcation = HasFurcation(tooth) ? MaxFurcation(tooth.FacialFurcation, tooth.LingualFurcation) : PeriodontalFurcation.Grade0,
        KeratinizedGingivaMm = tooth.Status == PeriodontalToothStatus.Present ? tooth.KeratinizedGingivaMm : null,
        Notes = tooth.Notes,
        Sites = tooth.Status == PeriodontalToothStatus.Missing
            ? []
            : tooth.Sites.Select(site => new PeriodontalSiteDTO
            {
                Surface = site.Surface,
                Position = site.Position,
                ProbingDepthMm = site.ProbingDepthMm,
                RecessionMm = site.RecessionMm,
                ClinicalAttachmentLevelMm = PeriodontalCalculations.CalculateCal(site.ProbingDepthMm, site.RecessionMm),
                Bleeding = site.Bleeding,
                Plaque = site.Plaque,
                Suppuration = site.Suppuration
            }).ToList()
    };

    private static PeriodontalTooth MapTooth(PeriodontalToothDTO dto)
    {
        var present = dto.Status == PeriodontalToothStatus.Present;
        var furcation = HasFurcationNumber(dto.ToothNumber);
        var facial = furcation ? (dto.FacialFurcation != PeriodontalFurcation.Grade0 ? dto.FacialFurcation : dto.Furcation) : PeriodontalFurcation.Grade0;
        var lingual = furcation ? dto.LingualFurcation : PeriodontalFurcation.Grade0;
        var tooth = new PeriodontalTooth
        {
            ToothNumber = dto.ToothNumber,
            Status = dto.Status,
            Mobility = present ? dto.Mobility : PeriodontalMobility.Grade0,
            FacialFurcation = present ? facial : PeriodontalFurcation.Grade0,
            LingualFurcation = present ? lingual : PeriodontalFurcation.Grade0,
            KeratinizedGingivaMm = present ? dto.KeratinizedGingivaMm : null,
            Notes = dto.Notes
        };
        if (dto.Status == PeriodontalToothStatus.Missing)
            return tooth;

        foreach (var site in dto.Sites)
        {
            tooth.Sites.Add(new PeriodontalSite
            {
                Surface = site.Surface,
                Position = site.Position,
                ProbingDepthMm = site.ProbingDepthMm,
                RecessionMm = site.RecessionMm,
                ClinicalAttachmentLevelMm = PeriodontalCalculations.CalculateCal(site.ProbingDepthMm, site.RecessionMm),
                Bleeding = site.Bleeding,
                Plaque = site.Plaque,
                Suppuration = site.Suppuration
            });
        }
        return tooth;
    }

    private static PeriodontalTooth CloneTooth(PeriodontalTooth source)
    {
        var tooth = new PeriodontalTooth
        {
            ToothNumber = source.ToothNumber,
            Status = source.Status,
            Mobility = source.Mobility,
            FacialFurcation = source.FacialFurcation,
            LingualFurcation = source.LingualFurcation,
            KeratinizedGingivaMm = source.KeratinizedGingivaMm,
            Notes = source.Notes
        };
        foreach (var site in source.Sites)
        {
            tooth.Sites.Add(new PeriodontalSite
            {
                Surface = site.Surface,
                Position = site.Position,
                ProbingDepthMm = site.ProbingDepthMm,
                RecessionMm = site.RecessionMm,
                ClinicalAttachmentLevelMm = PeriodontalCalculations.CalculateCal(site.ProbingDepthMm, site.RecessionMm),
                Bleeding = site.Bleeding,
                Plaque = site.Plaque,
                Suppuration = site.Suppuration
            });
        }
        return tooth;
    }

    private static bool HasFurcation(PeriodontalTooth tooth) =>
        tooth.Status == PeriodontalToothStatus.Present && FdiToothNumber.HasFurcation(tooth.ToothNumber);

    private static bool HasFurcationNumber(int toothNumber) => FdiToothNumber.HasFurcation(toothNumber);

    private static PeriodontalFurcation MaxFurcation(PeriodontalFurcation a, PeriodontalFurcation b) =>
        (PeriodontalFurcation)Math.Max((int)a, (int)b);

    private static int? MaxPd(PeriodontalTooth? tooth)
    {
        if (tooth == null || tooth.Status == PeriodontalToothStatus.Missing)
            return null;
        var values = tooth.Sites.Where(x => x.ProbingDepthMm.HasValue).Select(x => x.ProbingDepthMm!.Value).ToList();
        return values.Count == 0 ? null : values.Max();
    }

    private static int? MaxCal(PeriodontalTooth? tooth)
    {
        if (tooth == null || tooth.Status == PeriodontalToothStatus.Missing)
            return null;
        var values = tooth.Sites
            .Select(x => PeriodontalCalculations.CalculateCal(x.ProbingDepthMm, x.RecessionMm))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();
        return values.Count == 0 ? null : values.Max();
    }
}
