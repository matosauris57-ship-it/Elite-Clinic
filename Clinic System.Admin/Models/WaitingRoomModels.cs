namespace DentalCare.Admin.Models;

public sealed class WaitingRoomNotification
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string NotificationType { get; set; } = "";
    public int? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
}

public enum WaitingRoomConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Faulted
}
