namespace Clinic_System.Application.Tests.Common
{
    public class ClinicOperatingHoursTests
    {
        [Fact]
        public void GenerateSlots_RespectsOpenCloseAndDuration()
        {
            var hours = new ClinicOperatingHours
            {
                OpenTime = new TimeSpan(8, 0, 0),
                CloseTime = new TimeSpan(10, 0, 0),
                SlotDurationMinutes = 30,
                WorkingDays = [1, 2, 3, 4, 5]
            };

            var slots = hours.GenerateSlots();

            slots.Should().Equal(
                new TimeSpan(8, 0, 0),
                new TimeSpan(8, 30, 0),
                new TimeSpan(9, 0, 0),
                new TimeSpan(9, 30, 0));
        }

        [Fact]
        public void Allows_RejectsClosedDayAndOffSlot()
        {
            var hours = new ClinicOperatingHours
            {
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(12, 0, 0),
                SlotDurationMinutes = 60,
                WorkingDays = [(int)DayOfWeek.Monday]
            };

            var monday = new DateTime(2026, 9, 7);
            var sunday = new DateTime(2026, 9, 6);

            hours.Allows(monday, new TimeSpan(9, 0, 0)).Should().BeTrue();
            hours.Allows(monday, new TimeSpan(10, 30, 0)).Should().BeFalse();
            hours.Allows(sunday, new TimeSpan(9, 0, 0)).Should().BeFalse();
        }
    }
}
