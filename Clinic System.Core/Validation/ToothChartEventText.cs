namespace Clinic_System.Core.Validation;

public static class ToothChartEventText
{
    private static readonly (string From, string To)[] TitleTokens =
    [
        ("PitAndFissure", "Fosas y fisuras"),
        ("SmoothSurface", "Superficie lisa"),
        ("BuccalFacial", "Vestibular / Facial"),
        ("LingualPalatal", "Lingual / Palatal"),
        ("OcclusalIncisal", "Oclusal / Incisal"),
        ("WholeTooth", "Pieza completa"),
        ("MetalCeramic", "Metal-cerámica"),
        ("RootCanal", "Endodoncia"),
        ("InTreatment", "En tratamiento"),
        ("Root", "Radicular"),
        ("Secondary", "Secundaria"),
        ("Diagnosis", "Diagnóstico"),
        ("Planned", "Planificado"),
        ("Completed", "Completado"),
        ("Healthy", "Sano"),
        ("Missing", "Ausente"),
        ("Crown", "Corona"),
        ("Filling", "Restauración"),
        ("Implant", "Implante"),
        ("Fractured", "Fractura"),
        ("Extracted", "Extraído"),
        ("Prosthesis", "Prótesis"),
        ("Bridge", "Puente"),
        ("Sealant", "Sellante"),
        ("Mobility", "Movilidad"),
        ("Amalgam", "Amalgama"),
        ("Resin", "Resina"),
        ("Veneer", "Carilla"),
        ("Temporary", "Temporal"),
        ("Porcelain", "Porcelana"),
        ("Mesial", "Mesial"),
        ("Distal", "Distal")
    ];

    public static string BuildTitle(
        ToothChartPhase phase,
        int toothNumber,
        ToothSurface surface,
        ToothCondition condition,
        RestorationMaterial? restorationMaterial,
        CariesType? cariesType,
        IcdasCode? icdas)
    {
        var parts = new List<string>
        {
            Phase(phase),
            $"pieza {toothNumber}",
            Surface(surface),
            Condition(condition)
        };
        if (cariesType.HasValue)
            parts.Add(CariesKind(cariesType.Value));
        if (icdas.HasValue)
            parts.Add($"ICDAS {(int)icdas.Value}");
        if (restorationMaterial.HasValue)
            parts.Add(Material(restorationMaterial.Value));
        return string.Join(" · ", parts);
    }

    public static string LocalizeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return title ?? string.Empty;

        var text = title.Replace("diente ", "pieza ", StringComparison.OrdinalIgnoreCase);
        foreach (var (from, to) in TitleTokens)
            text = text.Replace(from, to, StringComparison.Ordinal);
        return text;
    }

    public static string Phase(ToothChartPhase phase) => phase switch
    {
        ToothChartPhase.Diagnosis => "Diagnóstico",
        ToothChartPhase.Planned => "Planificado",
        ToothChartPhase.InTreatment => "En tratamiento",
        ToothChartPhase.Completed => "Completado",
        _ => phase.ToString()
    };

    public static string Surface(ToothSurface surface) => surface switch
    {
        ToothSurface.WholeTooth => "Pieza completa",
        ToothSurface.Mesial => "Mesial",
        ToothSurface.Distal => "Distal",
        ToothSurface.BuccalFacial => "Vestibular / Facial",
        ToothSurface.LingualPalatal => "Lingual / Palatal",
        ToothSurface.OcclusalIncisal => "Oclusal / Incisal",
        _ => surface.ToString()
    };

    public static string Condition(ToothCondition condition) => condition switch
    {
        ToothCondition.Healthy => "Sano",
        ToothCondition.Caries => "Caries",
        ToothCondition.Missing => "Ausente",
        ToothCondition.Crown => "Corona",
        ToothCondition.Filling => "Restauración",
        ToothCondition.RootCanal => "Endodoncia",
        ToothCondition.Implant => "Implante",
        ToothCondition.Fractured => "Fractura",
        ToothCondition.Extracted => "Extraído",
        ToothCondition.Prosthesis => "Prótesis",
        ToothCondition.Bridge => "Puente",
        ToothCondition.Sealant => "Sellante",
        ToothCondition.Mobility => "Movilidad",
        ToothCondition.Other => "Otro",
        _ => condition.ToString()
    };

    public static string CariesKind(CariesType type) => type switch
    {
        CariesType.PitAndFissure => "Fosas y fisuras",
        CariesType.SmoothSurface => "Superficie lisa",
        CariesType.Root => "Radicular",
        CariesType.Secondary => "Secundaria",
        _ => type.ToString()
    };

    public static string Material(RestorationMaterial material) => material switch
    {
        RestorationMaterial.Amalgam => "Amalgama",
        RestorationMaterial.Resin => "Resina",
        RestorationMaterial.Inlay => "Inlay",
        RestorationMaterial.Onlay => "Onlay",
        RestorationMaterial.Veneer => "Carilla",
        RestorationMaterial.Temporary => "Temporal",
        RestorationMaterial.MetalCeramic => "Metal-cerámica",
        RestorationMaterial.Porcelain => "Porcelana",
        _ => material.ToString()
    };
}
