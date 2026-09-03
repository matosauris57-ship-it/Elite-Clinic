using Clinic_System.Application.Common;
using Clinic_System.Core.Validation;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Clinic_System.Infrastructure.Services.Email
{
    public class PatientNotificationDispatchService : IPatientNotificationDispatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IEmailSettingsProvider _emailSettings;
        private readonly IPatientNotificationSettingsService _notifications;
        private readonly ILogger<PatientNotificationDispatchService> _logger;

        public PatientNotificationDispatchService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IEmailSettingsProvider emailSettings,
            IPatientNotificationSettingsService notifications,
            ILogger<PatientNotificationDispatchService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _emailSettings = emailSettings;
            _notifications = notifications;
            _logger = logger;
        }

        [DisableConcurrentExecution(600)]
        public async Task DispatchDueAsync()
        {
            if (!_emailSettings.IsConfigured())
            {
                _logger.LogInformation("Omitiendo notificaciones automáticas: SMTP no configurado.");
                return;
            }

            var settings = _notifications.Get();
            var now = DateTime.Now;
            var clinic = string.IsNullOrWhiteSpace(_emailSettings.Get().SenderName)
                ? "la clínica"
                : _emailSettings.Get().SenderName;

            if (settings.DayBeforeEnabled || settings.SameDayEnabled)
                await SendAppointmentRemindersAsync(settings, clinic, now);

            if (settings.BirthdayEnabled)
                await SendBirthdayEmailsAsync(settings, clinic, now);
        }

        private async Task SendAppointmentRemindersAsync(PatientNotificationSettings settings, string clinic, DateTime now)
        {
            var start = now.Date;
            var end = now.Date.AddDays(2);
            var appointments = await _unitOfWork.AppointmentsRepository.GetForAutomaticRemindersAsync(start, end);

            foreach (var appointment in appointments)
            {
                var patient = appointment.Patient;
                var doctor = appointment.Doctor;
                if (patient == null || patient.EmailInvalid || !ContactEmail.TryValidate(patient.Email, out var to, out _) || string.IsNullOrWhiteSpace(to))
                    continue;

                var changed = false;

                if (appointment.DayBeforeReminderSentAt == null
                    && settings.ShouldSendDayBefore(now, appointment.AppointmentDate))
                {
                    if (await TrySendAsync(to, settings.ReminderSubject, settings.ReminderBody, clinic, patient.FullName, doctor?.FullName, appointment.AppointmentDate, null))
                    {
                        appointment.DayBeforeReminderSentAt = now;
                        changed = true;
                    }
                }

                if (appointment.SameDayReminderSentAt == null
                    && settings.ShouldSendSameDay(now, appointment.AppointmentDate))
                {
                    if (await TrySendAsync(to, settings.ReminderSubject, settings.ReminderBody, clinic, patient.FullName, doctor?.FullName, appointment.AppointmentDate, null))
                    {
                        appointment.SameDayReminderSentAt = now;
                        changed = true;
                    }
                }

                if (changed)
                    _unitOfWork.AppointmentsRepository.Update(appointment);
            }

            await _unitOfWork.SaveAsync();
        }

        private async Task SendBirthdayEmailsAsync(PatientNotificationSettings settings, string clinic, DateTime now)
        {
            var patients = await _unitOfWork.PatientsRepository.GetForBirthdayEmailsAsync(now.Year);

            foreach (var patient in patients)
            {
                if (!settings.ShouldSendBirthday(now, patient.DateOfBirth, patient.BirthdayEmailLastSentYear))
                    continue;
                if (!ContactEmail.TryValidate(patient.Email, out var to, out _) || string.IsNullOrWhiteSpace(to))
                    continue;

                var age = now.Year - patient.DateOfBirth.Year;
                if (patient.DateOfBirth.Date > now.Date.AddYears(-age))
                    age--;

                if (!await TrySendAsync(to, settings.BirthdaySubject, settings.BirthdayBody, clinic, patient.FullName, null, null, age))
                    continue;

                patient.BirthdayEmailLastSentYear = now.Year;
                _unitOfWork.PatientsRepository.Update(patient);
            }

            await _unitOfWork.SaveAsync();
        }

        private async Task<bool> TrySendAsync(
            string to,
            string subjectTemplate,
            string bodyTemplate,
            string clinic,
            string patientName,
            string? doctorName,
            DateTime? appointment,
            int? age)
        {
            try
            {
                var subject = PatientNotificationSettings.Apply(subjectTemplate, clinic, patientName, doctorName, appointment, age);
                var body = PatientNotificationSettings.Apply(bodyTemplate, clinic, patientName, doctorName, appointment, age);
                await _emailService.SendEmailAsync(to, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar notificación automática a {Email}", to);
                return false;
            }
        }
    }
}
