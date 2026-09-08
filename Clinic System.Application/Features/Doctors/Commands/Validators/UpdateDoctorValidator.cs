namespace Clinic_System.Application.Features.Doctors.Commands.Validators
{
    public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoctorValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.Id).NotEmpty().WithMessage("Doctor ID is required for update.");


            // تقسيم القواعد لتكون منظمة
            ApplyValidationsRules();
            ApplyCustomValidationsRules();
        }
        public void ApplyValidationsRules()
        {
            // Name
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
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
                .Matches(@"^\+?[0-9]{10,15}$")
                .When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone number must contain 10–15 digits (numbers only, optional +)");
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
