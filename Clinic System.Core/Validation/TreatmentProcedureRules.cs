namespace Clinic_System.Core.Validation;

public static class TreatmentProcedureRules
{
    public static (TreatmentProcedureTarget Target, TreatmentToothKindFilter Filter) DefaultsForCode(string? code)
    {
        var key = (code ?? string.Empty).Trim().ToLowerInvariant();
        return key switch
        {
            "limpieza" or "revision" or "revision-ortodoncia" or "brackets"
                or "blanqueamiento" or "radiografia" =>
                (TreatmentProcedureTarget.WholeMouth, TreatmentToothKindFilter.Any),
            "sellantes" => (TreatmentProcedureTarget.PerSurface, TreatmentToothKindFilter.Any),
            "extraccion-molar" => (TreatmentProcedureTarget.PerTooth, TreatmentToothKindFilter.Molar),
            "extraccion-simple" or "corona" or "endodoncia" or "implante" =>
                (TreatmentProcedureTarget.PerTooth, TreatmentToothKindFilter.Any),
            _ => (TreatmentProcedureTarget.PerTooth, TreatmentToothKindFilter.Any)
        };
    }

    public static string TargetLabel(TreatmentProcedureTarget target) => target switch
    {
        TreatmentProcedureTarget.WholeMouth => "Boca completa",
        TreatmentProcedureTarget.PerTooth => "Por pieza",
        TreatmentProcedureTarget.PerSurface => "Por superficie",
        _ => target.ToString()
    };

    public static string FilterLabel(TreatmentToothKindFilter filter) => filter switch
    {
        TreatmentToothKindFilter.Any => "Cualquier pieza",
        TreatmentToothKindFilter.Molar => "Solo molares",
        TreatmentToothKindFilter.Permanent => "Solo permanentes",
        TreatmentToothKindFilter.Anterior => "Solo anteriores",
        _ => filter.ToString()
    };

    public static string? ValidateAssignment(
        TreatmentProcedureTarget target,
        TreatmentToothKindFilter filter,
        int? toothNumber,
        ToothSurface? surface)
    {
        switch (target)
        {
            case TreatmentProcedureTarget.WholeMouth:
                return toothNumber.HasValue
                    ? "Este procedimiento se aplica a toda la boca, no a una pieza específica."
                    : null;

            case TreatmentProcedureTarget.PerTooth:
                if (!toothNumber.HasValue || !FdiToothNumber.IsValid(toothNumber.Value))
                    return "Seleccione una o más piezas en el odontograma.";
                return ValidateKind(filter, toothNumber.Value);

            case TreatmentProcedureTarget.PerSurface:
                if (!toothNumber.HasValue || !FdiToothNumber.IsValid(toothNumber.Value))
                    return "Seleccione una o más piezas en el odontograma.";
                if (surface is null or ToothSurface.WholeTooth)
                    return "Indique la superficie a tratar; este procedimiento no se aplica a la pieza completa.";
                return ValidateKind(filter, toothNumber.Value);

            default:
                return "El alcance del procedimiento no es válido.";
        }
    }

    public static string? ValidateKind(TreatmentToothKindFilter filter, int toothNumber) => filter switch
    {
        TreatmentToothKindFilter.Any => null,
        TreatmentToothKindFilter.Molar => FdiToothNumber.IsMolar(toothNumber)
            ? null
            : "Este procedimiento solo aplica a molares.",
        TreatmentToothKindFilter.Permanent => FdiToothNumber.IsPermanent(toothNumber)
            ? null
            : "Este procedimiento solo aplica a dentición permanente.",
        TreatmentToothKindFilter.Anterior => FdiToothNumber.IsAnterior(toothNumber)
            ? null
            : "Este procedimiento solo aplica a piezas anteriores.",
        _ => null
    };
}
