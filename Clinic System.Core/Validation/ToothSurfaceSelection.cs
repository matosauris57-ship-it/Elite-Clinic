namespace Clinic_System.Core.Validation;

public static class ToothSurfaceSelection
{
    public static readonly ToothSurface[] ClinicalOrder =
    [
        ToothSurface.Mesial,
        ToothSurface.OcclusalIncisal,
        ToothSurface.Distal,
        ToothSurface.BuccalFacial,
        ToothSurface.LingualPalatal
    ];

    public static List<ToothSurface> Resolve(ToothSurface surface, IEnumerable<ToothSurface>? surfaces)
    {
        var faces = FacesOf(surfaces);
        if (faces.Count > 0)
            return faces;
        return [surface];
    }

    public static List<ToothSurface> ResolveForWrite(
        ToothCondition condition,
        ToothSurface surface,
        IEnumerable<ToothSurface>? surfaces)
    {
        if (condition == ToothCondition.Bridge || ToothFindingRules.IsWholeToothOnly(condition))
            return [ToothSurface.WholeTooth];
        return Resolve(surface, surfaces);
    }

    public static List<ToothSurface> FacesOf(IEnumerable<ToothSurface>? surfaces) =>
        ClinicalOrder.Where(s => (surfaces ?? []).Contains(s)).Distinct().ToList();

    public static string Code(ToothSurface surface, int? toothNumber = null) => surface switch
    {
        ToothSurface.Mesial => "M",
        ToothSurface.Distal => "D",
        ToothSurface.OcclusalIncisal =>
            toothNumber is int tooth && FdiToothNumber.IsValid(tooth) && FdiToothNumber.IsAnterior(tooth) ? "I" : "O",
        ToothSurface.BuccalFacial => "V",
        ToothSurface.LingualPalatal =>
            toothNumber is int tooth && FdiToothNumber.IsValid(tooth) && FdiToothNumber.IsUpper(tooth) ? "P" : "L",
        _ => ""
    };

    public static string CombinationCode(IEnumerable<ToothSurface> surfaces, int? toothNumber = null)
    {
        var faces = FacesOf(surfaces);
        return faces.Count == 0
            ? ""
            : string.Concat(faces.Select(s => Code(s, toothNumber)));
    }

    public static string CreatedCountMessage(int toothCount, int surfaceCount)
    {
        var n = Math.Max(1, toothCount) * Math.Max(1, surfaceCount);
        if (n <= 1)
            return "Entrada clínica registrada.";
        if (toothCount > 1 && surfaceCount > 1)
            return $"Diagnóstico registrado en {surfaceCount} caras de {toothCount} piezas.";
        if (surfaceCount > 1)
            return $"Diagnóstico registrado en {surfaceCount} caras.";
        return $"Diagnóstico registrado en {toothCount} piezas.";
    }
}
