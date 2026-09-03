namespace DentalCare.Admin.Models;

public class EmailCampaignAudience
{
    public int EligibleCount { get; set; }
    public int WithEmailCount { get; set; }
    public int OptedOutCount { get; set; }
    public int InvalidCount { get; set; }
    public int BatchSize { get; set; } = 15;
    public int EstimatedBatches { get; set; }
    public bool SmtpConfigured { get; set; }
}

public class EmailCampaignForm
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class EmailCampaignListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RecipientCount { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public int PendingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class EmailCampaignRecipientItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime? SentAt { get; set; }
}

public class EmailCampaignDetail : EmailCampaignListItem
{
    public string Body { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 15;
    public List<EmailCampaignRecipientItem> RecentRecipients { get; set; } = [];
}
