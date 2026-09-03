namespace Clinic_System.Application.Features.ToothRecords.Commands.Validators;

public class BatchUpsertOdontogramValidator : AbstractValidator<BatchUpsertOdontogramCommand>
{
    public BatchUpsertOdontogramValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.Teeth).NotEmpty();
        RuleFor(x => x.Teeth)
            .Must(teeth => teeth.Select(x => x.ToothNumber).Distinct().Count() == teeth.Count)
            .WithMessage("El lote no puede contener el mismo diente más de una vez.");
        RuleForEach(x => x.Teeth).ChildRules(tooth =>
        {
            tooth.RuleFor(x => x.ToothNumber)
                .Must(FdiToothNumber.IsValid)
                .WithMessage("El diente debe usar una notación FDI válida.");
            tooth.RuleFor(x => x.Notes).MaximumLength(500);
        });
    }
}
