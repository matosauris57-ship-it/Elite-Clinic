namespace Clinic_System.Application.Features.Appointments.Queries.Validators
{
    public class GetAdminAgendaQueryValidator : AbstractValidator<GetAdminAgendaQuery>
    {
        public GetAdminAgendaQueryValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .When(x => !x.IsPaged)
                .WithMessage("Date is required.");

            RuleFor(x => x)
                .Must(x => x.EndDate!.Value.Date >= x.Date!.Value.Date)
                .When(x => x.Date.HasValue && x.EndDate.HasValue)
                .WithMessage("EndDate must be on or after Date.");

            RuleFor(x => x)
                .Must(x => !x.Date.HasValue || !x.EndDate.HasValue || (x.EndDate.Value.Date - x.Date.Value.Date).TotalDays <= 31)
                .When(x => !x.IsPaged)
                .WithMessage("The agenda range cannot exceed 31 days.");

            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .When(x => x.DoctorId.HasValue)
                .WithMessage("Invalid doctor ID.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("Invalid appointment status.");

            RuleFor(x => x.Search)
                .MaximumLength(80)
                .When(x => !string.IsNullOrWhiteSpace(x.Search));

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .When(x => x.IsPaged)
                .WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .When(x => x.IsPaged)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}
