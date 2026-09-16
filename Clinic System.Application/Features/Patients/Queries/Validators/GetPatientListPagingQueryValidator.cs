namespace Clinic_System.Application.Features.Patients.Queries.Validators
{
    public class GetPatientListPagingQueryValidator : AbstractValidator<GetPatientListPagingQuery>
    {
        public GetPatientListPagingQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.Status)
                .Must(s => string.IsNullOrWhiteSpace(s)
                    || s.Equals("all", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("active", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Status must be all, active or inactive.");

            RuleFor(x => x.Search)
                .MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Search));
        }
    }
}
