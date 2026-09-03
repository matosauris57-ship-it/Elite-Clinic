using Clinic_System.Core.Validation;
using DentalCare.Admin.Models;

namespace DentalCare.Admin.Services;

public static class PeriodontalChartFactory
{
    public static PeriodontalToothModel EnsureTooth(PeriodontalExamDetail exam, int toothNumber)
    {
        var tooth = exam.Teeth.FirstOrDefault(x => x.ToothNumber == toothNumber);
        if (tooth == null)
        {
            tooth = new PeriodontalToothModel { ToothNumber = toothNumber };
            exam.Teeth.Add(tooth);
        }

        if (tooth.Status == PeriodontalToothStatus.Missing)
            return tooth;

        EnsureSites(tooth);
        return tooth;
    }

    public static void EnsureSites(PeriodontalToothModel tooth)
    {
        foreach (var surface in new[] { PeriodontalSurface.Facial, PeriodontalSurface.Lingual })
        {
            foreach (var position in new[] { PeriodontalSitePosition.Mesial, PeriodontalSitePosition.Center, PeriodontalSitePosition.Distal })
            {
                if (tooth.Sites.All(s => s.Surface != surface || s.Position != position))
                    tooth.Sites.Add(new PeriodontalSiteModel { Surface = surface, Position = position });
            }
        }
    }

    public static void ApplyStatus(PeriodontalToothModel tooth, PeriodontalToothStatus status)
    {
        tooth.Status = status;
        if (status != PeriodontalToothStatus.Missing)
        {
            EnsureSites(tooth);
            return;
        }

        tooth.Sites.Clear();
        tooth.Mobility = PeriodontalMobility.Grade0;
        tooth.FacialFurcation = PeriodontalFurcation.Grade0;
        tooth.LingualFurcation = PeriodontalFurcation.Grade0;
        tooth.Furcation = PeriodontalFurcation.Grade0;
        tooth.KeratinizedGingivaMm = null;
    }

    public static PeriodontalSiteModel Site(PeriodontalToothModel tooth, PeriodontalSurface surface, PeriodontalSitePosition position) =>
        tooth.Sites.First(s => s.Surface == surface && s.Position == position);

    public static PeriodontalSiteModel? TrySite(PeriodontalToothModel tooth, PeriodontalSurface surface, PeriodontalSitePosition position) =>
        tooth.Sites.FirstOrDefault(s => s.Surface == surface && s.Position == position);

    public static int? MaxProbing(PeriodontalToothModel tooth)
    {
        if (tooth.Status == PeriodontalToothStatus.Missing)
            return null;
        var values = tooth.Sites.Where(x => x.ProbingDepthMm.HasValue).Select(x => x.ProbingDepthMm!.Value).ToList();
        return values.Count == 0 ? null : values.Max();
    }

    public static bool AnyBleeding(PeriodontalToothModel tooth) =>
        tooth.Status != PeriodontalToothStatus.Missing && tooth.Sites.Any(x => x.Bleeding);

    public static bool AnyPlaque(PeriodontalToothModel tooth) =>
        tooth.Status != PeriodontalToothStatus.Missing && tooth.Sites.Any(x => x.Plaque);

    public static bool AnySuppuration(PeriodontalToothModel tooth) =>
        tooth.Status != PeriodontalToothStatus.Missing && tooth.Sites.Any(x => x.Suppuration);

    public static string PocketMark(int? probingDepthMm)
    {
        var severity = PeriodontalCalculations.GetPocketSeverity(probingDepthMm);
        return severity switch
        {
            Clinic_System.Core.Enums.PeriodontalPocketSeverity.Normal => "N",
            Clinic_System.Core.Enums.PeriodontalPocketSeverity.Alert => "!",
            Clinic_System.Core.Enums.PeriodontalPocketSeverity.Elevated => "!!",
            _ => "—"
        };
    }

    public static string PocketColor(int? probingDepthMm) =>
        PeriodontalCalculations.GetPocketSeverity(probingDepthMm) switch
        {
            Clinic_System.Core.Enums.PeriodontalPocketSeverity.Normal => "#2e7d32",
            Clinic_System.Core.Enums.PeriodontalPocketSeverity.Alert => "#c9a227",
            Clinic_System.Core.Enums.PeriodontalPocketSeverity.Elevated => "#c62828",
            _ => "#9aa3ad"
        };

    public static string MobilityLabel(PeriodontalMobility value) => value switch
    {
        PeriodontalMobility.Grade0 => "Grado 0",
        PeriodontalMobility.Grade1 => "Grado I",
        PeriodontalMobility.Grade2 => "Grado II",
        PeriodontalMobility.Grade3 => "Grado III",
        _ => value.ToString()
    };

    public static string FurcationLabel(PeriodontalFurcation value) => value switch
    {
        PeriodontalFurcation.Grade0 => "Grado 0",
        PeriodontalFurcation.Grade1 => "Grado I",
        PeriodontalFurcation.Grade2 => "Grado II",
        PeriodontalFurcation.Grade3 => "Grado III",
        _ => value.ToString()
    };

    public static string FurcationShort(PeriodontalFurcation value) => value switch
    {
        PeriodontalFurcation.Grade1 => "F·I",
        PeriodontalFurcation.Grade2 => "F·II",
        PeriodontalFurcation.Grade3 => "F·III",
        _ => ""
    };

    public static string StatusLabel(PeriodontalToothStatus status) => status switch
    {
        PeriodontalToothStatus.Missing => "Ausente",
        PeriodontalToothStatus.Implant => "Implante",
        _ => "Presente"
    };

    public static string SurfaceCaption(PeriodontalSurface surface, bool upper) =>
        surface == PeriodontalSurface.Facial
            ? "Vestibular / Facial"
            : upper ? "Palatino" : "Lingual";

    public static PeriodontalLiveIndices LiveIndices(PeriodontalExamDetail exam)
    {
        var sites = exam.Teeth
            .Where(t => t.Status == PeriodontalToothStatus.Present)
            .SelectMany(t => t.Sites)
            .ToList();
        var recorded = sites.Where(s => s.ProbingDepthMm.HasValue).ToList();
        var recordedCount = recorded.Count;
        if (recordedCount == 0 && sites.Count == 0)
            return new PeriodontalLiveIndices();

        var flagBase = Math.Max(recordedCount == 0 ? sites.Count : recordedCount, 1);
        var source = recordedCount == 0 ? sites : recorded;
        return new PeriodontalLiveIndices
        {
            RecordedSiteCount = recordedCount,
            BleedingPercent = Math.Round(100m * source.Count(s => s.Bleeding) / flagBase, 1),
            PlaquePercent = Math.Round(100m * source.Count(s => s.Plaque) / flagBase, 1),
            MeanProbingDepthMm = recordedCount == 0
                ? null
                : Math.Round((decimal)recorded.Average(s => s.ProbingDepthMm!.Value), 1),
            SitesDeepGe5 = recorded.Count(s => s.ProbingDepthMm >= 5),
            SitesDeepGe6 = recorded.Count(s => s.ProbingDepthMm >= 6)
        };
    }
}
