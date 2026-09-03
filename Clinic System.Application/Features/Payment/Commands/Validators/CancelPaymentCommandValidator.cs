namespace Clinic_System.Application.Features.Payment.Commands.Validators
{
    public class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
    {
        public CancelPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.Reason).MaximumLength(500);
        }
    }
}
