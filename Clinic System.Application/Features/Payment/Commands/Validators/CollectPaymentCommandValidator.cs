namespace Clinic_System.Application.Features.Payment.Commands.Validators
{
    public class CollectPaymentCommandValidator : AbstractValidator<CollectPaymentCommand>
    {
        public CollectPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.PaymentMethod).IsInEnum();
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .When(x => x.Amount.HasValue && string.IsNullOrWhiteSpace(x.AmountInput));
            RuleFor(x => x.AmountInput)
                .Must(value => Money.TryParse(value, out var amount) && amount > 0)
                .When(x => !string.IsNullOrWhiteSpace(x.AmountInput))
                .WithMessage("El monto no es válido.");
        }
    }
}
