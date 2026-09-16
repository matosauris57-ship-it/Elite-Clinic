using System.Text.Json;

namespace Clinic_System.Core.Dashboard;

public static class DashboardLayoutEngine
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static DashboardLayoutDocument ParseOrDefault(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return DashboardWidgetCatalog.CreateDefaultLayout();

        try
        {
            var parsed = JsonSerializer.Deserialize<DashboardLayoutDocument>(json, JsonOptions);
            if (parsed?.Items == null || parsed.Items.Count == 0)
                return DashboardWidgetCatalog.CreateDefaultLayout();
            return parsed;
        }
        catch (JsonException)
        {
            return DashboardWidgetCatalog.CreateDefaultLayout();
        }
    }

    public static string Serialize(DashboardLayoutDocument document) =>
        JsonSerializer.Serialize(Normalize(document), JsonOptions);

    public static DashboardLayoutDocument Normalize(DashboardLayoutDocument document)
    {
        var defaults = DashboardWidgetCatalog.CreateDefaultLayout();
        var byKey = document.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.WidgetKey))
            .GroupBy(i => i.WidgetKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var result = new DashboardLayoutDocument { Version = 1 };
        foreach (var definition in DashboardWidgetCatalog.All)
        {
            if (!byKey.TryGetValue(definition.Key, out var item))
                item = defaults.Items.First(i => i.WidgetKey == definition.Key);

            item.Id = string.IsNullOrWhiteSpace(item.Id) ? definition.Key : item.Id;
            item.WidgetKey = definition.Key;
            item.W = Math.Clamp(item.W, definition.MinW, definition.MaxW);
            item.H = Math.Clamp(item.H, definition.MinH, definition.MaxH);
            item.X = Math.Clamp(item.X, 0, DashboardWidgetCatalog.Columns - item.W);
            item.Y = Math.Max(0, item.Y);
            item.Settings ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (definition.Required)
                item.Visible = true;
            result.Items.Add(item);
        }

        return result;
    }

    public static DashboardLayoutDocument ApplyClinicAvailability(
        DashboardLayoutDocument userLayout,
        DashboardLayoutDocument clinicLayout)
    {
        var storedClinicKeys = clinicLayout.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.WidgetKey))
            .Select(i => i.WidgetKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var clinicNormalized = Normalize(clinicLayout);
        var clinicVisible = clinicNormalized.Items
            .Where(i => i.Visible)
            .Select(i => i.WidgetKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in DashboardWidgetCatalog.All.Where(d => d.Required))
            clinicVisible.Add(definition.Key);

        var userHadKeys = userLayout.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.WidgetKey))
            .Select(i => i.WidgetKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var defaults = DashboardWidgetCatalog.CreateDefaultLayout();
        var normalized = Normalize(userLayout);
        foreach (var item in normalized.Items)
        {
            if (!storedClinicKeys.Contains(item.WidgetKey))
            {
                var fallback = defaults.Items.First(i => i.WidgetKey == item.WidgetKey);
                var shouldShow = fallback.Visible
                    || (DashboardWidgetCatalog.TryGet(item.WidgetKey, out var created) && created.Required);
                if (!userHadKeys.Contains(item.WidgetKey))
                {
                    item.Visible = shouldShow;
                    if (item.Visible)
                    {
                        item.X = fallback.X;
                        item.Y = fallback.Y;
                        item.W = fallback.W;
                        item.H = fallback.H;
                        PlaceIfNeeded(normalized, item);
                    }
                }
                else if (shouldShow)
                {
                    item.Visible = true;
                }
                continue;
            }

            if (!clinicVisible.Contains(item.WidgetKey) &&
                DashboardWidgetCatalog.TryGet(item.WidgetKey, out var def) &&
                !def.Required)
            {
                item.Visible = false;
            }
        }

        return normalized;
    }

    public static DashboardLayoutDocument FilterByPermissions(
        DashboardLayoutDocument layout,
        Func<string, bool> hasPermission)
    {
        var normalized = Normalize(layout);
        foreach (var item in normalized.Items)
        {
            if (!DashboardWidgetCatalog.TryGet(item.WidgetKey, out var def))
            {
                item.Visible = false;
                continue;
            }

            if (!hasPermission(def.Permission))
                item.Visible = false;
            else if (def.Required)
                item.Visible = true;
        }

        return normalized;
    }

    public static IReadOnlyList<DashboardWidgetPlacement> VisibleItems(DashboardLayoutDocument layout) =>
        layout.Items.Where(i => i.Visible).OrderBy(i => i.Y).ThenBy(i => i.X).ToList();

    public static DashboardLayoutDocument ApplyPositions(
        DashboardLayoutDocument layout,
        IEnumerable<DashboardWidgetPlacement> positions)
    {
        var normalized = Normalize(layout);
        var map = new Dictionary<string, DashboardWidgetPlacement>(StringComparer.OrdinalIgnoreCase);
        foreach (var pos in positions)
        {
            if (!string.IsNullOrWhiteSpace(pos.Id))
                map[pos.Id] = pos;
            if (!string.IsNullOrWhiteSpace(pos.WidgetKey))
                map[pos.WidgetKey] = pos;
        }
        foreach (var item in normalized.Items)
        {
            if (!map.TryGetValue(item.Id, out var pos) && !map.TryGetValue(item.WidgetKey, out pos))
                continue;
            item.X = pos.X;
            item.Y = pos.Y;
            item.W = pos.W;
            item.H = pos.H;
        }

        return Normalize(normalized);
    }

    public static DashboardLayoutDocument ForceVisible(DashboardLayoutDocument layout, string widgetKey)
    {
        var normalized = SetVisible(layout, widgetKey, true);
        var item = normalized.Items.FirstOrDefault(i =>
            string.Equals(i.WidgetKey, widgetKey, StringComparison.OrdinalIgnoreCase));
        if (item == null)
            return normalized;

        var anchor = normalized.Items.FirstOrDefault(i =>
            i.Visible && string.Equals(i.WidgetKey, DashboardWidgetKeys.SmartAlerts, StringComparison.OrdinalIgnoreCase));
        if (anchor != null)
        {
            item.X = anchor.X;
            item.Y = anchor.Y + anchor.H;
            var maxW = Math.Max(1, DashboardWidgetCatalog.Columns - item.X);
            item.W = Math.Clamp(Math.Max(anchor.W, 4), 3, maxW);
        }

        PlaceIfNeeded(normalized, item);
        return normalized;
    }

    public static DashboardLayoutDocument SetVisible(DashboardLayoutDocument layout, string widgetKey, bool visible)
    {
        var normalized = Normalize(layout);
        var item = normalized.Items.FirstOrDefault(i =>
            string.Equals(i.WidgetKey, widgetKey, StringComparison.OrdinalIgnoreCase));
        if (item == null)
            return normalized;
        if (DashboardWidgetCatalog.TryGet(widgetKey, out var def) && def.Required)
            item.Visible = true;
        else
            item.Visible = visible;
        if (visible)
            PlaceIfNeeded(normalized, item);
        return normalized;
    }

    public static DashboardLayoutDocument ApplyPresetSize(DashboardLayoutDocument layout, string widgetKey, string preset)
    {
        var normalized = Normalize(layout);
        if (!DashboardWidgetCatalog.TryGet(widgetKey, out var def))
            return normalized;
        var item = normalized.Items.First(i => i.WidgetKey == def.Key);
        var size = preset.ToLowerInvariant() switch
        {
            "small" => def.Small,
            "large" => def.Large,
            _ => def.Medium
        };
        item.W = size.W;
        item.H = size.H;
        return Normalize(normalized);
    }

    public static void PlaceIfNeeded(DashboardLayoutDocument layout, DashboardWidgetPlacement item)
    {
        if (layout.Items.Any(i => i.Visible && i.Id != item.Id && Overlaps(i, item)))
        {
            item.X = 0;
            item.Y = layout.Items.Where(i => i.Visible && i.Id != item.Id).Select(i => i.Y + i.H).DefaultIfEmpty(0).Max();
        }
    }

    private static bool Overlaps(DashboardWidgetPlacement a, DashboardWidgetPlacement b) =>
        a.X < b.X + b.W && a.X + a.W > b.X && a.Y < b.Y + b.H && a.Y + a.H > b.Y;
}
