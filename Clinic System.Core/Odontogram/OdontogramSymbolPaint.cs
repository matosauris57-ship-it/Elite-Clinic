using Clinic_System.Core.Enums;
using Clinic_System.Core.Validation;

namespace Clinic_System.Core.Odontogram;

public static class OdontogramSymbolPaint
{
    public static bool PaintsSurfacePath(OdontogramConditionSymbol symbol)
    {
        var shape = OdontogramSymbolShapes.Normalize(symbol.Shape);
        return shape is OdontogramSymbolShapes.Fill or OdontogramSymbolShapes.Outline;
    }

    public static bool DrawsMark(OdontogramConditionSymbol symbol) =>
        symbol.Enabled && !PaintsSurfacePath(symbol);

    public static bool UsesWholeToothOutline(OdontogramConditionSymbol symbol)
    {
        var shape = OdontogramSymbolShapes.Normalize(symbol.Shape);
        return shape is OdontogramSymbolShapes.Outline or OdontogramSymbolShapes.Fill
            or OdontogramSymbolShapes.Square or OdontogramSymbolShapes.SquareFilled;
    }

    public static bool IsAbsenceCondition(ToothCondition condition) =>
        condition is ToothCondition.Missing or ToothCondition.Extracted;

    public static bool IsPrimaryWholeTooth(ToothCondition condition) =>
        ToothFindingRules.IsWholeToothOnly(condition) || IsAbsenceCondition(condition);

    public static string SurfacePathStyle(OdontogramConditionSymbol? symbol)
    {
        if (symbol is not { Enabled: true } || !PaintsSurfacePath(symbol))
            return "--symbol-fill: transparent; --symbol-stroke: transparent; --symbol-fill-opacity: 0; --symbol-stroke-width: 0;";

        var color = OdontogramSymbolColor.Normalize(symbol.Color);
        var opacity = Math.Clamp(symbol.Opacity, 0.1, 1);
        var width = Math.Clamp(symbol.StrokeWidth, 0.5, 6);
        var shape = OdontogramSymbolShapes.Normalize(symbol.Shape);
        var style = OdontogramSymbolStyles.Normalize(symbol.Style);

        if (shape == OdontogramSymbolShapes.Fill || style == OdontogramSymbolStyles.Fill)
        {
            return $"--symbol-fill: {color}; --symbol-stroke: {color}; --symbol-fill-opacity: {opacity.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}; --symbol-stroke-width: {width.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)};";
        }

        return $"--symbol-fill: transparent; --symbol-stroke: {color}; --symbol-fill-opacity: 0; --symbol-stroke-width: {width.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}; --symbol-opacity: {opacity.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)};";
    }

    public static string PhaseCssVariables(OdontogramPhaseSymbol? phase)
    {
        var color = OdontogramSymbolColor.Normalize(phase?.Color, "#9E9E9E");
        var opacity = Math.Clamp(phase?.Opacity ?? 1, 0.1, 1);
        var soft = OdontogramSymbolColor.ToRgba(color, Math.Min(0.48, opacity));
        var ink = OdontogramSymbolColor.ContrastInk(color);
        return $"--phase-color: {color}; --phase-soft: {soft}; --phase-ink: {ink};";
    }

    public static bool MarkIsFilled(OdontogramConditionSymbol symbol)
    {
        var shape = OdontogramSymbolShapes.Normalize(symbol.Shape);
        var style = OdontogramSymbolStyles.Normalize(symbol.Style);

        if (style == OdontogramSymbolStyles.Outline || style == OdontogramSymbolStyles.Line)
            return shape is OdontogramSymbolShapes.Dot;

        return shape is OdontogramSymbolShapes.CircleFilled
            or OdontogramSymbolShapes.SquareFilled
            or OdontogramSymbolShapes.Fill
            or OdontogramSymbolShapes.Dot
            || style == OdontogramSymbolStyles.Fill;
    }
}
