using Clinic_System.Core.Finance;

namespace Clinic_System.Application.Tests.Features.Payments;

public class PaymentAccountTests
{
    [Fact]
    public void InvoiceDiscount_ReducesTotalWithoutChangingLineSubtotal()
    {
        var payment = new Payment
        {
            AmountPaid = 1000,
            InvoiceLines =
            [
                new InvoiceLine { Description = "Corona", Quantity = 1, UnitPrice = 800 },
                new InvoiceLine { Description = "Limpieza", Quantity = 1, UnitPrice = 200 }
            ]
        };

        payment.ApplyDiscount(100);

        payment.Subtotal.Should().Be(1000);
        payment.InvoiceTotal.Should().Be(900);
        payment.AmountPaid.Should().Be(900);
        payment.DiscountAmount.Should().Be(100);
    }

    [Fact]
    public void PartialRefund_UndoesCollectedMoney()
    {
        var payment = new Payment { AmountPaid = 800 };
        payment.ApplyReceipt(500, PaymentMethod.Cash, "abono");

        payment.ApplyRefund(200, PaymentMethod.Cash, "reembolso parcial");

        payment.AmountCollected.Should().Be(300);
        payment.Balance.Should().Be(500);
        payment.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
        payment.Receipts.Should().Contain(r => r.Kind == PaymentReceiptKind.Refund && r.Amount == 200);
    }

    [Fact]
    public void VoidReceipt_RestoresBalance()
    {
        var payment = new Payment { AmountPaid = 800 };
        var receipt = payment.ApplyReceipt(300, PaymentMethod.Cash);
        receipt.Id = 11;

        payment.VoidReceipt(11, "captura duplicada");

        receipt.IsVoided.Should().BeTrue();
        payment.AmountCollected.Should().Be(0);
        payment.PaymentStatus.Should().Be(PaymentStatus.Pending);
        payment.Balance.Should().Be(800);
    }

    [Fact]
    public void Plan_AllowsMultipleInvoicesAgainstRemainingBalance()
    {
        var plan = new TreatmentPlan
        {
            Title = "8 coronas",
            Status = TreatmentPlanStatus.Approved,
            Items =
            [
                new PlanItem { ProcedureName = "Corona", Quantity = 8, UnitPrice = 1000 }
            ]
        };

        var first = new Payment { AmountPaid = 2000, InvoiceLines = [new InvoiceLine { Description = "Cuenta", Quantity = 1, UnitPrice = 2000 }] };
        first.RecalculateInvoiceAmount();
        plan.AttachInvoice(first);

        plan.RemainingToBill.Should().Be(6000);
        plan.CanInvoice.Should().BeTrue();

        var second = new Payment { AmountPaid = 6000, InvoiceLines = [new InvoiceLine { Description = "Saldo", Quantity = 1, UnitPrice = 6000 }] };
        second.RecalculateInvoiceAmount();
        plan.AttachInvoice(second);

        plan.RemainingToBill.Should().Be(0);
        plan.CanInvoice.Should().BeFalse();
        plan.Invoices.Should().HaveCount(2);
    }

    [Fact]
    public void VisitInvoice_IsNotThePlanAccount()
    {
        var visit = new Payment
        {
            AppointmentId = 9,
            AmountPaid = 150,
            AdditionalNotes = "Factura de visita"
        };
        visit.TreatmentPlanId.Should().BeNull();
        visit.InvoiceTotal.Should().Be(150);
    }
}
