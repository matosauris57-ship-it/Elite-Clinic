namespace Clinic_System.Core.Entities;

public class EmailCampaign : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public EmailCampaignStatus Status { get; set; } = EmailCampaignStatus.Draft;
    public int BatchSize { get; set; } = 15;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RecipientCount { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }

    public virtual ICollection<EmailCampaignRecipient> Recipients { get; set; } = new List<EmailCampaignRecipient>();
}

public class EmailCampaignRecipient
{
    public int Id { get; set; }
    public int EmailCampaignId { get; set; }
    public int PatientId { get; set; }
    public string Email { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public EmailCampaignRecipientStatus Status { get; set; } = EmailCampaignRecipientStatus.Pending;
    public string? Error { get; set; }
    public DateTime? SentAt { get; set; }

    public virtual EmailCampaign EmailCampaign { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
}
