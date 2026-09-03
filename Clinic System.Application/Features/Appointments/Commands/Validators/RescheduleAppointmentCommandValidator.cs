namespace Clinic_System.Application.Features.Appointments.Commands.Validators
{
    public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;
        private readonly IClinicOperatingHoursService operatingHours;
        public RescheduleAppointmentCommandValidator(
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
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0)
                .MustAsync(AppointmentExists)
                .WithMessage("Appointment not found");

            RuleFor(x => x.PatientId)
                .GreaterThan(0).WithMessage("Invalid Patient ID.")
                .When(x => currentUserService.PatientId == null);

            RuleFor(x => x)
               .MustAsync(NewDateTimeDifferentFromOld)
               .WithMessage("New appointment date and time must be different from the current appointment")
               .When(x => x.AppointmentDate >= DateTime.Today);

            RuleFor(x => x.AppointmentDate)
               .GreaterThanOrEqualTo(DateTime.Today)
               .WithMessage("Appointment date cannot be in the past");

            RuleFor(x => x.AppointmentTime)
            .NotEmpty()
            .WithMessage("Appointment time is required");

            RuleFor(x => x)
                .MustAsync(BeWithinClinicHours)
                .WithMessage("El horario está fuera del horario de trabajo de la clínica.");
        }

        private async Task<bool> AppointmentExists(int appointmentId, CancellationToken cancellationToken)
        {
            var appointment = await unitOfWork.AppointmentsRepository
                .GetByIdAsync(appointmentId, cancellationToken);

            return appointment != null;
        }
        private async Task<bool> NewDateTimeDifferentFromOld(
                    RescheduleAppointmentCommand command,
                    CancellationToken cancellationToken)
        {
            var appointment = await unitOfWork.AppointmentsRepository
                .GetByIdAsync(command.AppointmentId, cancellationToken);

            if (appointment == null)
                return true;

            var existingDateTime = appointment.AppointmentDate;
            var newDateTime = command.AppointmentDate.Date + command.AppointmentTime;

            return existingDateTime != newDateTime;
        }

        private async Task<bool> BeWithinClinicHours(RescheduleAppointmentCommand command, CancellationToken cancellationToken)
        {
            var hours = await operatingHours.GetAsync(cancellationToken);
            return hours.Allows(command.AppointmentDate, command.AppointmentTime);
        }
    }
}
