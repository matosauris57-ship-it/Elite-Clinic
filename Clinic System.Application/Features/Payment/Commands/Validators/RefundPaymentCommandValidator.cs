namespace Clinic_System.Application.Features.Payment.Commands.Validators
{
    public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
    {
        public RefundPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.Reason).MaximumLength(500);
            RuleFor(x => x.Amount).GreaterThan(0).When(x => x.Amount.HasValue);
        }
    }
}
