namespace Clinic_System.Application.Features.Patients.Commands.Validators
{
    public class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
    {
        public CreatePatientValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Patient Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{10,15}$")
                .WithMessage("Phone number must contain 10–15 digits (numbers only, optional +)");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of Birth is required")
                .LessThan(DateTime.Now).WithMessage("Date of Birth must be in the past");

            RuleFor(x => x.Phone)
                .MustAsync(async (phone, cancellationToken) =>
                {
                    var existingDoctors = await unitOfWork.DoctorsRepository.FindAsync(d => d.Phone == phone);
                    var existingPatients = await unitOfWork.PatientsRepository.FindAsync(d => d.Phone == phone);
                    return !existingDoctors.Any() && !existingPatients.Any();
                })
                .WithMessage("Phone number is already exists");

            RuleFor(x => x.NationalId)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

            RuleFor(x => x.NationalId)
                .MustAsync(async (nationalId, cancellationToken) =>
                {
                    var trimmed = nationalId!.Trim();
                    var existing = await unitOfWork.PatientsRepository.FindAsync(p => p.NationalId == trimmed);
                    return !existing.Any();
                })
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId))
                .WithMessage("National ID is already exists");

            RuleFor(x => x.Email)
                .Custom((email, context) =>
                {
                    if (!ContactEmail.TryValidate(email, out _, out var error) && error != null)
                        context.AddFailure(error);
                });
        }
    }
}
