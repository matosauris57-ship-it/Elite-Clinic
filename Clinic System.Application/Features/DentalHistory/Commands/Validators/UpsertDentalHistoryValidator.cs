namespace Clinic_System.Application.Features.DentalHistory.Commands.Validators
{
    public class UpsertDentalHistoryValidator : AbstractValidator<UpsertDentalHistoryCommand>
    {
        public UpsertDentalHistoryValidator()
        {
            RuleFor(x => x.PatientId).GreaterThan(0);
        }
    }
}
