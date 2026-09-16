namespace Clinic_System.Application.Service.Implemention
{
    public class TreatmentPlanService : ITreatmentPlanService
    {
        private readonly IUnitOfWork unitOfWork;

        public TreatmentPlanService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<TreatmentPlan> CreateAsync(
            int patientId, string title, string? notes, DateTime? validUntil, decimal discountAmount,
            List<PlanItemInput> items, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var patient = await unitOfWork.PatientsRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient == null)
                throw new NotFoundException($"Patient with ID {patientId} not found.");

            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Treatment plan must include at least one item.");

            foreach (var item in items)
            {
                if (item.ToothNumber.HasValue && !FdiToothNumber.IsValid(item.ToothNumber.Value))
                    throw new InvalidOperationException("El diente debe usar una notación FDI válida.");
                if (item.TreatmentProcedureId.HasValue &&
                    await unitOfWork.TreatmentProceduresRepository.GetByIdAsync(item.TreatmentProcedureId.Value, cancellationToken) == null)
                    throw new NotFoundException($"Treatment procedure with ID {item.TreatmentProcedureId} not found.");
            }

            if (discountAmount > 0)
            {
                var subtotal = Money.Sum(items.Select(i => Money.Multiply(i.UnitPrice, i.Quantity)));
                if (Money.Normalize(discountAmount) > subtotal)
                    throw new InvalidOperationException("El descuento no puede superar el subtotal del presupuesto.");
            }

            var plan = new TreatmentPlan
            {
                PatientId = patientId,
                Title = title,
                Notes = notes,
                ValidUntil = validUntil,
                DiscountAmount = discountAmount,
                Status = TreatmentPlanStatus.Draft,
                Items = items.Select(i => new PlanItem
                {
                    ProcedureName = i.ProcedureName,
                    TreatmentProcedureId = i.TreatmentProcedureId,
                    ToothNumber = i.ToothNumber,
                    ToothSurface = i.ToothSurface,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Notes = i.Notes,
                    AcceptanceStatus = PlanItemAcceptanceStatus.Pending,
                    ExecutionStatus = PlanItemExecutionStatus.Pending
                }).ToList()
            };

            await unitOfWork.TreatmentPlansRepository.AddAsync(plan, cancellationToken);
            await AddEventAsync(plan, "Presupuesto creado", "Estado: borrador.", recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> IssueAsync(int planId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var plan = await GetRequiredAsync(planId, cancellationToken);
            plan.Issue();
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Presupuesto entregado", plan.Title, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> ApproveAsync(int planId, string? recordedByUserId, string? acceptedByName = null, CancellationToken cancellationToken = default)
        {
            var plan = await unitOfWork.TreatmentPlansRepository.GetWithItemsAsync(planId, cancellationToken);
            if (plan == null)
                throw new NotFoundException($"Treatment plan with ID {planId} not found.");

            plan.Approve(acceptedByName);
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Presupuesto aceptado", acceptedByName, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> RejectAsync(
            int planId, string? reason, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var plan = await GetRequiredAsync(planId, cancellationToken);
            plan.Reject(reason);
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Presupuesto rechazado", reason, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> CompleteAsync(
            int planId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var plan = await GetRequiredAsync(planId, cancellationToken);
            plan.Complete();
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Presupuesto completado", plan.Notes, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> InvoiceAsync(
            int planId, string? recordedByUserId, decimal? amount = null, bool completedOnly = false, CancellationToken cancellationToken = default)
        {
            var plan = await GetRequiredAsync(planId, cancellationToken);
            if (plan.IsExpired)
                throw new InvalidOperationException("Este presupuesto está vencido. Cree uno nuevo para facturar.");
            if (plan.Status != TreatmentPlanStatus.Approved)
                throw new InvalidOperationException("Acepte el presupuesto antes de facturarlo.");

            if (completedOnly)
                return await InvoiceCompletedItemsAsync(plan, recordedByUserId, cancellationToken);

            if (!plan.CanInvoice)
                throw new InvalidOperationException("Este plan ya no tiene saldo por facturar. Use abonos en las facturas existentes.");

            var remaining = plan.RemainingToBill;
            var toBill = amount is > 0 ? Money.Normalize(amount.Value) : remaining;
            if (toBill <= 0)
                throw new InvalidOperationException("El monto a facturar debe ser mayor a cero.");
            if (toBill > remaining)
                throw new InvalidOperationException($"El monto no puede superar el saldo por facturar ({Money.Format(remaining)}).");

            var isFullRemaining = toBill == remaining && plan.AmountBilled <= 0;
            var lines = isFullRemaining
                ? plan.ToBillableLines()
                : [("Cuenta del plan: " + plan.Title, (int?)null, 1, toBill)];

            if (lines.Count == 0)
                throw new InvalidOperationException("El presupuesto no tiene ítems para facturar.");

            var invoiceCount = plan.InvoicePaymentIds.Count + 1;
            var note = $"Plan #{plan.Id} · factura {invoiceCount}: {plan.Title}";
            if (note.Length > 500)
                note = note[..500];

            var payment = new Payment
            {
                PatientId = plan.PatientId,
                AppointmentId = null,
                AmountPaid = toBill,
                PaymentStatus = PaymentStatus.Pending,
                AdditionalNotes = note,
                PaymentDate = null
            };

            foreach (var line in lines)
            {
                payment.InvoiceLines.Add(new InvoiceLine
                {
                    Description = line.Description,
                    ToothNumber = line.ToothNumber,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice
                });
            }

            payment.RecalculateInvoiceAmount();
            plan.AttachInvoice(payment);
            await unitOfWork.PaymentsRepository.AddAsync(payment, cancellationToken);
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Presupuesto facturado", note, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> AcceptItemAsync(int itemId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var item = await unitOfWork.TreatmentPlansRepository.GetItemWithPlanAsync(itemId, cancellationToken)
                ?? throw new NotFoundException("No se encontró el procedimiento del presupuesto.");
            var plan = item.TreatmentPlan;
            if (plan.Status != TreatmentPlanStatus.Approved)
                throw new InvalidOperationException("Acepte el presupuesto antes de aceptar cada procedimiento.");

            item.Accept();
            if (item.DentalTreatmentId == null)
            {
                var treatment = new DentalTreatment
                {
                    PatientId = plan.PatientId,
                    ProcedureName = item.ProcedureName,
                    TreatmentProcedureId = item.TreatmentProcedureId,
                    ToothNumber = item.ToothNumber,
                    ToothSurface = item.ToothSurface,
                    Cost = item.LineTotal,
                    Status = DentalTreatmentStatus.Planned,
                    ProcedureDetails = "Pendiente de ejecución en el plan de tratamiento."
                };
                await unitOfWork.DentalTreatmentsRepository.AddAsync(treatment, cancellationToken);
                item.DentalTreatment = treatment;
            }

            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Procedimiento aceptado", item.ProcedureName, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> RejectItemAsync(int itemId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var item = await unitOfWork.TreatmentPlansRepository.GetItemWithPlanAsync(itemId, cancellationToken)
                ?? throw new NotFoundException("No se encontró el procedimiento del presupuesto.");
            var plan = item.TreatmentPlan;
            if (plan.Status != TreatmentPlanStatus.Approved)
                throw new InvalidOperationException("Acepte el presupuesto antes de rechazar procedimientos.");

            item.Reject();
            if (item.DentalTreatment != null &&
                item.DentalTreatment.Status is DentalTreatmentStatus.Planned or DentalTreatmentStatus.InProgress)
            {
                item.DentalTreatment.Cancel("Procedimiento rechazado en el presupuesto.");
            }

            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Procedimiento rechazado", item.ProcedureName, recordedByUserId, cancellationToken);
            return plan;
        }

        private async Task<TreatmentPlan> InvoiceCompletedItemsAsync(
            TreatmentPlan plan, string? recordedByUserId, CancellationToken cancellationToken)
        {
            var items = plan.BillableCompletedItems.ToList();
            if (items.Count == 0)
                throw new InvalidOperationException("No hay procedimientos realizados pendientes de facturar.");

            var total = Money.Sum(items.Select(i => i.LineTotal));
            var invoiceCount = plan.InvoicePaymentIds.Count + 1;
            var note = $"Plan #{plan.Id} · realizados: {plan.Title}";
            if (note.Length > 500)
                note = note[..500];

            var payment = new Payment
            {
                PatientId = plan.PatientId,
                AmountPaid = total,
                PaymentStatus = PaymentStatus.Pending,
                AdditionalNotes = note
            };

            foreach (var item in items)
            {
                payment.InvoiceLines.Add(new InvoiceLine
                {
                    Description = item.ProcedureName,
                    ToothNumber = item.ToothNumber,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DentalTreatmentId = item.DentalTreatmentId
                });
                item.InvoicedPayment = payment;
            }

            payment.RecalculateInvoiceAmount();
            plan.AttachInvoice(payment);
            await unitOfWork.PaymentsRepository.AddAsync(payment, cancellationToken);
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Factura de realizados", note, recordedByUserId, cancellationToken);
            return plan;
        }

        public Task<TreatmentPlan?> GetByIdAsync(int planId, CancellationToken cancellationToken = default)
            => unitOfWork.TreatmentPlansRepository.GetWithItemsAsync(planId, cancellationToken);

        public Task<IEnumerable<TreatmentPlan>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
            => unitOfWork.TreatmentPlansRepository.GetByPatientIdAsync(patientId, cancellationToken);

        private async Task<TreatmentPlan> GetRequiredAsync(int planId, CancellationToken cancellationToken)
        {
            var plan = await unitOfWork.TreatmentPlansRepository.GetWithItemsAsync(planId, cancellationToken);
            return plan ?? throw new NotFoundException($"Treatment plan with ID {planId} not found.");
        }

        private Task AddEventAsync(
            TreatmentPlan plan, string title, string? description, string? recordedByUserId,
            CancellationToken cancellationToken)
        {
            var teeth = plan.Items.Where(x => x.ToothNumber.HasValue).Select(x => x.ToothNumber!.Value).Distinct().ToList();
            var recordedAt = DateTime.UtcNow;
            return unitOfWork.DentalClinicalEventsRepository.AddAsync(new DentalClinicalEvent
            {
                PatientId = plan.PatientId,
                ToothNumber = teeth.Count == 1 ? teeth[0] : null,
                Type = DentalClinicalEventType.TreatmentPlan,
                Phase = plan.Status == TreatmentPlanStatus.Completed ? ToothChartPhase.Completed : ToothChartPhase.Planned,
                Title = title,
                Description = string.IsNullOrWhiteSpace(description) ? plan.Title : $"{plan.Title}. {description}",
                ReferenceType = nameof(TreatmentPlan),
                ReferenceId = plan.Id > 0 ? plan.Id.ToString() : $"patient:{plan.PatientId}:{recordedAt:O}",
                RecordedByUserId = recordedByUserId,
                RecordedAt = recordedAt
            }, cancellationToken);
        }
    }
}
