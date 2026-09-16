namespace Clinic_System.Application.Service.Implemention
{
    public class InvoiceLineService : IInvoiceLineService
    {
        private readonly IUnitOfWork unitOfWork;

        public InvoiceLineService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<Payment> AddLinesAsync(int paymentId, List<InvoiceLineInput> lines, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(paymentId, cancellationToken);
            if (payment == null)
                throw new NotFoundException($"Payment with ID {paymentId} not found.");

            if (!payment.CanAddInvoiceLines)
                throw new InvalidOperationException("No se pueden agregar líneas a una factura reembolsada o cancelada.");

            if (lines == null || lines.Count == 0)
                throw new InvalidOperationException("At least one invoice line is required.");

            var existingLines = (await unitOfWork.InvoiceLinesRepository.GetByPaymentIdAsync(paymentId, cancellationToken)).ToList();
            var existingTotal = existingLines.Count > 0
                ? Money.Sum(existingLines.Select(l => l.LineTotal))
                : payment.Subtotal;

            if (existingLines.Count == 0 && existingTotal > 0.01m)
            {
                var carryForward = new InvoiceLine
                {
                    PaymentId = paymentId,
                    Description = "Cargo inicial de la factura",
                    Quantity = 1,
                    UnitPrice = existingTotal
                };
                await unitOfWork.InvoiceLinesRepository.AddAsync(carryForward, cancellationToken);
                payment.InvoiceLines.Add(carryForward);
            }
            else if (existingLines.Count == 0)
            {
                existingTotal = 0;
            }

            foreach (var line in lines)
            {
                var unitPrice = ResolveUnitPrice(line);
                var invoiceLine = new InvoiceLine
                {
                    PaymentId = paymentId,
                    Description = line.Description,
                    ToothNumber = line.ToothNumber,
                    Quantity = line.Quantity,
                    UnitPrice = unitPrice,
                    DentalTreatmentId = line.DentalTreatmentId
                };
                await unitOfWork.InvoiceLinesRepository.AddAsync(invoiceLine, cancellationToken);
                payment.InvoiceLines.Add(invoiceLine);
            }

            var addedTotal = Money.Sum(lines.Select(l => Money.Multiply(ResolveUnitPrice(l), l.Quantity)));
            var total = Money.Normalize(existingTotal + addedTotal);
            payment.RecalculateInvoiceAmount(total > 0 ? total : 0.01m);
            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);

            return payment;
        }

        public async Task<Payment> RemoveLineAsync(int lineId, CancellationToken cancellationToken = default)
        {
            var line = await unitOfWork.InvoiceLinesRepository.GetByIdAsync(lineId, cancellationToken)
                ?? throw new NotFoundException($"Invoice line with ID {lineId} not found.");

            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(line.PaymentId, cancellationToken)
                ?? throw new NotFoundException($"Payment with ID {line.PaymentId} not found.");

            if (!payment.CanEditInvoice)
                throw new InvalidOperationException("No se pueden eliminar líneas de una factura con abonos, reembolsada o cancelada.");

            unitOfWork.InvoiceLinesRepository.SoftDelete(line, cancellationToken);

            var remaining = (await unitOfWork.InvoiceLinesRepository.GetByPaymentIdAsync(payment.Id, cancellationToken))
                .Where(l => l.Id != lineId)
                .ToList();
            var total = Money.Sum(remaining.Select(l => l.LineTotal));
            payment.RecalculateInvoiceAmount(total > 0 ? total : 0.01m);
            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);
            return payment;
        }

        public Task<IEnumerable<InvoiceLine>> GetByPaymentIdAsync(int paymentId, CancellationToken cancellationToken = default)
            => unitOfWork.InvoiceLinesRepository.GetByPaymentIdAsync(paymentId, cancellationToken);

        private static decimal ResolveUnitPrice(InvoiceLineInput line)
        {
            if (string.IsNullOrWhiteSpace(line.UnitPriceInput))
                return Money.Normalize(line.UnitPrice);

            if (!Money.TryParse(line.UnitPriceInput, out var parsed))
                throw new InvalidOperationException("El precio unitario no es válido.");

            return parsed;
        }
    }
}
