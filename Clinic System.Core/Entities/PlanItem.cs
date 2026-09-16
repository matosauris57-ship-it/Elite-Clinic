using Clinic_System.Core.Finance;

namespace Clinic_System.Core.Entities
{
    public class PlanItem : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int TreatmentPlanId { get; set; }
        public virtual TreatmentPlan TreatmentPlan { get; set; } = null!;

        public virtual string ProcedureName { get; set; } = null!;
        public virtual int? TreatmentProcedureId { get; set; }
        public virtual TreatmentProcedure? TreatmentProcedure { get; set; }
        public virtual int? ToothNumber { get; set; }
        public virtual ToothSurface? ToothSurface { get; set; }
        public virtual int Quantity { get; set; } = 1;
        public virtual decimal UnitPrice { get; set; }
        public virtual string? Notes { get; set; }

        public virtual PlanItemAcceptanceStatus AcceptanceStatus { get; set; } = PlanItemAcceptanceStatus.Pending;
        public virtual PlanItemExecutionStatus ExecutionStatus { get; set; } = PlanItemExecutionStatus.Pending;
        public virtual int? DentalTreatmentId { get; set; }
        public virtual DentalTreatment? DentalTreatment { get; set; }
        public virtual int? ScheduledAppointmentId { get; set; }
        public virtual Appointment? ScheduledAppointment { get; set; }
        public virtual int? InvoicedPaymentId { get; set; }
        public virtual Payment? InvoicedPayment { get; set; }

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal LineTotal => Money.Multiply(UnitPrice, Quantity);

        public bool CanAccept => AcceptanceStatus == PlanItemAcceptanceStatus.Pending;
        public bool CanReject => AcceptanceStatus == PlanItemAcceptanceStatus.Pending;
        public bool CanSchedule =>
            AcceptanceStatus == PlanItemAcceptanceStatus.Approved
            && ExecutionStatus is PlanItemExecutionStatus.Pending or PlanItemExecutionStatus.Scheduled;
        public bool CanInvoice =>
            AcceptanceStatus == PlanItemAcceptanceStatus.Approved
            && ExecutionStatus == PlanItemExecutionStatus.Completed
            && InvoicedPaymentId == null;

        public void Accept()
        {
            if (!CanAccept)
                throw new InvalidOperationException("Este procedimiento ya fue aceptado o rechazado.");

            AcceptanceStatus = PlanItemAcceptanceStatus.Approved;
            ExecutionStatus = PlanItemExecutionStatus.Pending;
            UpdatedAt = DateTime.Now;
        }

        public void Reject()
        {
            if (!CanReject)
                throw new InvalidOperationException("Este procedimiento ya fue aceptado o rechazado.");

            AcceptanceStatus = PlanItemAcceptanceStatus.Rejected;
            ExecutionStatus = PlanItemExecutionStatus.Cancelled;
            UpdatedAt = DateTime.Now;
        }

        public void MarkScheduled(int appointmentId)
        {
            if (AcceptanceStatus != PlanItemAcceptanceStatus.Approved)
                throw new InvalidOperationException("Solo un procedimiento aceptado se puede agendar.");
            if (ExecutionStatus is PlanItemExecutionStatus.Completed or PlanItemExecutionStatus.Cancelled)
                throw new InvalidOperationException("No se puede agendar un procedimiento completado o cancelado.");

            ScheduledAppointmentId = appointmentId;
            ExecutionStatus = PlanItemExecutionStatus.Scheduled;
            UpdatedAt = DateTime.Now;
        }

        public void MarkInProgress()
        {
            if (AcceptanceStatus != PlanItemAcceptanceStatus.Approved)
                throw new InvalidOperationException("Solo un procedimiento aceptado puede iniciarse.");
            if (ExecutionStatus is PlanItemExecutionStatus.Completed or PlanItemExecutionStatus.Cancelled)
                throw new InvalidOperationException("No se puede iniciar un procedimiento completado o cancelado.");

            ExecutionStatus = PlanItemExecutionStatus.InProgress;
            UpdatedAt = DateTime.Now;
        }

        public void MarkCompleted()
        {
            if (AcceptanceStatus != PlanItemAcceptanceStatus.Approved)
                throw new InvalidOperationException("Solo un procedimiento aceptado puede completarse.");
            if (ExecutionStatus == PlanItemExecutionStatus.Cancelled)
                throw new InvalidOperationException("No se puede completar un procedimiento cancelado.");

            ExecutionStatus = PlanItemExecutionStatus.Completed;
            UpdatedAt = DateTime.Now;
        }

        public void MarkInvoiced(int paymentId)
        {
            InvoicedPaymentId = paymentId;
            UpdatedAt = DateTime.Now;
        }
    }
}
