namespace Clinic_System.Application.Features.Appointments.Commands.Validators
{
    public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;
        private readonly IClinicOperatingHoursService operatingHours;

        public BookAppointmentCommandValidator(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IClinicOperatingHoursService operatingHours)
        {
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
            this.operatingHours = operatingHours;

            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .MustAsync(DoctorExists)
                .WithMessage("Doctor not found");

            RuleFor(x => x.PatientId)
                .GreaterThan(0)
                .MustAsync(PatientExists)
                .WithMessage("Patient not found")
                .When(x => currentUserService.PatientId == null);

            RuleFor(x => x.AppointmentDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("La fecha de la cita no puede estar en el pasado.")
                .Must((command, date) => date.Date.Add(command.AppointmentTime) > DateTime.Now)
                .WithMessage("La fecha y hora de la cita deben ser posteriores a la hora actual.");

            RuleFor(x => x.AppointmentTime)
                .NotEmpty()
                .WithMessage("La hora de la cita es obligatoria.")
                .MustAsync(BeWithinClinicHours)
                .WithMessage("El horario está fuera del horario de trabajo de la clínica.");

            RuleFor(x => x.QuotedAmount)
                .GreaterThan(0)
                .When(x => x.QuotedAmount.HasValue)
                .WithMessage("El precio del tratamiento debe ser mayor a 0.");
        }

        private async Task<bool> DoctorExists(int doctorId, CancellationToken cancellationToken)
        {
            var doctor = await unitOfWork.DoctorsRepository
                .GetByIdAsync(doctorId, cancellationToken);

            return doctor != null;
        }

        private async Task<bool> PatientExists(int patientId, CancellationToken cancellationToken)
        {
            var patient = await unitOfWork.PatientsRepository
                .GetByIdAsync(patientId, cancellationToken);

            return patient != null;
        }

        private async Task<bool> BeWithinClinicHours(BookAppointmentCommand command, TimeSpan time, CancellationToken cancellationToken)
        {
            var hours = await operatingHours.GetAsync(cancellationToken);
            return hours.Allows(command.AppointmentDate, time);
        }
    }
}
