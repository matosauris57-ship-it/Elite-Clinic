using Clinic_System.Application.DTOs.Authentications;
using Clinic_System.Core.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<string> CreateUserAsync(string userName, string email, string password, string role, CancellationToken cancellationToken = default)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(userName))
                throw new DomainException("User name cannot be empty");

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email cannot be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new DomainException("Password cannot be empty");

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new DomainException($"Failed to create user: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new DomainException($"Failed to assign role: {roleErrors}");
                }
            }

            return user.Id;
        }

        public async Task<string> CreateUserForGoogleAsync(string userName, string email, string role, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new DomainException("User name cannot be empty");

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email cannot be empty");

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true 
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new DomainException($"Failed to create Google user: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new DomainException($"Failed to assign role: {roleErrors}");
                }
            }

            return user.Id;
        }

        public async Task<bool> SoftDeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return false;

            // Soft Delete: Set IsDeleted and DeletedAt
            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> RestoreUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsDeleted)
                return false;

            user.IsDeleted = false;
            user.DeletedAt = null;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> HardDeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<string?> GetUserEmailAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email;
        }
        public async Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            return user?.UserName;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetUserDisplayNamesAsync(
            IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var userId in userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct())
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    continue;

                var display = !string.IsNullOrWhiteSpace(user.UserName)
                    ? user.UserName
                    : user.Email;
                if (!string.IsNullOrWhiteSpace(display))
                    names[userId] = display;
            }

            return names;
        }

        public async Task<(bool IsAuthenticated, bool IsEmailConfirmed, string Id, string UserName, string Email, List<string> Roles)> LoginAsync(string userNameOrEmail, string password)
        {
            var user = userNameOrEmail.Contains("@")
                ? await _userManager.FindByEmailAsync(userNameOrEmail)
                : await _userManager.FindByNameAsync(userNameOrEmail);

            if (user == null || user.IsDeleted)
            {
                return (false, false, string.Empty, string.Empty, string.Empty, new List<string>());
            }

            // CheckPasswordSignInAsync falla con RequireConfirmedEmail y lo reporta como credenciales inválidas.
            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return (false, false, string.Empty, string.Empty, string.Empty, new List<string>());
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isClinicAccount = roles.Any(r =>
                !string.Equals(r, AdminPermissionCatalog.SystemRoles.Patient, StringComparison.OrdinalIgnoreCase));

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                if (isClinicAccount)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    return (true, false, user.Id, user.UserName!, user.Email!, new List<string>());
                }
            }

            return (true, true, user.Id, user.UserName!, user.Email!, roles.ToList());
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new DomainException("User not found");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }

        public string EncodeToken(string token)
        {
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }

        public string DecodeToken(string encodedToken)
        {
            var bytes = WebEncoders.Base64UrlDecode(encodedToken);
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return result.Succeeded;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new DomainException("User not found");
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

        public async Task<(bool Succeeded, string Error)> ResetPasswordAsync(string email, string decodedToken, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "User not found");

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (result.Succeeded)
            {
                return (true, null); 
            }

            // �� ��� ��� �� ������� �� ����� �����
            // �����: "Password requires non-alphanumeric, Password requires digit"
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            return (false, errors);
        }

        public async Task<(string UserId, string UserName, string Role, string Token, string Error)> GenerateTokenForResendEmailConfirmationAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return (null, null, null, null, "No user found with this email.");

            if (user.EmailConfirmed)
                return (null, null, null, null, "Email is already confirmed.");

            // ����� ���� ����
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var userRole = roles.FirstOrDefault();

            return (user.Id, user.UserName, userRole, token, null);
        }

        public async Task<(string Email, string UserName)> GetUserEmailAndUserNameAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (string.Empty, string.Empty);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return (string.Empty, string.Empty);

            return (user.Email ?? string.Empty, user.UserName ?? string.Empty);
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, string newEmail, string newUserName, string currentPassword, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("User not found");

            if (!isAdmin)
            {
                var checkPass = await _userManager.CheckPasswordAsync(user, currentPassword);
                if (!checkPass) throw new Exception("Invalid current password.");
            }

            if (!string.IsNullOrEmpty(newEmail) && user.Email != newEmail)
            {
                var result = await _userManager.SetEmailAsync(user, newEmail);
                if (!result.Succeeded) throw new Exception(result.Errors.First().Description);
                user.EmailConfirmed = false;
            }

            if (!string.IsNullOrEmpty(newUserName) && user.UserName != newUserName)
            {
                var result = await _userManager.SetUserNameAsync(user, newUserName);
                if (!result.Succeeded) throw new Exception(result.Errors.First().Description);
            }

            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<(bool Success, string? Error)> UpdateManagedUserAccountAsync(
            string userId,
            string? userName,
            string? email,
            string? newPassword,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "No se encontró la cuenta de acceso del médico.");

            if (!string.IsNullOrWhiteSpace(email) &&
                !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(user, email.Trim());
                if (!setEmail.Succeeded)
                    return (false, setEmail.Errors.FirstOrDefault()?.Description ?? "No se pudo actualizar el correo.");

                user.EmailConfirmed = true;
            }

            if (!string.IsNullOrWhiteSpace(userName) &&
                !string.Equals(user.UserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                var setName = await _userManager.SetUserNameAsync(user, userName.Trim());
                if (!setName.Succeeded)
                    return (false, setName.Errors.FirstOrDefault()?.Description ?? "No se pudo actualizar el usuario.");
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                    return (false, "No se pudo actualizar la contraseña.");

                var addResult = await _userManager.AddPasswordAsync(user, newPassword);
                if (!addResult.Succeeded)
                    return (false, addResult.Errors.FirstOrDefault()?.Description ?? "No se pudo actualizar la contraseña.");
            }

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return (false, update.Errors.FirstOrDefault()?.Description ?? "No se pudo guardar la cuenta de acceso.");

            return (true, null);
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new DomainException("Password cannot be empty");

            IdentityResult result;

            if (isAdmin)
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded) throw new Exception("Failed to reset existing password.");

                result = await _userManager.AddPasswordAsync(user, newPassword);
            }
            else
            {
                result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            }

            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault()?.Description ?? "Failed to update password.";
                if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                    throw new UnauthorizedAccessException();

                throw new Exception(error);
            }

            return true;
        }

        public async Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var userWithEmail = await _userManager.FindByEmailAsync(email);
            if (userWithEmail == null) return true;
            if (excludeUserId != null && userWithEmail.Id == excludeUserId) return true;
            return false;
        }

        public async Task<bool> IsUserNameUniqueAsync(string userName, string? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return true;
            if (excludeUserId != null && user.Id == excludeUserId) return true;
            return false;
        }

        public async Task<(bool Exists, string Id, string UserName, List<string> Roles)> GetUserDetailsByEmailForGoogleAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // �� ������ �� �����
            if (user == null)
                return (false, string.Empty, string.Empty, new List<string>());

            // �� ����ϡ ���� ��������� ������
            var roles = await _userManager.GetRolesAsync(user);

            return (true, user.Id, user.UserName ?? string.Empty, roles.ToList());
        }

        public async Task<PasswordRecoveryAccountMatch?> FindAccountForRecoveryAsync(string identifier, CancellationToken cancellationToken = default)
        {
            var value = identifier.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            ApplicationUser? user = null;
            if (value.Contains('@'))
                user = await _userManager.FindByEmailAsync(value);

            user ??= await _userManager.FindByNameAsync(value);
            if (user == null && !value.Contains('@'))
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == value, cancellationToken);

            if (user == null)
                return null;

            var loaded = await _userManager.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken) ?? user;

            var roles = await _userManager.GetRolesAsync(loaded);
            var display = loaded.Doctor?.FullName
                ?? loaded.Patient?.FullName
                ?? loaded.UserName
                ?? loaded.Email
                ?? value;

            var userType = loaded.Doctor != null || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor)
                ? "Médico"
                : loaded.Patient != null || roles.Contains(AdminPermissionCatalog.SystemRoles.Patient)
                    ? "Paciente"
                    : "Staff";

            return new PasswordRecoveryAccountMatch(
                loaded.Id,
                loaded.Email ?? string.Empty,
                loaded.UserName ?? string.Empty,
                display,
                userType);
        }
    }
}