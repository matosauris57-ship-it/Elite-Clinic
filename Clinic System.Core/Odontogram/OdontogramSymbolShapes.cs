namespace Clinic_System.Core.Odontogram;

public static class OdontogramSymbolShapes
{
    public const string Circle = "circle";
    public const string CircleFilled = "circle-filled";
    public const string Square = "square";
    public const string SquareFilled = "square-filled";
    public const string Triangle = "triangle";
    public const string Line = "line";
    public const string Cross = "cross";
    public const string Outline = "outline";
    public const string Fill = "fill";
    public const string Dot = "dot";

    public static readonly IReadOnlyList<string> All =
    [
        Circle, CircleFilled, Square, SquareFilled, Triangle, Line, Cross, Outline, Fill, Dot
    ];

    public static bool IsKnown(string? shape) =>
        !string.IsNullOrWhiteSpace(shape) && All.Contains(shape.Trim(), StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string? shape) =>
        All.FirstOrDefault(x => string.Equals(x, shape?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? Circle;
}

public static class OdontogramSymbolStyles
{
    public const string Outline = "outline";
    public const string Fill = "fill";
    public const string Line = "line";

    public static readonly IReadOnlyList<string> All = [Outline, Fill, Line];

    public static bool IsKnown(string? style) =>
        !string.IsNullOrWhiteSpace(style) && All.Contains(style.Trim(), StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string? style) =>
        All.FirstOrDefault(x => string.Equals(x, style?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? Outline;
}
