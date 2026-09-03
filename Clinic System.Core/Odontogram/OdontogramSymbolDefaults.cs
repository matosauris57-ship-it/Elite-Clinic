using Clinic_System.Core.Enums;

namespace Clinic_System.Core.Odontogram;

public static class OdontogramSymbolDefaults
{
    public static OdontogramSymbolConfigDocument Create(string clinicKey = OdontogramSymbolConfigDocument.DefaultClinicKey) =>
        new()
        {
            ClinicKey = string.IsNullOrWhiteSpace(clinicKey)
                ? OdontogramSymbolConfigDocument.DefaultClinicKey
                : clinicKey.Trim(),
            Conditions = AllConditions().Select(Condition).ToList(),
            Phases = AllPhases().Select(Phase).ToList()
        };

    public static OdontogramConditionSymbol Condition(ToothCondition condition) => condition switch
    {
        ToothCondition.Healthy => Row(condition, OdontogramSymbolShapes.Outline, "#66BB6A", OdontogramSymbolStyles.Outline, 0.35, 1.5),
        ToothCondition.Caries => Row(condition, OdontogramSymbolShapes.Circle, "#EF5350", OdontogramSymbolStyles.Fill, 0.9, 2),
        ToothCondition.Missing => Row(condition, OdontogramSymbolShapes.Cross, "#212121", OdontogramSymbolStyles.Line, 1, 2.4),
        ToothCondition.Crown => Row(condition, OdontogramSymbolShapes.Outline, "#FFCA28", OdontogramSymbolStyles.Outline, 0.95, 2.2),
        ToothCondition.Filling => Row(condition, OdontogramSymbolShapes.SquareFilled, "#42A5F5", OdontogramSymbolStyles.Fill, 0.88, 1.8),
        ToothCondition.RootCanal => Row(condition, OdontogramSymbolShapes.Triangle, "#AB47BC", OdontogramSymbolStyles.Fill, 0.9, 1.8),
        ToothCondition.Implant => Row(condition, OdontogramSymbolShapes.Square, "#26C6DA", OdontogramSymbolStyles.Outline, 0.95, 2.2),
        ToothCondition.Fractured => Row(condition, OdontogramSymbolShapes.Line, "#FF7043", OdontogramSymbolStyles.Line, 1, 2.2),
        ToothCondition.Extracted => Row(condition, OdontogramSymbolShapes.Cross, "#EF5350", OdontogramSymbolStyles.Line, 1, 2.4),
        ToothCondition.Prosthesis => Row(condition, OdontogramSymbolShapes.Outline, "#8D6E63", OdontogramSymbolStyles.Outline, 0.9, 2),
        ToothCondition.Bridge => Row(condition, OdontogramSymbolShapes.Line, "#78909C", OdontogramSymbolStyles.Line, 0.95, 2.4),
        ToothCondition.Sealant => Row(condition, OdontogramSymbolShapes.Dot, "#26A69A", OdontogramSymbolStyles.Fill, 0.9, 1.5),
        ToothCondition.Other => Row(condition, OdontogramSymbolShapes.Circle, "#90A4AE", OdontogramSymbolStyles.Outline, 0.8, 2),
        ToothCondition.Mobility => Row(condition, OdontogramSymbolShapes.Triangle, "#5C6BC0", OdontogramSymbolStyles.Outline, 0.9, 2),
        _ => Row(condition, OdontogramSymbolShapes.Circle, "#757575", OdontogramSymbolStyles.Outline, 0.8, 2)
    };

    public static OdontogramPhaseSymbol Phase(ToothChartPhase phase) => phase switch
    {
        ToothChartPhase.Diagnosis => new() { Phase = phase, Color = "#EF5350", Opacity = 1 },
        ToothChartPhase.Planned => new() { Phase = phase, Color = "#FFCA28", Opacity = 1 },
        ToothChartPhase.InTreatment => new() { Phase = phase, Color = "#AB47BC", Opacity = 1 },
        ToothChartPhase.Completed => new() { Phase = phase, Color = "#42A5F5", Opacity = 1 },
        _ => new() { Phase = phase, Color = "#9E9E9E", Opacity = 1 }
    };

    public static IReadOnlyList<ToothCondition> AllConditions() =>
        Enum.GetValues<ToothCondition>();

    public static IReadOnlyList<ToothChartPhase> AllPhases() =>
        Enum.GetValues<ToothChartPhase>();

