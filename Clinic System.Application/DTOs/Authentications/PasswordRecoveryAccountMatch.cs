namespace Clinic_System.Application.DTOs.Authentications;

public sealed record PasswordRecoveryAccountMatch(
    string UserId,
    string Email,
    string UserName,
    string DisplayName,
    string UserType);
