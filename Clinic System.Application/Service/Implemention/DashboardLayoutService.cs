using Clinic_System.Application.DTOs.Dashboard;
using Clinic_System.Core.Dashboard;

namespace Clinic_System.Application.Service.Implemention;

public class DashboardLayoutService : IDashboardLayoutService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DashboardLayoutService> _logger;

    public DashboardLayoutService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<DashboardLayoutService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<DashboardLayoutResponseDTO> GetUserLayoutAsync(CancellationToken cancellationToken = default)
    {
        var clinic = await GetOrCreateClinicAsync(cancellationToken);
        var clinicDoc = DashboardLayoutEngine.ParseOrDefault(clinic.LayoutJson);
        var userId = RequireUserId();
        var userEntity = await _unitOfWork.DashboardLayoutsRepository.GetByUserIdAsync(userId, cancellationToken);
        var isUser = userEntity != null;
        var userDoc = userEntity == null
            ? clinicDoc
            : DashboardLayoutEngine.ParseOrDefault(userEntity.LayoutJson);

        var merged = DashboardLayoutEngine.FilterByPermissions(
            DashboardLayoutEngine.ApplyClinicAvailability(userDoc, clinicDoc),
            _currentUser.HasPermission);

        return new DashboardLayoutResponseDTO
        {
            Layout = merged,
            ClinicEnabledKeys = DashboardLayoutEngine.VisibleItems(clinicDoc).Select(i => i.WidgetKey).ToList(),
            IsUserLayout = isUser
        };
    }

    public async Task<DashboardLayoutResponseDTO> SaveUserLayoutAsync(DashboardLayoutDocument layout, CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var clinic = await GetOrCreateClinicAsync(cancellationToken);
        var clinicDoc = DashboardLayoutEngine.ParseOrDefault(clinic.LayoutJson);
        var sanitized = DashboardLayoutEngine.FilterByPermissions(
            DashboardLayoutEngine.ApplyClinicAvailability(layout, clinicDoc),
            _currentUser.HasPermission);

        var entity = await _unitOfWork.DashboardLayoutsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (entity == null)
        {
            entity = new DashboardLayout
            {
                Scope = DashboardLayoutScopes.User,
                UserId = userId,
                LayoutJson = DashboardLayoutEngine.Serialize(sanitized),
                UpdatedByUserId = userId
            };
            await _unitOfWork.DashboardLayoutsRepository.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.LayoutJson = DashboardLayoutEngine.Serialize(sanitized);
            entity.UpdatedByUserId = userId;
            _unitOfWork.DashboardLayoutsRepository.Update(entity, cancellationToken);
        }

        await _unitOfWork.SaveAsync(cancellationToken);
        return await GetUserLayoutAsync(cancellationToken);
    }

    public async Task<DashboardLayoutResponseDTO> RestoreUserLayoutAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var existing = await _unitOfWork.DashboardLayoutsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existing != null)
        {
            _unitOfWork.DashboardLayoutsRepository.Delete(existing, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        _logger.LogInformation("Dashboard user layout restored. UserId={UserId}", userId);
        return await GetUserLayoutAsync(cancellationToken);
    }

    public async Task<DashboardClinicConfigDTO> GetClinicConfigAsync(CancellationToken cancellationToken = default)
    {
        EnsureClinicAdmin();
        var clinic = await GetOrCreateClinicAsync(cancellationToken);
        return MapClinic(DashboardLayoutEngine.ParseOrDefault(clinic.LayoutJson));
    }

    public async Task<DashboardClinicConfigDTO> SaveClinicConfigAsync(DashboardLayoutDocument layout, CancellationToken cancellationToken = default)
    {
        EnsureClinicAdmin();
        var clinic = await GetOrCreateClinicAsync(cancellationToken);
        var previous = DashboardLayoutEngine.ParseOrDefault(clinic.LayoutJson);
        var normalized = DashboardLayoutEngine.Normalize(layout);
        clinic.LayoutJson = DashboardLayoutEngine.Serialize(normalized);
        clinic.UpdatedByUserId = _currentUser.UserId;
        _unitOfWork.DashboardLayoutsRepository.Update(clinic, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        var enabledBefore = DashboardLayoutEngine.VisibleItems(previous).Select(i => i.WidgetKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var enabledAfter = DashboardLayoutEngine.VisibleItems(normalized).Select(i => i.WidgetKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activated = enabledAfter.Except(enabledBefore, StringComparer.OrdinalIgnoreCase).ToList();
        var deactivated = enabledBefore.Except(enabledAfter, StringComparer.OrdinalIgnoreCase).ToList();
        if (activated.Count > 0 || deactivated.Count > 0)
        {
            _logger.LogInformation(
                "Dashboard clinic widgets updated by {UserId}. Activated={Activated}; Deactivated={Deactivated}",
                _currentUser.UserId,
                string.Join(",", activated),
                string.Join(",", deactivated));
        }

        return MapClinic(normalized);
    }

    public async Task<DashboardClinicConfigDTO> RestoreClinicConfigAsync(CancellationToken cancellationToken = default)
    {
        EnsureClinicAdmin();
        var clinic = await GetOrCreateClinicAsync(cancellationToken);
        var defaults = DashboardWidgetCatalog.CreateDefaultLayout();
        clinic.LayoutJson = DashboardLayoutEngine.Serialize(defaults);
        clinic.UpdatedByUserId = _currentUser.UserId;
        _unitOfWork.DashboardLayoutsRepository.Update(clinic, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        _logger.LogInformation("Dashboard clinic layout restored by {UserId}", _currentUser.UserId);
        return MapClinic(defaults);
    }

    public async Task<PatientDashboardStatsDTO> GetPatientStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission("pacientes.view"))
            throw new UnauthorizedException("No autorizado para consultar pacientes.");

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);

        var total = await _unitOfWork.PatientsRepository.CountAsync(null, cancellationToken);
        var newThisMonth = await _unitOfWork.PatientsRepository.CountAsync(p => p.CreatedAt >= monthStart, cancellationToken);
        var newLastMonth = await _unitOfWork.PatientsRepository.CountAsync(
            p => p.CreatedAt >= lastMonthStart && p.CreatedAt < monthStart, cancellationToken);

        return new PatientDashboardStatsDTO
        {
            TotalPatients = total,
            NewThisMonth = newThisMonth,
            NewLastMonth = newLastMonth
        };
    }

    public async Task<List<RecentClinicalActivityItemDTO>> GetRecentActivityAsync(DateTime since, int take, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission("historial.view"))
            throw new UnauthorizedException("No autorizado para consultar el historial clínico.");

        take = Math.Clamp(take, 5, 20);
        var events = await _unitOfWork.DentalClinicalEventsRepository.GetRecentAsync(since, take, cancellationToken);
        return events.Select(e => new RecentClinicalActivityItemDTO
        {
            Id = e.Id,
            PatientId = e.PatientId,
            PatientName = e.Patient?.FullName ?? "Paciente",
            Title = ToothChartEventText.LocalizeTitle(e.Title),
            Description = e.Description,
            Type = e.Type.ToString(),
            RecordedAt = e.RecordedAt
        }).ToList();
    }

    public async Task<PeriodontalIncompleteStatsDTO> GetPeriodontalIncompleteAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission("periodontograma.view"))
            throw new UnauthorizedException("No autorizado para consultar periodontogramas.");

        var count = await _unitOfWork.PeriodontalExamsRepository.CountAsync(e => e.RecordedSiteCount == 0, cancellationToken);
        return new PeriodontalIncompleteStatsDTO { IncompleteExams = count };
    }

    private async Task<DashboardLayout> GetOrCreateClinicAsync(CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.DashboardLayoutsRepository.GetClinicDefaultAsync(cancellationToken);
        if (clinic != null)
            return clinic;

        clinic = new DashboardLayout
        {
            Scope = DashboardLayoutScopes.Clinic,
            UserId = null,
            LayoutJson = DashboardLayoutEngine.Serialize(DashboardWidgetCatalog.CreateDefaultLayout()),
            UpdatedByUserId = _currentUser.UserId
        };
        await _unitOfWork.DashboardLayoutsRepository.AddAsync(clinic, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        return clinic;
    }

    private DashboardClinicConfigDTO MapClinic(DashboardLayoutDocument layout)
    {
        var normalized = DashboardLayoutEngine.Normalize(layout);
        return new DashboardClinicConfigDTO
        {
            Layout = normalized,
            Widgets = DashboardWidgetCatalog.All.Select(d =>
            {
                var item = normalized.Items.First(i => i.WidgetKey == d.Key);
                return new DashboardCatalogItemDTO
                {
                    Key = d.Key,
                    Title = d.Title,
                    Description = d.Description,
                    Permission = d.Permission,
                    Required = d.Required,
                    Enabled = item.Visible,
                    AllowedForUser = _currentUser.HasPermission(d.Permission)
                };
            }).ToList()
        };
    }

    private string RequireUserId()
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
            throw new UnauthorizedException("Sesión no válida.");
        return _currentUser.UserId;
    }

    private void EnsureClinicAdmin()
    {
        if (!_currentUser.HasPermission("configuracion.view"))
            throw new UnauthorizedException("No autorizado para configurar el Dashboard de la clínica.");
    }
}