    public static OdontogramSymbolConfigDocument Merge(OdontogramSymbolConfigDocument? stored)
    {
        var merged = Create(stored?.ClinicKey ?? OdontogramSymbolConfigDocument.DefaultClinicKey);
        if (stored == null)
            return merged;

        merged.UpdatedAt = stored.UpdatedAt;
        merged.UpdatedBy = stored.UpdatedBy;
        merged.History = stored.History ?? [];

        foreach (var row in merged.Conditions)
        {
            var existing = stored.Conditions?.FirstOrDefault(x => x.Condition == row.Condition);
            if (existing == null)
                continue;

            row.Shape = OdontogramSymbolShapes.Normalize(existing.Shape);
            row.Color = OdontogramSymbolColor.Normalize(existing.Color, row.Color);
            row.Style = OdontogramSymbolStyles.Normalize(existing.Style);
            row.Opacity = Math.Clamp(existing.Opacity, 0.1, 1);
            row.StrokeWidth = Math.Clamp(existing.StrokeWidth, 0.5, 6);
            row.CustomSvg = existing.CustomSvg;
            row.Enabled = existing.Enabled;
        }

        foreach (var row in merged.Phases)
        {
            var existing = stored.Phases?.FirstOrDefault(x => x.Phase == row.Phase);
            if (existing == null)
                continue;

            row.Color = OdontogramSymbolColor.Normalize(existing.Color, row.Color);
            row.Opacity = Math.Clamp(existing.Opacity, 0.1, 1);
        }

        return merged;
    }

    public static string? Validate(OdontogramSymbolConfigDocument document)
    {
        if (document.Conditions.Count == 0)
            return "La configuración de condiciones no puede estar vacía.";

        foreach (var row in document.Conditions)
        {
            if (!OdontogramSymbolShapes.IsKnown(row.Shape))
                return "Hay una forma de símbolo no reconocida.";
            if (!OdontogramSymbolColor.IsValid(row.Color))
                return "Hay un color inválido. Use un código hexadecimal, por ejemplo #FF0000.";
            if (!OdontogramSymbolStyles.IsKnown(row.Style))
                return "Hay un estilo de símbolo no reconocido.";
            if (row.Opacity is < 0.1 or > 1)
                return "La opacidad debe estar entre 0.1 y 1.";
            if (row.StrokeWidth is < 0.5 or > 6)
                return "El grosor de línea debe estar entre 0.5 y 6.";
        }

        foreach (var row in document.Phases)
        {
            if (!OdontogramSymbolColor.IsValid(row.Color))
                return "Hay un color de fase inválido. Use un código hexadecimal, por ejemplo #FF0000.";
            if (row.Opacity is < 0.1 or > 1)
                return "La opacidad de fase debe estar entre 0.1 y 1.";
        }

        return null;
    }

    public static string ShapeLabel(string shape) => OdontogramSymbolShapes.Normalize(shape) switch
    {
        OdontogramSymbolShapes.Circle => "Círculo",
        OdontogramSymbolShapes.CircleFilled => "Círculo relleno",
        OdontogramSymbolShapes.Square => "Cuadrado",
        OdontogramSymbolShapes.SquareFilled => "Cuadrado relleno",
        OdontogramSymbolShapes.Triangle => "Triángulo",
        OdontogramSymbolShapes.Line => "Línea",
        OdontogramSymbolShapes.Cross => "Cruz / X",
        OdontogramSymbolShapes.Outline => "Contorno",
        OdontogramSymbolShapes.Fill => "Relleno",
        OdontogramSymbolShapes.Dot => "Punto",
        _ => shape
    };

    public static string StyleLabel(string style) => OdontogramSymbolStyles.Normalize(style) switch
    {
        OdontogramSymbolStyles.Outline => "Contorno",
        OdontogramSymbolStyles.Fill => "Relleno",
        OdontogramSymbolStyles.Line => "Línea",
        _ => style
    };

    public static string ConditionLabel(ToothCondition condition) => condition switch
    {
        ToothCondition.Healthy => "Sano",
        ToothCondition.Caries => "Caries",
        ToothCondition.Missing => "Diente ausente",
        ToothCondition.Crown => "Corona",
        ToothCondition.Filling => "Restauración",
        ToothCondition.RootCanal => "Tratamiento de conducto",
        ToothCondition.Implant => "Implante",
        ToothCondition.Fractured => "Fractura",
        ToothCondition.Extracted => "Diente extraído",
        ToothCondition.Prosthesis => "Prótesis",
        ToothCondition.Bridge => "Puente",
        ToothCondition.Sealant => "Sellante",
        ToothCondition.Other => "Otros",
        ToothCondition.Mobility => "Movilidad",
        _ => condition.ToString()
    };

    public static string PhaseLabel(ToothChartPhase phase) => phase switch
    {
        ToothChartPhase.Diagnosis => "Diagnóstico",
        ToothChartPhase.Planned => "Planificado",
        ToothChartPhase.InTreatment => "En tratamiento",
        ToothChartPhase.Completed => "Completado",
        _ => phase.ToString()
    };

    private static OdontogramConditionSymbol Row(
        ToothCondition condition,
        string shape,
        string color,
        string style,
        double opacity,
        double strokeWidth) => new()
    {
        Condition = condition,
        Shape = shape,
        Color = color,
        Style = style,
        Opacity = opacity,
        StrokeWidth = strokeWidth,
        Enabled = true
    };
}
