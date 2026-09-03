namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Validators
{
    public class CreateTreatmentPlanValidator : AbstractValidator<CreateTreatmentPlanCommand>
    {
        public CreateTreatmentPlanValidator()
        {
            RuleFor(x => x.PatientId).GreaterThan(0);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Notes).MaximumLength(2000);
            RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Items).NotEmpty();
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProcedureName).NotEmpty();
                item.RuleFor(i => i.ProcedureName).MaximumLength(200);
                item.RuleFor(i => i.TreatmentProcedureId).GreaterThan(0).When(i => i.TreatmentProcedureId.HasValue);
                item.RuleFor(i => i.ToothNumber!.Value)
                    .Must(FdiToothNumber.IsValid)
                    .When(i => i.ToothNumber.HasValue);
                item.RuleFor(i => i.ToothSurface).IsInEnum().When(i => i.ToothSurface.HasValue);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
                item.RuleFor(i => i.Notes).MaximumLength(1000);
            });
        }
    }
}
