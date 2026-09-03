using Clinic_System.Core.Authorization;

namespace Clinic_System.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Task<List<string>> GetCurrentUserRolesAsync()
        {
            var roles = User == null
                ? []
                : AdminRoleAuthorization.GetRoleValues(User)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            return Task.FromResult(roles);
        }

        public string? UserId =>
            User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public bool IsAdmin => User != null && AdminRoleAuthorization.IsAdminUser(User);

        public bool IsStaff
        {
            get
            {
                if (User == null || !IsAuthenticated)
                    return false;

                if (IsAdmin)
                    return true;

                return User.Claims.Any(c =>
                    string.Equals(c.Type, AdminPermissionCatalog.ClaimType, StringComparison.OrdinalIgnoreCase) &&
                    AdminPermissionCatalog.IsValid(c.Value));
            }
        }

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return false;

            if (IsAdmin)
                return true;

            return User?.Claims.Any(c =>
                string.Equals(c.Type, AdminPermissionCatalog.ClaimType, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public int? DoctorId
        {
            get
            {
                var claimValue = User?.FindFirst("DoctorId")?.Value;
                return int.TryParse(claimValue, out int id) ? id : null;
            }
        }

        public int? PatientId
        {
            get
            {
                var claimValue = User?.FindFirst("PatientId")?.Value;
                return int.TryParse(claimValue, out int id) ? id : null;
            }
        }
    }
}