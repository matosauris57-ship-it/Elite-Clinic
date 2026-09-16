namespace Clinic_System.Application.Features.Doctors.Commands.Validators
{
    public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;

        public UpdateDoctorValidator(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;

            ApplyValidationsRules();
            ApplyCustomValidationsRules();
        }
        public void ApplyValidationsRules()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Doctor ID is required for update.");

            // Name
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
                .Must(PersonNameRules.IsValid).WithMessage(PersonNameRules.InvalidNameMessage)
                .When(x => !string.IsNullOrEmpty(x.FullName));

            // Address & Specialization
            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.Specialization)
                .MaximumLength(100).WithMessage("Specialization must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Specialization));

            RuleFor(x => x.SignatureImageUrl)
                .MaximumLength(1_500_000).WithMessage("Signature image is too large")
                .Must(BeValidSignatureDataUrl).WithMessage("Signature image must be a PNG, JPG or WebP data URL")
                .When(x => !string.IsNullOrWhiteSpace(x.SignatureImageUrl));

            // Phone (Format Only)
            RuleFor(x => x.Phone)
                .Must(phone => PatientFieldLimits.IsValidPhone(phone, required: false))
                .When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone number must contain 10–15 digits (numbers only, optional +)");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.UserName)
                .Matches(@"^(?=.*\d)[A-Za-z][A-Za-z0-9_]*$")
                .WithMessage("Username must start with a letter and contain at least one number.")
                .When(x => !string.IsNullOrWhiteSpace(x.UserName));

            RuleFor(x => x.Password)
                .PasswordRule()
                .When(x => !string.IsNullOrWhiteSpace(x.Password));

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Password and Confirm Password do not match")
                .When(x => !string.IsNullOrWhiteSpace(x.Password));
        }

        public void ApplyCustomValidationsRules()
        {
            // 3. Check Phone Uniqueness (Using UnitOfWork -> Doctor Repo)
            RuleFor(x => x.Phone)
                .MustAsync(async (command, phone, cancellationToken) =>
                {
                    // Search in Doctors table
                    // تأكد أن لديك ميثود FindAsync أو استخدم AnyAsync لو متاحة
                    var existingDoctor = await _unitOfWork.DoctorsRepository.FindAsync(d => d.Phone == phone);
                    var existingPatient = await _unitOfWork.PatientsRepository.FindAsync(d => d.Phone == phone);
                    
                    bool phoneUsedByOther =
                        existingDoctor.Any(d => d.Id != command.Id)
                        || existingPatient.Any();

                    return !phoneUsedByOther; // Valid if no one else uses it
                })
                .WithMessage("Phone number is already exists")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Email)
                .MustAsync(async (command, email, cancellationToken) =>
                {
                    var doctor = await _unitOfWork.DoctorsRepository.GetByIdAsync(command.Id, cancellationToken);
                    return await _identityService.IsEmailUniqueAsync(email, doctor?.ApplicationUserId, cancellationToken);
                })
                .WithMessage("Email is already exists")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.UserName)
                .MustAsync(async (command, userName, cancellationToken) =>
                {
                    var doctor = await _unitOfWork.DoctorsRepository.GetByIdAsync(command.Id, cancellationToken);
                    return await _identityService.IsUserNameUniqueAsync(userName, doctor?.ApplicationUserId, cancellationToken);
                })
                .WithMessage("Username is already exists")
                .When(x => !string.IsNullOrWhiteSpace(x.UserName));
        }

        private static bool BeValidSignatureDataUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            return value.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("data:image/jpg;base64,", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase);
        }
    }
}
