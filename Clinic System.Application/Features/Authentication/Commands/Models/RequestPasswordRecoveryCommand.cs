namespace Clinic_System.Application.Features.Authentication.Commands.Models;

public class RequestPasswordRecoveryCommand : IRequest<Response<string>>
{
    public string Identifier { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
