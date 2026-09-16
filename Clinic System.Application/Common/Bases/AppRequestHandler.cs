using Clinic_System.Core.Authorization;

namespace Clinic_System.Application.Common.Bases
{
    public abstract class AppRequestHandler<TRequest, TResponse> : ResponseHandler, IRequestHandler<TRequest, Response<TResponse>>
        where TRequest : IRequest<Response<TResponse>>
    {
        protected readonly ICurrentUserService _currentUserService;

        public AppRequestHandler(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        protected string CurrentUserId => _currentUserService.UserId;

        protected int? CurrentDoctorId => _currentUserService.DoctorId;
        protected int? CurrentPatientId => _currentUserService.PatientId;
        protected bool IsAdmin => _currentUserService.IsAdmin;
        protected bool RestrictsToOwnDoctorData => _currentUserService.RestrictsToOwnDoctorData;
        protected bool CanViewAllClinicData => _currentUserService.CanViewAllClinicData;

        protected bool HasDoctorDirectoryPermission(string action) =>
            IsAdmin ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("medicos", action));

        protected Task<Response<TResponse>?> ValidateDoctorDirectoryAccess(int targetDoctorId, bool requireEdit)
        {
            var action = requireEdit ? AdminPermissionCatalog.Actions.Edit : AdminPermissionCatalog.Actions.View;
            if (HasDoctorDirectoryPermission(action))
                return Task.FromResult<Response<TResponse>?>(null);

            return ValidateDoctorAccess(targetDoctorId);
        }

        protected Task<Response<TResponse>?> ValidateDoctorAccess(int targetDoctorId)
        {
            if (IsAdmin)
                return Task.FromResult<Response<TResponse>?>(null);

            if (CurrentDoctorId == targetDoctorId)
                return Task.FromResult<Response<TResponse>?>(null);

            return Task.FromResult<Response<TResponse>?>(
                Unauthorized<TResponse>("Acceso denegado. Solo puede consultar sus propios datos."));
        }

        protected Task<Response<TResponse>?> ValidatePatientAccess(int targetPatientId)
        {
            if (IsAdmin)
                return Task.FromResult<Response<TResponse>?>(null);

            if (CurrentPatientId == targetPatientId)
                return Task.FromResult<Response<TResponse>?>(null);

            return Task.FromResult<Response<TResponse>?>(
                Unauthorized<TResponse>("Acceso denegado. Solo puede consultar sus propios datos."));
        }

        protected int? ApplyDoctorScope(int? requestedDoctorId) =>
            RestrictsToOwnDoctorData ? CurrentDoctorId : requestedDoctorId;

        protected Task<Response<TResponse>?> ValidateScopedDoctorSelection(int doctorId)
        {
            if (!RestrictsToOwnDoctorData)
                return Task.FromResult<Response<TResponse>?>(null);

            if (CurrentDoctorId == doctorId)
                return Task.FromResult<Response<TResponse>?>(null);

            return Task.FromResult<Response<TResponse>?>(
                Unauthorized<TResponse>("Solo puede agendar o consultar citas asignadas a usted."));
        }

        protected Task<(int TargetId, Response<TResponse>? Error)> GetAuthorizedDoctorId(int? requestDoctorId)
        {
            if (RestrictsToOwnDoctorData)
            {
                if (requestDoctorId is > 0 && requestDoctorId != CurrentDoctorId)
                    return Task.FromResult<(int, Response<TResponse>?)>(
                        (0, Unauthorized<TResponse>("Acceso denegado. Solo puede consultar sus propios datos.")));

                return Task.FromResult<(int, Response<TResponse>?)>((CurrentDoctorId!.Value, null));
            }

            if (IsAdmin || CanViewAllClinicData)
            {
                if (requestDoctorId is null or 0)
                {
                    if (CurrentDoctorId.HasValue)
                        return Task.FromResult<(int, Response<TResponse>?)>((CurrentDoctorId.Value, null));

                    return Task.FromResult<(int, Response<TResponse>?)>(
                        (0, BadRequest<TResponse>("Debe indicar el médico.")));
                }

                return Task.FromResult<(int, Response<TResponse>?)>((requestDoctorId.Value, null));
            }

            if (CurrentDoctorId.HasValue)
                return Task.FromResult<(int, Response<TResponse>?)>((CurrentDoctorId.Value, null));

            return Task.FromResult<(int, Response<TResponse>?)>(
                (0, Unauthorized<TResponse>("Acceso denegado. No tiene permiso para consultar datos de médicos.")));
        }

        protected Task<(int TargetId, Response<TResponse>? Error)> GetAuthorizedPatientId(
            int? requestPatientId,
            string? staffPermission = null)
        {
            var canActForAnotherPatient =
                IsAdmin ||
                (!string.IsNullOrWhiteSpace(staffPermission) &&
                 _currentUserService.HasPermission(staffPermission));

            if (canActForAnotherPatient)
            {
                if (requestPatientId is null or 0)
                    return Task.FromResult<(int, Response<TResponse>?)>(
                        (0, BadRequest<TResponse>("Debe indicar el paciente.")));

                return Task.FromResult<(int, Response<TResponse>?)>((requestPatientId.Value, null));
            }

            if (CurrentPatientId.HasValue)
            {
                if (requestPatientId is > 0 && requestPatientId != CurrentPatientId)
                    return Task.FromResult<(int, Response<TResponse>?)>(
                        (0, Unauthorized<TResponse>("Acceso denegado. Solo puede acceder a sus propios datos.")));

                return Task.FromResult<(int, Response<TResponse>?)>((CurrentPatientId.Value, null));
            }

            return Task.FromResult<(int, Response<TResponse>?)>(
                (0, Unauthorized<TResponse>("Acceso denegado. No tiene permiso para agendar o consultar este paciente.")));
        }

        public abstract Task<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
