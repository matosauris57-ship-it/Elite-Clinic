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

        protected Task<(int TargetId, Response<TResponse>? Error)> GetAuthorizedDoctorId(int? requestDoctorId)
        {
            if (IsAdmin)
            {
                if (requestDoctorId is null or 0)
                    return Task.FromResult<(int, Response<TResponse>?)>(
                        (0, BadRequest<TResponse>("Debe indicar el médico.")));

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
