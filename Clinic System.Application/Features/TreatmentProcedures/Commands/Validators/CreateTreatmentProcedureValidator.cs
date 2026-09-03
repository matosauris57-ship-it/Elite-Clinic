namespace Clinic_System.Application.Features.TreatmentProcedures.Commands.Validators
{
    public class CreateTreatmentProcedureValidator : AbstractValidator<CreateTreatmentProcedureCommand>
    {
        public CreateTreatmentProcedureValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DurationMinutes).GreaterThan(0);
            RuleFor(x => x.DoctorPrices)
                .Must(prices => prices.Select(p => p.DoctorId).Distinct().Count() == prices.Count)
                .WithMessage("Each doctor can have only one price per procedure.");
            RuleForEach(x => x.DoctorPrices).ChildRules(price =>
            {
                price.RuleFor(p => p.DoctorId).GreaterThan(0);
                price.RuleFor(p => p.Price).GreaterThanOrEqualTo(0);
            });
        }
    }
}
