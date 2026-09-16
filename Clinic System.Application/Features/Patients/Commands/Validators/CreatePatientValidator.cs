using FluentValidation.Results;

namespace Clinic_System.Application.Features.Patients.Commands.Validators
{
    public class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
    {
        public CreatePatientValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Patient Name is required")
                .MaximumLength(PatientFieldLimits.FullName)
                .WithMessage($"Name must not exceed {PatientFieldLimits.FullName} characters")
                .Must(PersonNameRules.IsValid)
                .WithMessage(PersonNameRules.InvalidNameMessage);

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(PatientFieldLimits.Address)
                .WithMessage($"Address must not exceed {PatientFieldLimits.Address} characters");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Must(phone => PatientFieldLimits.IsValidPhone(phone, required: true))
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
                .MaximumLength(PatientFieldLimits.NationalId)
                .Must(PatientFieldLimits.IsValidNationalId)
                .WithMessage("National ID format is invalid")
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

            RuleFor(x => x.NationalId)
                .CustomAsync(async (nationalId, context, _) =>
                {
                    var existing = await PatientNationalIdRules.FindDuplicateAsync(unitOfWork, nationalId);
                    if (existing != null)
                    {
                        context.AddFailure(new ValidationFailure(nameof(CreatePatientCommand.NationalId), "National ID is already exists")
                        {
                            CustomState = existing.Id
                        });
                    }
                })
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

            RuleFor(x => x.Email)
                .MaximumLength(PatientFieldLimits.Email)
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage($"Email must not exceed {PatientFieldLimits.Email} characters");

            RuleFor(x => x.Email)
                .Custom((email, context) =>
                {
                    if (!ContactEmail.TryValidate(email, out _, out var error) && error != null)
                        context.AddFailure(error);
                });
        }
    }
}
