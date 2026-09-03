namespace Clinic_System.Application.Features.DentalTreatments.Commands.Validators
{
    public class CreateDentalTreatmentValidator : AbstractValidator<CreateDentalTreatmentCommand>
    {
        public CreateDentalTreatmentValidator()
        {
            RuleFor(x => x.PatientId).GreaterThan(0);
            RuleFor(x => x.ProcedureName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ProcedureDetails).MaximumLength(2000);
            RuleFor(x => x.MedicalNotes).MaximumLength(4000);
            RuleFor(x => x.TreatmentProcedureId).GreaterThan(0).When(x => x.TreatmentProcedureId.HasValue);
            RuleFor(x => x.ToothNumber!.Value)
                .Must(FdiToothNumber.IsValid)
                .When(x => x.ToothNumber.HasValue);
            RuleFor(x => x.ToothSurface).IsInEnum().When(x => x.ToothSurface.HasValue);
        }
    }
}
