namespace Clinic_System.Application.Common;

public static class BillingCopy
{
    public static string Method(PaymentMethod? method) => method switch
    {
        PaymentMethod.Cash => "Efectivo",
        PaymentMethod.CreditCard => "Tarjeta",
        PaymentMethod.InstaPay => "InstaPay",
        _ => "Sin método"
    };

    public static string Status(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Pendiente",
        PaymentStatus.PartiallyPaid => "Abonado",
        PaymentStatus.Paid => "Pagado",
        PaymentStatus.Failed => "Fallido",
        PaymentStatus.Refunded => "Reembolsado",
        PaymentStatus.Cancelled => "Cancelado",
        _ => status.ToString()
    };
}
