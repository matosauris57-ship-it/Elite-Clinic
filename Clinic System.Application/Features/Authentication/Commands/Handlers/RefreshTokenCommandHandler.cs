using Clinic_System.Core.Entities;
using Clinic_System.Core.Interfaces.UnitOfWork;
using System.Data;

namespace Clinic_System.Application.Features.Authentication.Commands.Handlers
{
    public class RefreshTokenCommandHandler : ResponseHandler, IRequestHandler<RefreshTokenCommand, Response<JwtAuthResult>>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IPermissionResolver _permissionResolver;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IAuthenticationService authenticationService,
            IPermissionResolver permissionResolver,
            IUnitOfWork unitOfWork,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _authenticationService = authenticationService;
            _permissionResolver = permissionResolver;
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<JwtAuthResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start handling RefreshTokenCommand for AccessToken: {AccessToken}", request.AccessToken);

            try
            {
                var principal = _authenticationService.GetPrincipalFromExpiredToken(request.AccessToken);

                if (principal == null)
                    return BadRequest<JwtAuthResult>("Invalid Token");

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                var customClaims = new List<Claim>();

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    var permissions = await _permissionResolver.ResolvePermissionsAsync(userId, cancellationToken);
                    foreach (var permission in permissions)
                        customClaims.Add(new Claim(AdminPermissionCatalog.ClaimType, permission));
                }

                var roles = principal.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (roles.Contains("Doctor"))
                {
                    var doctor = await unitOfWork.DoctorsRepository.GetDoctorByUserIdAsync(userId);
                    if (doctor != null)
                    {
                        customClaims.Add(new Claim("DoctorId", doctor.Id.ToString()));
                    }
                }
                else if (roles.Contains("Patient"))
                {
                    var patient = await unitOfWork.PatientsRepository.GetPatientByUserIdAsync(userId);
                    if (patient != null)
                    {
                        customClaims.Add(new Claim("PatientId", patient.Id.ToString()));
                    }
                }

                var (accessToken, refreshToken, expiresAt) = await _authenticationService.RefreshTokenAsync(request.AccessToken, request.RefreshToken , customClaims);

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Failed to refresh token. Invalid or expired refresh token provided.");
                    return Unauthorized<JwtAuthResult>("Invalid or Expired Refresh Token");
                }

                var response = new JwtAuthResult
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Permissions = customClaims
                        .Where(c => c.Type == AdminPermissionCatalog.ClaimType)
                        .Select(c => c.Value)
                        .ToList()
                };

                _logger.LogInformation("Token refreshed successfully for user.");

                return Success(response, "Token Refreshed Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while refreshing the token.");

                return Unauthorized<JwtAuthResult>("An error occurred while processing your request.");
            }
        }
    }
}
