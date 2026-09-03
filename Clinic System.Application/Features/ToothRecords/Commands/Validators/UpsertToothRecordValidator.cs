namespace Clinic_System.Application.Features.ToothRecords.Commands.Validators
{
    public class UpsertToothRecordValidator : AbstractValidator<UpsertToothRecordCommand>
    {
        public UpsertToothRecordValidator()
        {
            RuleFor(x => x.PatientId).GreaterThan(0);
            RuleFor(x => x.ToothNumber)
                .Must(FdiToothNumber.IsValid)
                .WithMessage("El diente debe usar una notación FDI válida.");
        }
    }
}
