namespace Clinic_System.Application.Features.Authentication.Commands.Validators;

public class RequestPasswordRecoveryCommandValidator : AbstractValidator<RequestPasswordRecoveryCommand>
{
    public RequestPasswordRecoveryCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Indique su correo o usuario.")
            .MaximumLength(256).WithMessage("El correo o usuario es demasiado largo.");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("El comentario es demasiado largo.");
    }
}
