namespace DentalCare.Admin.Models;

public static class ToothClinicalLabels
{
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

    public static string Phase(ToothChartPhase phase) => phase switch
    {
        ToothChartPhase.Diagnosis => "Diagnóstico",
        ToothChartPhase.Planned => "Planificado",
        ToothChartPhase.InTreatment => "En tratamiento",
        ToothChartPhase.Completed => "Completado",
        _ => phase.ToString()
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

    public static bool IsWholeToothCondition(ToothCondition condition) =>
        Clinic_System.Core.Validation.ToothFindingRules.IsWholeToothOnly(
            (Clinic_System.Core.Enums.ToothCondition)(int)condition);

    public static bool ShowsMaterial(ToothCondition condition) =>
        condition is ToothCondition.Filling or ToothCondition.Crown or ToothCondition.Bridge
            or ToothCondition.Prosthesis or ToothCondition.Sealant;

    public static bool ShowsCariesDetails(ToothCondition condition) =>
        condition == ToothCondition.Caries;

    public static string CariesKind(CariesType type) => type switch
    {
        CariesType.PitAndFissure => "Caries de fosas y fisuras",
        CariesType.SmoothSurface => "Caries de superficie lisa",
        CariesType.Root => "Caries radicular",
        CariesType.Secondary => "Caries secundaria / recurrente",
        _ => type.ToString()
    };

    public static string Icdas(IcdasCode code) => code switch
    {
        IcdasCode.Sound => "0 — Superficie sana",
        IcdasCode.InitialVisual => "1 — Cambio visual inicial",
        IcdasCode.DistinctVisual => "2 — Cambio visual definido",
        IcdasCode.LocalizedEnamelBreakdown => "3 — Ruptura localizada del esmalte",
        IcdasCode.UnderlyingDarkShadow => "4 — Sombra oscura subyacente",
        IcdasCode.DistinctCavityWithDentin => "5 — Cavidad manifiesta con dentina visible",
        IcdasCode.ExtensiveCavityWithDentin => "6 — Cavidad extensa con dentina visible",
        _ => ((int)code).ToString()
    };
}
