namespace DentalCare.Admin.Models;

public class PasswordRecoveryRequestItem
{
    public int Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public bool UserMatched { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? UserDisplayName { get; set; }
    public string? UserType { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByName { get; set; }
    public string? ResolutionNote { get; set; }
}

public class PasswordRecoveryListResponse
{
    public int PendingCount { get; set; }
    public List<PasswordRecoveryRequestItem> Items { get; set; } = [];
}
