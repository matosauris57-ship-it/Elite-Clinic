namespace Clinic_System.Application.DTOs
{
    public class NotificationDTO
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
    }
}
