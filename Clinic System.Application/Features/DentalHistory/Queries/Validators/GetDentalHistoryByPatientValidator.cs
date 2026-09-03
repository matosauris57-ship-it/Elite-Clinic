namespace Clinic_System.Application.Features.DentalHistory.Queries.Validators
{
    public class GetDentalHistoryByPatientValidator : AbstractValidator<GetDentalHistoryByPatientQuery>
    {
        public GetDentalHistoryByPatientValidator()
        {
            RuleFor(x => x.PatientId).GreaterThan(0);
        }
    }
}
