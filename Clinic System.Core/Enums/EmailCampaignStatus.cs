namespace Clinic_System.Core.Enums;

public enum EmailCampaignStatus
{
    Draft = 1,
    Running = 2,
    Paused = 3,
    Completed = 4,
    Cancelled = 5
}

public enum EmailCampaignRecipientStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Skipped = 4
}
