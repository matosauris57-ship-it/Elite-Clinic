namespace Clinic_System.Core.Validation;

public static class ToothFindingRules
{
    public static bool IsWholeToothOnly(ToothCondition condition) =>
        condition is ToothCondition.Missing or ToothCondition.Extracted or ToothCondition.Implant
            or ToothCondition.Crown or ToothCondition.Prosthesis or ToothCondition.Bridge
            or ToothCondition.RootCanal or ToothCondition.Mobility;

    public static bool IsSurfaceOnly(ToothCondition condition) =>
        condition is ToothCondition.Caries or ToothCondition.Filling or ToothCondition.Sealant;

    public static bool AllowedOnWholeTooth(ToothCondition condition) =>
        !IsSurfaceOnly(condition);

    public static bool AllowedOnSurface(ToothCondition condition) =>
        !IsWholeToothOnly(condition);

    public static bool MatchesScope(ToothCondition condition, ToothSurface surface)
    {
        if (surface == ToothSurface.WholeTooth)
            return AllowedOnWholeTooth(condition);
        return AllowedOnSurface(condition);
    }

    public static bool RequiresCariesDetails(ToothCondition condition) =>
        condition == ToothCondition.Caries;

    public static string? SuggestedDiagnosis(ToothCondition condition) => condition switch
    {
        ToothCondition.Healthy => null,
        ToothCondition.Caries => "Caries dental",
        ToothCondition.Filling => "Obturación / restauración",
        ToothCondition.Crown => "Corona dental",
        ToothCondition.Missing or ToothCondition.Extracted => "Pieza ausente",
        ToothCondition.Implant => "Implante osteointegrado",
        ToothCondition.Fractured => "Fractura dentaria",
        ToothCondition.RootCanal => "Tratamiento de conductos",
        ToothCondition.Prosthesis => "Prótesis dental",
        ToothCondition.Bridge => "Puente fijo",
        ToothCondition.Sealant => "Sellante de fosas y fisuras",
        ToothCondition.Mobility => "Movilidad dentaria",
        _ => null
    };

    public static string? SuggestedTreatment(ToothCondition condition) => condition switch
    {
        ToothCondition.Caries => "Restauración con resina",
        ToothCondition.Filling => null,
        ToothCondition.Missing => "Implante o prótesis",
        ToothCondition.Fractured => "Evaluar restauración o corona",
        _ => null
    };
}
