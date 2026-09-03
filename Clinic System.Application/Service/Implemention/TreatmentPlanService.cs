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
                    Notes = i.Notes
                }).ToList()
            };

            await unitOfWork.TreatmentPlansRepository.AddAsync(plan, cancellationToken);
            await AddEventAsync(plan, "Plan de tratamiento creado", "Estado: borrador.", recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> ApproveAsync(int planId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var plan = await unitOfWork.TreatmentPlansRepository.GetWithItemsAsync(planId, cancellationToken);
            if (plan == null)
                throw new NotFoundException($"Treatment plan with ID {planId} not found.");

            plan.Approve();
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Plan de tratamiento aprobado", plan.Notes, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> RejectAsync(
            int planId, string? reason, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var plan = await GetRequiredAsync(planId, cancellationToken);
            plan.Reject(reason);
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Plan de tratamiento rechazado", reason, recordedByUserId, cancellationToken);
            return plan;
        }

        public async Task<TreatmentPlan> CompleteAsync(
            int planId, string? recordedByUserId, CancellationToken cancellationToken = default)
        {
            var plan = await GetRequiredAsync(planId, cancellationToken);
            plan.Complete();
            unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
            await AddEventAsync(plan, "Plan de tratamiento completado", plan.Notes, recordedByUserId, cancellationToken);
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
