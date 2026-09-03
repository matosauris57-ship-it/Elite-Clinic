namespace Clinic_System.Application.Features.InvoiceLines.Commands.Validators
{
    public class AddInvoiceLinesValidator : AbstractValidator<AddInvoiceLinesCommand>
    {
        public AddInvoiceLinesValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.Lines).NotEmpty();
            RuleForEach(x => x.Lines).ChildRules(line =>
            {
                line.RuleFor(l => l.Description).NotEmpty();
                line.RuleFor(l => l.Quantity).GreaterThan(0);
                line.RuleFor(l => l.UnitPrice)
                    .GreaterThanOrEqualTo(0)
                    .When(l => string.IsNullOrWhiteSpace(l.UnitPriceInput));
                line.RuleFor(l => l.UnitPriceInput)
                    .Must(value => Money.TryParse(value, out _))
                    .When(l => !string.IsNullOrWhiteSpace(l.UnitPriceInput))
                    .WithMessage("El precio unitario no es válido.");
            });
        }
    }
}
