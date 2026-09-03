namespace Clinic_System.Core.Validation;

public static class PeriodontalThresholds
{
    public const int PocketNormalMaxMm = 3;
    public const int PocketAlertMaxMm = 5;
    public const int MaxMillimeters = 15;
}

public static class PeriodontalCalculations
{
    public static int? CalculateCal(int? probingDepthMm, int? recessionMm)
    {
        if (!probingDepthMm.HasValue && !recessionMm.HasValue)
            return null;
        return (probingDepthMm ?? 0) + (recessionMm ?? 0);
    }

    public static PeriodontalPocketSeverity GetPocketSeverity(int? probingDepthMm)
    {
        if (!probingDepthMm.HasValue)
            return PeriodontalPocketSeverity.Unrecorded;
        if (probingDepthMm.Value <= PeriodontalThresholds.PocketNormalMaxMm)
            return PeriodontalPocketSeverity.Normal;
        if (probingDepthMm.Value <= PeriodontalThresholds.PocketAlertMaxMm)
            return PeriodontalPocketSeverity.Alert;
        return PeriodontalPocketSeverity.Elevated;
    }

    public static bool IsValidMillimeters(int? value) =>
        !value.HasValue || value.Value is >= 0 and <= PeriodontalThresholds.MaxMillimeters;

    public static int[] PermanentArchOrder { get; } =
    [
        18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28,
        48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38
    ];

    public static int[] UpperArch { get; } =
        [18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28];

    public static int[] LowerArch { get; } =
        [48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38];

    public static int NextTooth(int toothNumber)
    {
        var index = Array.IndexOf(PermanentArchOrder, toothNumber);
        if (index < 0)
            return toothNumber;
        return PermanentArchOrder[(index + 1) % PermanentArchOrder.Length];
    }

    public static int PreviousTooth(int toothNumber)
    {
        var index = Array.IndexOf(PermanentArchOrder, toothNumber);
        if (index < 0)
            return toothNumber;
        return PermanentArchOrder[(index - 1 + PermanentArchOrder.Length) % PermanentArchOrder.Length];
    }

    public static PeriodontalExamIndices ComputeIndices(IEnumerable<PeriodontalTooth> teeth)
    {
        var sites = teeth
            .Where(t => t.Status == PeriodontalToothStatus.Present)
            .SelectMany(t => t.Sites)
            .ToList();
        var recorded = sites.Where(s => s.ProbingDepthMm.HasValue).ToList();
        var recordedCount = recorded.Count;
        if (recordedCount == 0 && sites.Count == 0)
            return new PeriodontalExamIndices();

        var flagBase = Math.Max(recordedCount, 1);
        var bleedingCount = recorded.Count(s => s.Bleeding);
        var plaqueCount = recorded.Count(s => s.Plaque);
        if (recordedCount == 0)
        {
            flagBase = Math.Max(sites.Count, 1);
            bleedingCount = sites.Count(s => s.Bleeding);
            plaqueCount = sites.Count(s => s.Plaque);
        }

        return new PeriodontalExamIndices
        {
            RecordedSiteCount = recordedCount,
            BleedingPercent = Math.Round(100m * bleedingCount / flagBase, 1),
            PlaquePercent = Math.Round(100m * plaqueCount / flagBase, 1),
            MeanProbingDepthMm = recordedCount == 0
                ? null
                : Math.Round((decimal)recorded.Average(s => s.ProbingDepthMm!.Value), 1),
            SitesDeepGe5 = recorded.Count(s => s.ProbingDepthMm >= 5),
            SitesDeepGe6 = recorded.Count(s => s.ProbingDepthMm >= 6)
        };
    }

    public static PeriodontalToothStatus StatusFromOdontogram(
        ToothCondition? condition,
        BridgeRole? bridgeRole = null)
    {
        if (ToothBridgeRules.IsAbsentCondition(condition, bridgeRole))
            return PeriodontalToothStatus.Missing;
        if (condition == ToothCondition.Implant)
            return PeriodontalToothStatus.Implant;
        return PeriodontalToothStatus.Present;
    }
}

public sealed class PeriodontalExamIndices
{
    public int RecordedSiteCount { get; init; }
    public decimal BleedingPercent { get; init; }
    public decimal PlaquePercent { get; init; }
    public decimal? MeanProbingDepthMm { get; init; }
    public int SitesDeepGe5 { get; init; }
    public int SitesDeepGe6 { get; init; }
}
