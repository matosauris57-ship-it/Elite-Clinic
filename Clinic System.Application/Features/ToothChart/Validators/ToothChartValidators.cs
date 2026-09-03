namespace Clinic_System.Application.Features.ToothChart.Validators;

public class CreateToothChartEntryValidator : AbstractValidator<CreateToothChartEntryCommand>
{
    public CreateToothChartEntryValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.ToothNumber)
            .Must((cmd, tooth) =>
                FdiToothNumber.IsValid(tooth) ||
                (cmd.ToothNumbers != null && cmd.ToothNumbers.Any(FdiToothNumber.IsValid)) ||
                (cmd.BridgeUnits != null && cmd.BridgeUnits.Any(u => FdiToothNumber.IsValid(u.ToothNumber))))
            .WithMessage("El diente debe usar una notación FDI válida.");
        RuleFor(x => x.Surface).IsInEnum();
        RuleFor(x => x.Phase).IsInEnum();
        RuleFor(x => x.Condition).IsInEnum();
        RuleFor(x => x)
            .Must(x => ToothFindingRules.MatchesScope(x.Condition, x.Surface))
            .WithMessage("Esa condición no corresponde al alcance seleccionado (pieza completa o superficie).");
        RuleFor(x => x.CariesType).IsInEnum().When(x => x.CariesType.HasValue);
        RuleFor(x => x.Icdas).IsInEnum().When(x => x.Icdas.HasValue);
        RuleFor(x => x.CariesType)
            .NotNull()
            .When(x => ToothFindingRules.RequiresCariesDetails(x.Condition))
            .WithMessage("Indique el tipo de caries.");
        RuleFor(x => x.Icdas)
            .NotNull()
            .When(x => ToothFindingRules.RequiresCariesDetails(x.Condition))
            .WithMessage("Indique la clasificación ICDAS.");
        RuleFor(x => x.CariesType)
            .Null()
            .When(x => !ToothFindingRules.RequiresCariesDetails(x.Condition));
        RuleFor(x => x.Icdas)
            .Null()
            .When(x => !ToothFindingRules.RequiresCariesDetails(x.Condition));
        RuleFor(x => x.RestorationMaterial).IsInEnum().When(x => x.RestorationMaterial.HasValue);
        RuleFor(x => x.RestorationMaterial)
            .Must((cmd, material) => RestorationMaterialRules.IsAllowed(cmd.ToothNumber, cmd.Condition, material))
            .When(x => x.RestorationMaterial.HasValue)
            .WithMessage("El material de restauración no aplica a esta pieza o condición.");
        RuleFor(x => x.Severity).IsInEnum().When(x => x.Severity.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.ClinicalDiagnosis).MaximumLength(200);
        RuleFor(x => x.ProposedTreatment).MaximumLength(500);
        RuleFor(x => x.AppointmentId).GreaterThan(0).When(x => x.AppointmentId.HasValue);
        RuleFor(x => x)
            .Must(x => x.Condition != ToothCondition.Bridge || x.BridgeUnits.Count == 0 ||
                       ToothBridgeRules.Validate(x.BridgeUnits.Select(u => new BridgeUnit(u.ToothNumber, u.Role)).ToList()) == null)
            .WithMessage(x => ToothBridgeRules.Validate(x.BridgeUnits.Select(u => new BridgeUnit(u.ToothNumber, u.Role)).ToList())
                           ?? "El tramo del puente no es válido.");
    }
}

public class CreateToothChartEntriesBatchValidator : AbstractValidator<CreateToothChartEntriesBatchCommand>
{
    public CreateToothChartEntriesBatchValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.ToothNumbers.Count > 0 || x.BridgeUnits.Count > 0)
            .WithMessage("Seleccione al menos una pieza.");
        RuleFor(x => x.ToothNumbers)
            .Must(x => x.Distinct().Count() <= 32)
            .WithMessage("Puede registrar como máximo 32 piezas a la vez.");
        RuleForEach(x => x.ToothNumbers)
            .Must(FdiToothNumber.IsValid)
            .When(x => x.ToothNumbers.Count > 0)
            .WithMessage("Cada diente debe usar una notación FDI válida.");
        RuleFor(x => x.Surface).IsInEnum();
        RuleFor(x => x.Phase).IsInEnum();
        RuleFor(x => x.Condition).IsInEnum();
        RuleFor(x => x)
            .Must(x => ToothFindingRules.MatchesScope(x.Condition, x.Surface))
            .WithMessage("Esa condición no corresponde al alcance seleccionado (pieza completa o superficie).");
        RuleFor(x => x.CariesType).NotNull().When(x => ToothFindingRules.RequiresCariesDetails(x.Condition))
            .WithMessage("Indique el tipo de caries.");
        RuleFor(x => x.Icdas).NotNull().When(x => ToothFindingRules.RequiresCariesDetails(x.Condition))
            .WithMessage("Indique la clasificación ICDAS.");
        RuleFor(x => x.CariesType)
            .Null()
            .When(x => !ToothFindingRules.RequiresCariesDetails(x.Condition));
        RuleFor(x => x.Icdas)
            .Null()
            .When(x => !ToothFindingRules.RequiresCariesDetails(x.Condition));
        RuleFor(x => x.RestorationMaterial).IsInEnum().When(x => x.RestorationMaterial.HasValue);
        RuleFor(x => x.Severity).IsInEnum().When(x => x.Severity.HasValue);
        RuleFor(x => x.ClinicalDiagnosis).MaximumLength(200);
        RuleFor(x => x.ProposedTreatment).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.AppointmentId).GreaterThan(0).When(x => x.AppointmentId.HasValue);
        RuleFor(x => x)
            .Must(x => x.Condition != ToothCondition.Bridge || x.BridgeUnits.Count == 0 ||
                       ToothBridgeRules.Validate(x.BridgeUnits.Select(u => new BridgeUnit(u.ToothNumber, u.Role)).ToList()) == null)
            .WithMessage(x => ToothBridgeRules.Validate(x.BridgeUnits.Select(u => new BridgeUnit(u.ToothNumber, u.Role)).ToList())
                           ?? "El tramo del puente no es válido.");
    }
}

public class GetCurrentToothChartValidator : AbstractValidator<GetCurrentToothChartQuery>
{
    public GetCurrentToothChartValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.Dentition)
            .Must(x => x is null || x.Equals("permanent", StringComparison.OrdinalIgnoreCase) || x.Equals("deciduous", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Dentition debe ser 'permanent' o 'deciduous'.");
        RuleFor(x => x.Quadrant).InclusiveBetween(1, 8).When(x => x.Quadrant.HasValue);
    }
}

public class GetDentalTimelineValidator : AbstractValidator<GetDentalTimelineQuery>
{
    public GetDentalTimelineValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.ToothNumber!.Value)
            .Must(FdiToothNumber.IsValid)
            .When(x => x.ToothNumber.HasValue)
            .WithMessage("El diente debe usar una notación FDI válida.");
    }
}
