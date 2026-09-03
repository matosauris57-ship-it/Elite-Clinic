using System.Text.Json.Serialization;
using Clinic_System.Core.Enums;

namespace Clinic_System.Core.Odontogram;

public sealed class OdontogramSymbolConfigDocument
{
    public const string DefaultClinicKey = "default";

    public string ClinicKey { get; set; } = DefaultClinicKey;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<OdontogramConditionSymbol> Conditions { get; set; } = [];
    public List<OdontogramPhaseSymbol> Phases { get; set; } = [];
    public List<OdontogramSymbolConfigChange> History { get; set; } = [];

    public OdontogramConditionSymbol ForCondition(ToothCondition condition) =>
        Conditions.FirstOrDefault(x => x.Condition == condition)
        ?? OdontogramSymbolDefaults.Condition(condition);

    public OdontogramPhaseSymbol ForPhase(ToothChartPhase phase) =>
        Phases.FirstOrDefault(x => x.Phase == phase)
        ?? OdontogramSymbolDefaults.Phase(phase);
}

public sealed class OdontogramConditionSymbol
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ToothCondition Condition { get; set; }

    public string Shape { get; set; } = OdontogramSymbolShapes.Circle;
    public string Color { get; set; } = "#EF5350";
    public string Style { get; set; } = OdontogramSymbolStyles.Outline;
    public double Opacity { get; set; } = 0.85;
    public double StrokeWidth { get; set; } = 2;
    public string? CustomSvg { get; set; }
    public bool Enabled { get; set; } = true;

    public OdontogramConditionSymbol Clone() => new()
    {
        Condition = Condition,
        Shape = Shape,
        Color = Color,
        Style = Style,
        Opacity = Opacity,
        StrokeWidth = StrokeWidth,
        CustomSvg = CustomSvg,
        Enabled = Enabled
    };
}

public sealed class OdontogramPhaseSymbol
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ToothChartPhase Phase { get; set; }

    public string Color { get; set; } = "#EF5350";
    public double Opacity { get; set; } = 1;

    public OdontogramPhaseSymbol Clone() => new()
    {
        Phase = Phase,
        Color = Color,
        Opacity = Opacity
    };
}

public sealed class OdontogramSymbolConfigChange
{
    public DateTimeOffset At { get; set; }
    public string User { get; set; } = "";
    public string ClinicKey { get; set; } = OdontogramSymbolConfigDocument.DefaultClinicKey;
    public string Action { get; set; } = "save";
    public List<OdontogramConditionSymbol> PreviousConditions { get; set; } = [];
    public List<OdontogramConditionSymbol> NewConditions { get; set; } = [];
    public List<OdontogramPhaseSymbol> PreviousPhases { get; set; } = [];
    public List<OdontogramPhaseSymbol> NewPhases { get; set; } = [];
}
