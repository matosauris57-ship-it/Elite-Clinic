namespace Clinic_System.Core.Entities;

public class PasswordRecoveryRequest : IAuditable
{
    public virtual int Id { get; set; }
    public virtual string Identifier { get; set; } = null!;
    public virtual string NormalizedIdentifier { get; set; } = null!;
    public virtual string? Comment { get; set; }
    public virtual string? UserId { get; set; }
    public virtual string? UserEmail { get; set; }
    public virtual string? UserName { get; set; }
    public virtual string? UserDisplayName { get; set; }
    public virtual string? UserType { get; set; }
    public virtual bool UserMatched { get; set; }
    public virtual PasswordRecoveryStatus Status { get; set; } = PasswordRecoveryStatus.Pending;
    public virtual DateTime RequestedAt { get; set; }
    public virtual DateTime? ResolvedAt { get; set; }
    public virtual string? ResolvedByUserId { get; set; }
    public virtual string? ResolvedByName { get; set; }
    public virtual string? ResolutionNote { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }

    public bool IsPending => Status == PasswordRecoveryStatus.Pending;

    public void MarkResolved(string resolvedByUserId, string resolvedByName, string? note = null)
    {
        Status = PasswordRecoveryStatus.Resolved;
        ResolvedAt = DateTime.Now;
        ResolvedByUserId = resolvedByUserId;
        ResolvedByName = resolvedByName;
        ResolutionNote = TrimNote(note);
        UpdatedAt = DateTime.Now;
    }

    public void Dismiss(string resolvedByUserId, string resolvedByName, string? note = null)
    {
        Status = PasswordRecoveryStatus.Dismissed;
        ResolvedAt = DateTime.Now;
        ResolvedByUserId = resolvedByUserId;
        ResolvedByName = resolvedByName;
        ResolutionNote = TrimNote(note);
        UpdatedAt = DateTime.Now;
    }

    private static string? TrimNote(string? note)
    {
        var trimmed = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        return trimmed is { Length: > 500 } ? trimmed[..500] : trimmed;
    }
}
