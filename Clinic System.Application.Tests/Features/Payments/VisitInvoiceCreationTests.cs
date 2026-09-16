using Clinic_System.Core.Finance;

namespace Clinic_System.Application.Tests.Features.Payments;

public class VisitInvoiceCreationTests
{
    [Fact]
    public async Task CreatePaymentAsync_UsesQuotedAmountAndProcedureName()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var payments = new Mock<IPaymentRepository>();
        var appointments = new Mock<IAppointmentRepository>();
        unitOfWork.SetupGet(x => x.PaymentsRepository).Returns(payments.Object);
        unitOfWork.SetupGet(x => x.AppointmentsRepository).Returns(appointments.Object);
        payments.Setup(p => p.GetPaymentByAppointmentIdAsync(5)).ReturnsAsync((Payment?)null);
        appointments.Setup(a => a.GetAppointmentWithDetailsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Appointment
            {
                Id = 5,
                PatientId = 2,
                QuotedAmount = 800,
                TreatmentProcedure = new TreatmentProcedure { Name = "Sellantes", Price = 500 }
            });
        Payment? added = null;
        payments.Setup(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((payment, _) => added = payment)
            .Returns(Task.CompletedTask);

        var service = new PaymentService(unitOfWork.Object);
        var payment = await service.CreatePaymentAsync(5);

        added.Should().NotBeNull();
        added!.InvoiceLines.Should().ContainSingle();
        added.InvoiceLines.Single().Description.Should().Be("Sellantes");
        added.InvoiceLines.Single().UnitPrice.Should().Be(800);
        payment.AmountPaid.Should().Be(800);
        payment.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task CreatePaymentAsync_InvoicesCompletedPlanItem()
    {
        var plan = new TreatmentPlan { Title = "Plan", Status = TreatmentPlanStatus.Approved, PatientId = 2 };
        var item = new PlanItem
        {
            ProcedureName = "Endodoncia",
            Quantity = 1,
            UnitPrice = 4500,
            ToothNumber = 16,
            TreatmentPlan = plan,
            AcceptanceStatus = PlanItemAcceptanceStatus.Approved,
            ExecutionStatus = PlanItemExecutionStatus.Completed
        };
        plan.Items.Add(item);

        var unitOfWork = new Mock<IUnitOfWork>();
        var payments = new Mock<IPaymentRepository>();
        var appointments = new Mock<IAppointmentRepository>();
        var plans = new Mock<ITreatmentPlanRepository>();
        unitOfWork.SetupGet(x => x.PaymentsRepository).Returns(payments.Object);
        unitOfWork.SetupGet(x => x.AppointmentsRepository).Returns(appointments.Object);
        unitOfWork.SetupGet(x => x.TreatmentPlansRepository).Returns(plans.Object);
        payments.Setup(p => p.GetPaymentByAppointmentIdAsync(9)).ReturnsAsync((Payment?)null);
        appointments.Setup(a => a.GetAppointmentWithDetailsAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Appointment
            {
                Id = 9,
                PatientId = 2,
                PlanItemId = 1,
                PlanItem = item,
                TreatmentPlan = plan,
                ToothNumber = 16
            });
        payments.Setup(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new PaymentService(unitOfWork.Object);
        var payment = await service.CreatePaymentAsync(9);

        payment.InvoiceLines.Should().ContainSingle(l => l.Description == "Endodoncia" && l.UnitPrice == 4500 && l.ToothNumber == 16);
        item.InvoicedPayment.Should().Be(payment);
        plan.Invoices.Should().Contain(payment);
        plans.Verify(p => p.Update(plan, It.IsAny<CancellationToken>()), Times.Once);
    }
}
