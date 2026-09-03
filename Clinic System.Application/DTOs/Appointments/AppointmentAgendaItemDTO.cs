namespace Clinic_System.Application.DTOs.Appointments
{
    public class AppointmentAgendaItemDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string PatientFullName { get; set; } = null!;
        public string PatientPhone { get; set; } = null!;
        public string? PatientEmail { get; set; }
        public string DoctorName { get; set; } = null!;
        public string DoctorPhone { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public string AppointmentDate { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
