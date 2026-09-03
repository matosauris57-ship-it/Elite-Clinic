namespace Clinic_System.Application.Features.DentalTreatments.Commands.Validators;

public class CompleteDentalTreatmentValidator : AbstractValidator<CompleteDentalTreatmentCommand>
{
    public CompleteDentalTreatmentValidator()
    {
        RuleFor(x => x.TreatmentId).GreaterThan(0);
        When(x => x.ClinicalResult != null, () =>
        {
            RuleFor(x => x.ClinicalResult!.Surface).IsInEnum();
            RuleFor(x => x.ClinicalResult!.Condition).IsInEnum();
            RuleFor(x => x.ClinicalResult!.Severity).IsInEnum()
                .When(x => x.ClinicalResult!.Severity.HasValue);
            RuleFor(x => x.ClinicalResult!.Notes).MaximumLength(1000);
        });
    }
}
