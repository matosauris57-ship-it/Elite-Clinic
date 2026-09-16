using FluentValidation.Results;

namespace Clinic_System.Application.Features.Patients.Commands.Validators
{
    public class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePatientValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.Id).NotEmpty().WithMessage("Patient ID is required for update.");

            ApplyValidationsRules();
            ApplyCustomValidationsRules();
        }
        public void ApplyValidationsRules()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(PatientFieldLimits.FullName).WithMessage($"Name must not exceed {PatientFieldLimits.FullName} characters")
                .Must(PersonNameRules.IsValid).WithMessage(PersonNameRules.InvalidNameMessage)
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.Address)
                .MaximumLength(PatientFieldLimits.Address).WithMessage($"Address must not exceed {PatientFieldLimits.Address} characters")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.Phone)
                .Must(phone => PatientFieldLimits.IsValidPhone(phone, required: false))
                .When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone number must contain 10–15 digits (numbers only, optional +)");

            RuleFor(x => x.NationalId)
                .MaximumLength(PatientFieldLimits.NationalId)
                .Must(PatientFieldLimits.IsValidNationalId)
                .WithMessage("National ID format is invalid")
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

            RuleFor(x => x.Email)
                .MaximumLength(PatientFieldLimits.Email)
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage($"Email must not exceed {PatientFieldLimits.Email} characters");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today)
                .WithMessage("Date of Birth must be in the past")
                .GreaterThan(new DateTime(1900, 1, 1))
                .WithMessage("Date of Birth is not valid")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Gender)
                .Must(g => Enum.TryParse<Gender>(g, true, out _))
                .WithMessage("Gender is invalid")
                .When(x => !string.IsNullOrWhiteSpace(x.Gender));
        }

        public void ApplyCustomValidationsRules()
        {
            RuleFor(x => x.Phone)
                .MustAsync(async (command, phone, cancellationToken) =>
                {
                    var existingDoctor = await _unitOfWork.DoctorsRepository.FindAsync(d => d.Phone == phone);
                    var existingPatient = await _unitOfWork.PatientsRepository.FindAsync(d => d.Phone == phone);

                    bool phoneUsedByOther =
                        existingDoctor.Any()
                        || existingPatient.Any(d => d.Id != command.Id);

                    return !phoneUsedByOther;
                })
                .WithMessage("Phone number is already exists")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.NationalId)
                .CustomAsync(async (nationalId, context, _) =>
                {
                    var command = (UpdatePatientCommand)context.InstanceToValidate;
                    var existing = await PatientNationalIdRules.FindDuplicateAsync(_unitOfWork, nationalId, command.Id);
                    if (existing != null)
                    {
                        context.AddFailure(new ValidationFailure(nameof(UpdatePatientCommand.NationalId), "National ID is already exists")
                        {
                            CustomState = existing.Id
                        });
                    }
                })
                .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

            RuleFor(x => x.Email)
                .Custom((email, context) =>
                {
                    if (!ContactEmail.TryValidate(email, out _, out var error) && error != null)
                        context.AddFailure(error);
                });
        }
    }
}
