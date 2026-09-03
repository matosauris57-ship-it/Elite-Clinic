namespace Clinic_System.Application.Common
{
    public class ClinicOperatingHours
    {
        public TimeSpan OpenTime { get; set; } = new(12, 0, 0);
        public TimeSpan CloseTime { get; set; } = new(22, 0, 0);
        public int SlotDurationMinutes { get; set; } = 15;
        public List<int> WorkingDays { get; set; } = [0, 1, 2, 3, 4, 5, 6];

        public ClinicOperatingHours Normalize()
        {
            var duration = SlotDurationMinutes is < 5 or > 120 ? 15 : SlotDurationMinutes;
            var open = OpenTime;
            var close = CloseTime;
            if (close <= open)
            {
                open = new TimeSpan(8, 0, 0);
                close = new TimeSpan(18, 0, 0);
            }

            var days = (WorkingDays ?? [])
                .Where(d => d is >= 0 and <= 6)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
            if (days.Count == 0)
                days = [0, 1, 2, 3, 4, 5, 6];

            return new ClinicOperatingHours
            {
                OpenTime = open,
                CloseTime = close,
                SlotDurationMinutes = duration,
                WorkingDays = days
            };
        }

        public bool IsOpenOn(DateTime date) =>
            WorkingDays.Contains((int)date.DayOfWeek);

        public List<TimeSpan> GenerateSlots()
        {
            var hours = Normalize();
            var slots = new List<TimeSpan>();
            var current = hours.OpenTime;
            var step = TimeSpan.FromMinutes(hours.SlotDurationMinutes);

            while (current.Add(step) <= hours.CloseTime)
            {
                slots.Add(current);
                current = current.Add(step);
            }

            return slots;
        }

        public bool Allows(DateTime date, TimeSpan time) =>
            IsOpenOn(date) && GenerateSlots().Contains(time);
    }
}
