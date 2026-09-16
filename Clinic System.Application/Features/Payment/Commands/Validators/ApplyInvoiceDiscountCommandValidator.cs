namespace Clinic_System.Application.Features.Payment.Commands.Validators
{
    public class ApplyInvoiceDiscountCommandValidator : AbstractValidator<ApplyInvoiceDiscountCommand>
    {
        public ApplyInvoiceDiscountCommandValidator()
        {
            RuleFor(x => x.PaymentId).GreaterThan(0);
            RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        }
    }
}
