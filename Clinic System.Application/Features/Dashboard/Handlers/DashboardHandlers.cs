using Clinic_System.Application.DTOs.Dashboard;
using Clinic_System.Application.Features.Dashboard.Models;

namespace Clinic_System.Application.Features.Dashboard.Handlers;

public class DashboardQueryHandlers :
    AppRequestHandler<GetDashboardLayoutQuery, DashboardLayoutResponseDTO>,
    IRequestHandler<GetClinicDashboardConfigQuery, Response<DashboardClinicConfigDTO>>,
    IRequestHandler<GetPatientDashboardStatsQuery, Response<PatientDashboardStatsDTO>>,
    IRequestHandler<GetRecentClinicalActivityQuery, Response<List<RecentClinicalActivityItemDTO>>>,
    IRequestHandler<GetPeriodontalIncompleteStatsQuery, Response<PeriodontalIncompleteStatsDTO>>
{
    private readonly IDashboardLayoutService _service;

    public DashboardQueryHandlers(ICurrentUserService currentUserService, IDashboardLayoutService service)
        : base(currentUserService)
    {
        _service = service;
    }

    public override async Task<Response<DashboardLayoutResponseDTO>> Handle(GetDashboardLayoutQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("dashboard.view"))
            return Unauthorized<DashboardLayoutResponseDTO>("No autorizado para ver el Dashboard.");

        var data = await _service.GetUserLayoutAsync(cancellationToken);
        return Success(data, "Layout del Dashboard.");
    }

    public async Task<Response<DashboardClinicConfigDTO>> Handle(GetClinicDashboardConfigQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("configuracion.view"))
            return Unauthorized<DashboardClinicConfigDTO>("No autorizado para configurar el Dashboard.");

        var data = await _service.GetClinicConfigAsync(cancellationToken);
        return Success(data, "Configuración de widgets de la clínica.");
    }

    public async Task<Response<PatientDashboardStatsDTO>> Handle(GetPatientDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("pacientes.view"))
            return Unauthorized<PatientDashboardStatsDTO>("No autorizado para consultar pacientes.");

        var data = await _service.GetPatientStatsAsync(cancellationToken);
        return Success(data);
    }

    public async Task<Response<List<RecentClinicalActivityItemDTO>>> Handle(GetRecentClinicalActivityQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("historial.view"))
            return Unauthorized<List<RecentClinicalActivityItemDTO>>("No autorizado para consultar el historial clínico.");

        var since = request.Period switch
        {
            "24h" => DateTime.Now.AddHours(-24),
            "30d" => DateTime.Today.AddDays(-30),
            _ => DateTime.Today.AddDays(-7)
        };
        var data = await _service.GetRecentActivityAsync(since, request.Take, cancellationToken);
        return Success(data);
    }

    public async Task<Response<PeriodontalIncompleteStatsDTO>> Handle(GetPeriodontalIncompleteStatsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("periodontograma.view"))
            return Unauthorized<PeriodontalIncompleteStatsDTO>("No autorizado para consultar periodontogramas.");

        var data = await _service.GetPeriodontalIncompleteAsync(cancellationToken);
        return Success(data);
    }
}

public class DashboardCommandHandlers :
    AppRequestHandler<SaveDashboardLayoutCommand, DashboardLayoutResponseDTO>,
    IRequestHandler<RestoreDashboardLayoutCommand, Response<DashboardLayoutResponseDTO>>,
    IRequestHandler<SaveClinicDashboardConfigCommand, Response<DashboardClinicConfigDTO>>,
    IRequestHandler<RestoreClinicDashboardConfigCommand, Response<DashboardClinicConfigDTO>>
{
    private readonly IDashboardLayoutService _service;

    public DashboardCommandHandlers(ICurrentUserService currentUserService, IDashboardLayoutService service)
        : base(currentUserService)
    {
        _service = service;
    }

    public override async Task<Response<DashboardLayoutResponseDTO>> Handle(SaveDashboardLayoutCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("dashboard.view"))
            return Unauthorized<DashboardLayoutResponseDTO>("No autorizado para guardar el Dashboard.");

        var data = await _service.SaveUserLayoutAsync(request.Layout, cancellationToken);
        return Success(data, "Dashboard guardado.");
    }

    public async Task<Response<DashboardLayoutResponseDTO>> Handle(RestoreDashboardLayoutCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("dashboard.view"))
            return Unauthorized<DashboardLayoutResponseDTO>("No autorizado.");

        var data = await _service.RestoreUserLayoutAsync(cancellationToken);
        return Success(data, "Dashboard restaurado.");
    }

    public async Task<Response<DashboardClinicConfigDTO>> Handle(SaveClinicDashboardConfigCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("configuracion.view"))
            return Unauthorized<DashboardClinicConfigDTO>("No autorizado para configurar el Dashboard.");

        var data = await _service.SaveClinicConfigAsync(request.Layout, cancellationToken);
        return Success(data, "Widgets de la clínica guardados.");
    }

    public async Task<Response<DashboardClinicConfigDTO>> Handle(RestoreClinicDashboardConfigCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasPermission("configuracion.view"))
            return Unauthorized<DashboardClinicConfigDTO>("No autorizado.");

        var data = await _service.RestoreClinicConfigAsync(cancellationToken);
        return Success(data, "Widgets restaurados a los valores predeterminados.");
    }
}
