namespace Clinic_System.Application.Features.InvoiceLines.Commands.Validators
{
    public class DeleteInvoiceLineCommandValidator : AbstractValidator<DeleteInvoiceLineCommand>
    {
        public DeleteInvoiceLineCommandValidator()
        {
            RuleFor(x => x.LineId).GreaterThan(0);
        }
    }
}
