namespace Clinic_System.Application.Common
{
    public class PatientNotificationSettings
    {
        public const string ReminderBodyDefault =
            "Hola {nombre}, le recordamos su cita en {clinica} el {fecha} a las {hora} con {doctor}. Si no puede asistir, avísenos. Gracias.";

        public const string BirthdayBodyDefault =
            "Hola {nombre}, en {clinica} le deseamos un muy feliz cumpleaños. ¡Que cumpla {edad} años llenos de salud y sonrisas!";

        public bool DayBeforeEnabled { get; set; } = true;
        public bool SameDayEnabled { get; set; }
        public TimeSpan DayBeforeSendTime { get; set; } = new(9, 0, 0);
        public TimeSpan SameDaySendTime { get; set; } = new(8, 0, 0);

        public bool BirthdayEnabled { get; set; }
        public TimeSpan BirthdaySendTime { get; set; } = new(8, 0, 0);

        public string ReminderSubject { get; set; } = "Recordatorio de cita - {clinica}";
        public string ReminderBody { get; set; } = ReminderBodyDefault;
        public string BirthdaySubject { get; set; } = "Feliz cumpleaños - {clinica}";
        public string BirthdayBody { get; set; } = BirthdayBodyDefault;

        public PatientNotificationSettings Normalize()
        {
            return new PatientNotificationSettings
            {
                DayBeforeEnabled = DayBeforeEnabled,
                SameDayEnabled = SameDayEnabled,
                DayBeforeSendTime = ClampTime(DayBeforeSendTime, new TimeSpan(9, 0, 0)),
                SameDaySendTime = ClampTime(SameDaySendTime, new TimeSpan(8, 0, 0)),
                BirthdayEnabled = BirthdayEnabled,
                BirthdaySendTime = ClampTime(BirthdaySendTime, new TimeSpan(8, 0, 0)),
                ReminderSubject = First(ReminderSubject, "Recordatorio de cita - {clinica}"),
                ReminderBody = First(ReminderBody, ReminderBodyDefault),
                BirthdaySubject = First(BirthdaySubject, "Feliz cumpleaños - {clinica}"),
                BirthdayBody = First(BirthdayBody, BirthdayBodyDefault)
            };
        }

        public bool ShouldSendDayBefore(DateTime now, DateTime appointment) =>
            DayBeforeEnabled
            && appointment.Date == now.Date.AddDays(1)
            && now.TimeOfDay >= DayBeforeSendTime;

        public bool ShouldSendSameDay(DateTime now, DateTime appointment) =>
            SameDayEnabled
            && appointment.Date == now.Date
            && appointment > now
            && now.TimeOfDay >= SameDaySendTime;

        public bool ShouldSendBirthday(DateTime now, DateTime dateOfBirth, int? lastSentYear) =>
            BirthdayEnabled
            && lastSentYear != now.Year
            && now.TimeOfDay >= BirthdaySendTime
            && IsBirthdayOn(now.Date, dateOfBirth);

        public static bool IsBirthdayOn(DateTime day, DateTime dateOfBirth)
        {
            if (dateOfBirth.Month == day.Month && dateOfBirth.Day == day.Day)
                return true;

            return dateOfBirth is { Month: 2, Day: 29 }
                && day is { Month: 2, Day: 28 }
                && !DateTime.IsLeapYear(day.Year);
        }

        public static string Apply(string template, string clinicName, string patientName, string? doctorName = null, DateTime? appointment = null, int? age = null)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("es-ES");
            var fecha = appointment?.ToString("d 'de' MMMM yyyy", culture) ?? "—";
            var hora = appointment?.ToString("HH:mm", culture) ?? "—";

            return template
                .Replace("{nombre}", patientName, StringComparison.OrdinalIgnoreCase)
                .Replace("{clinica}", clinicName, StringComparison.OrdinalIgnoreCase)
                .Replace("{fecha}", fecha, StringComparison.OrdinalIgnoreCase)
                .Replace("{hora}", hora, StringComparison.OrdinalIgnoreCase)
                .Replace("{doctor}", doctorName ?? "—", StringComparison.OrdinalIgnoreCase)
                .Replace("{edad}", age?.ToString() ?? "—", StringComparison.OrdinalIgnoreCase);
        }

        private static TimeSpan ClampTime(TimeSpan value, TimeSpan fallback)
        {
            if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1))
                return fallback;
            return new TimeSpan(value.Hours, value.Minutes, 0);
        }

        private static string First(string? value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
