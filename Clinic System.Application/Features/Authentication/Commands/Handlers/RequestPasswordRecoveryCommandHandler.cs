namespace Clinic_System.Application.Features.Authentication.Commands.Handlers;

public class RequestPasswordRecoveryCommandHandler : ResponseHandler, IRequestHandler<RequestPasswordRecoveryCommand, Response<string>>
{
    private readonly IPasswordRecoveryService _service;
    private readonly ILogger<RequestPasswordRecoveryCommandHandler> _logger;

    public RequestPasswordRecoveryCommandHandler(
        IPasswordRecoveryService service,
        ILogger<RequestPasswordRecoveryCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Response<string>> Handle(RequestPasswordRecoveryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _service.SubmitAsync(request.Identifier, request.Comment, cancellationToken);
            return Success(
                "Si la cuenta existe, el personal de la clínica verá la solicitud y podrá ayudarle a recuperar el acceso.",
                "Solicitud registrada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar recuperación de contraseña para {Identifier}", request.Identifier);
            return BadRequest<string>("No se pudo registrar la solicitud. Intente de nuevo en unos minutos.");
        }
    }
}
