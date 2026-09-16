namespace Clinic_System.Application.Features.Authentication.Queries.Models;

public class GetPasswordRecoveryRequestsQuery : IRequest<Response<PasswordRecoveryListDTO>>
{
    public string? Status { get; set; }
}

public class GetPasswordRecoveryPendingCountQuery : IRequest<Response<int>>;

public class ResolvePasswordRecoveryCommand : IRequest<Response<PasswordRecoveryRequestDTO>>
{
    public int Id { get; set; }
    public bool Dismiss { get; set; }
    public string? Note { get; set; }
}
