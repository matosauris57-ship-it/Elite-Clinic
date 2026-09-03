namespace Clinic_System.Application.Features.Periodontogram.Validators;

public class ListPeriodontalExamsValidator : AbstractValidator<ListPeriodontalExamsQuery>
{
    public ListPeriodontalExamsValidator() => RuleFor(x => x.PatientId).GreaterThan(0);
}

public class GetPeriodontalExamValidator : AbstractValidator<GetPeriodontalExamQuery>
{
    public GetPeriodontalExamValidator() => RuleFor(x => x.ExamId).GreaterThan(0);
}

public class ComparePeriodontalExamsValidator : AbstractValidator<ComparePeriodontalExamsQuery>
{
    public ComparePeriodontalExamsValidator()
    {
        RuleFor(x => x.PreviousExamId).GreaterThan(0);
        RuleFor(x => x.CurrentExamId).GreaterThan(0).NotEqual(x => x.PreviousExamId);
    }
}

public class CreatePeriodontalExamValidator : AbstractValidator<CreatePeriodontalExamCommand>
{
    public CreatePeriodontalExamValidator() => RuleFor(x => x.PatientId).GreaterThan(0);
}

public class SavePeriodontalExamValidator : AbstractValidator<SavePeriodontalExamCommand>
{
    public SavePeriodontalExamValidator()
    {
        RuleFor(x => x.ExamId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Teeth).NotNull();
        RuleForEach(x => x.Teeth).SetValidator(new PeriodontalToothValidator());
    }
}

public class PeriodontalToothValidator : AbstractValidator<PeriodontalToothDTO>
{
    public PeriodontalToothValidator()
    {
        RuleFor(x => x.ToothNumber)
            .Must(n => FdiToothNumber.IsValid(n) && FdiToothNumber.IsPermanent(n))
            .WithMessage("El diente debe usar FDI permanente.");
        RuleFor(x => x.Mobility).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Furcation).IsInEnum();
        RuleFor(x => x.FacialFurcation).IsInEnum();
        RuleFor(x => x.LingualFurcation).IsInEnum();
        RuleFor(x => x.KeratinizedGingivaMm)
            .Must(PeriodontalCalculations.IsValidMillimeters)
            .WithMessage("La encía queratinizada debe ser un entero entre 0 y 15 mm.");
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleForEach(x => x.Sites).SetValidator(new PeriodontalSiteValidator());
        RuleFor(x => x.Sites)
            .Must(sites => sites.Select(s => (s.Surface, s.Position)).Distinct().Count() == sites.Count)
            .WithMessage("No se permiten sitios periodontales duplicados en la misma pieza.");
        RuleFor(x => x)
            .Must(x => x.Status != PeriodontalToothStatus.Present || FdiToothNumber.HasFurcation(x.ToothNumber) ||
                       (x.FacialFurcation == PeriodontalFurcation.Grade0 && x.LingualFurcation == PeriodontalFurcation.Grade0 && x.Furcation == PeriodontalFurcation.Grade0))
            .WithMessage("La furcación solo se registra en molares y primeros premolares superiores.");
    }
}

public class PeriodontalSiteValidator : AbstractValidator<PeriodontalSiteDTO>
{
    public PeriodontalSiteValidator()
    {
        RuleFor(x => x.Surface).IsInEnum();
        RuleFor(x => x.Position).IsInEnum();
        RuleFor(x => x.ProbingDepthMm)
            .Must(PeriodontalCalculations.IsValidMillimeters)
            .WithMessage("La profundidad de sondaje debe ser un entero entre 0 y 15 mm.");
        RuleFor(x => x.RecessionMm)
            .Must(PeriodontalCalculations.IsValidMillimeters)
            .WithMessage("La recesión debe ser un entero entre 0 y 15 mm.");
    }
}

public class DeletePeriodontalExamValidator : AbstractValidator<DeletePeriodontalExamCommand>
{
    public DeletePeriodontalExamValidator() => RuleFor(x => x.ExamId).GreaterThan(0);
}
