namespace Clinic_System.Core.Validation;

public sealed record BridgeUnit(int ToothNumber, BridgeRole Role);

public static class ToothBridgeRules
{
    public static int[] PermanentUpper { get; } =
        [18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28];

    public static int[] PermanentLower { get; } =
        [48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38];

    public static int[] DeciduousUpper { get; } =
        [55, 54, 53, 52, 51, 61, 62, 63, 64, 65];

    public static int[] DeciduousLower { get; } =
        [85, 84, 83, 82, 81, 71, 72, 73, 74, 75];

    public static bool IsAbsentCondition(ToothCondition? condition, BridgeRole? role = null) =>
        role == BridgeRole.Pontic
        || condition is ToothCondition.Missing or ToothCondition.Extracted
        || (condition == ToothCondition.Bridge && role == BridgeRole.Pontic);

    public static int[]? ArchOrderFor(IReadOnlyCollection<int> teeth)
    {
        if (teeth.Count == 0 || teeth.Any(t => !FdiToothNumber.IsValid(t)))
            return null;

        var upper = teeth.All(FdiToothNumber.IsUpper);
        var lower = teeth.All(t => !FdiToothNumber.IsUpper(t));
        if (!upper && !lower)
            return null;

        var permanent = teeth.All(FdiToothNumber.IsPermanent);
        var deciduous = teeth.All(t => !FdiToothNumber.IsPermanent(t));
        if (!permanent && !deciduous)
            return null;

        if (permanent)
            return upper ? PermanentUpper : PermanentLower;
        return upper ? DeciduousUpper : DeciduousLower;
    }

    public static List<int> ExpandSpan(IReadOnlyCollection<int> selected)
    {
        var distinct = selected.Distinct().ToList();
        var arch = ArchOrderFor(distinct);
        if (arch == null || distinct.Count == 0)
            return distinct;

        var indices = distinct
            .Select(t => Array.IndexOf(arch, t))
            .Where(i => i >= 0)
            .OrderBy(i => i)
            .ToList();
        if (indices.Count == 0)
            return distinct;

        return arch.Skip(indices[0]).Take(indices[^1] - indices[0] + 1).ToList();
    }

    public static List<BridgeUnit> InferRoles(
        IReadOnlyCollection<int> selected,
        Func<int, bool> isAbsent)
    {
        var span = ExpandSpan(selected);
        return span.Select(t => new BridgeUnit(t, isAbsent(t) ? BridgeRole.Pontic : BridgeRole.Abutment)).ToList();
    }

    public static string? Validate(IReadOnlyList<BridgeUnit> units)
    {
        var teeth = units.Select(u => u.ToothNumber).ToList();
        if (teeth.Distinct().Count() != teeth.Count)
            return "El tramo no puede repetir la misma pieza.";
        if (teeth.Count < 2)
            return "Seleccione al menos dos piezas para el puente.";

        var arch = ArchOrderFor(teeth);
        if (arch == null)
            return "El tramo debe estar en la misma arcada (no mezcle superior e inferior ni dentición distinta).";

        var expected = ExpandSpan(teeth);
        var ordered = teeth
            .OrderBy(t => Array.IndexOf(arch, t))
            .ToList();
        if (!expected.SequenceEqual(ordered))
            return "El tramo debe ser continuo a lo largo de la arcada.";

        if (units.All(u => u.Role != BridgeRole.Abutment) || units.All(u => u.Role != BridgeRole.Pontic))
            return "Marque el hueco (póntico) o deje ausente la pieza intermedia. El puente necesita al menos un pilar y un póntico.";

        return null;
    }
}
