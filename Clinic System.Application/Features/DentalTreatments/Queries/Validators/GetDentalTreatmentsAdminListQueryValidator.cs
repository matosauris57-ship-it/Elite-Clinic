namespace Clinic_System.Application.Features.DentalTreatments.Queries.Validators
{
    public class GetDentalTreatmentsAdminListQueryValidator : AbstractValidator<GetDentalTreatmentsAdminListQuery>
    {
        public GetDentalTreatmentsAdminListQueryValidator()
        {
            RuleFor(x => x.Search)
                .MaximumLength(80)
                .When(x => !string.IsNullOrWhiteSpace(x.Search));

            RuleFor(x => x)
                .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.ToDate.Value.Date >= x.FromDate.Value.Date)
                .WithMessage("ToDate must be on or after FromDate.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .When(x => x.IsPaged);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .When(x => x.IsPaged);
        }
    }
}
