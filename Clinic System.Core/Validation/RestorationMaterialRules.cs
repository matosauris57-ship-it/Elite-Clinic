namespace Clinic_System.Core.Validation;

public static class RestorationMaterialRules
{
    public static bool RequiresMaterial(ToothCondition condition) =>
        condition is ToothCondition.Filling or ToothCondition.Crown or ToothCondition.Bridge
            or ToothCondition.Prosthesis or ToothCondition.Sealant;

    public static bool IsAllowed(int toothNumber, ToothCondition condition, RestorationMaterial? material)
    {
        if (!material.HasValue)
            return true;
        if (!FdiToothNumber.IsValid(toothNumber) || !RequiresMaterial(condition))
            return false;

        var posterior = FdiToothNumber.IsPosterior(toothNumber);
        var anterior = FdiToothNumber.IsAnterior(toothNumber);

        return material.Value switch
        {
            RestorationMaterial.Amalgam => condition == ToothCondition.Filling && posterior,
            RestorationMaterial.Resin => condition is ToothCondition.Filling or ToothCondition.Sealant or ToothCondition.Crown,
            RestorationMaterial.Inlay => condition == ToothCondition.Filling && posterior,
            RestorationMaterial.Onlay => condition == ToothCondition.Filling && posterior,
            RestorationMaterial.Veneer => (condition is ToothCondition.Filling or ToothCondition.Crown) && anterior,
            RestorationMaterial.Temporary => true,
            RestorationMaterial.MetalCeramic => condition is ToothCondition.Crown or ToothCondition.Bridge or ToothCondition.Prosthesis,
            RestorationMaterial.Porcelain => condition is ToothCondition.Crown or ToothCondition.Bridge or ToothCondition.Prosthesis,
            _ => false
        };
    }
}
