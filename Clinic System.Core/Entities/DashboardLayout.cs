namespace Clinic_System.Core.Entities;

public class DashboardLayout : IAuditable
{
    public virtual int Id { get; set; }
    public virtual string Scope { get; set; } = DashboardLayoutScopes.User;
    public virtual string? UserId { get; set; }
    public virtual string LayoutJson { get; set; } = "{}";
    public virtual string? UpdatedByUserId { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }
}

public static class DashboardLayoutScopes
{
    public const string Clinic = "Clinic";
    public const string User = "User";
}
