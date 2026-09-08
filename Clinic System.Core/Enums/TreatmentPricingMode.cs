namespace Clinic_System.Core.Enums;

public enum TreatmentPricingMode
{
    /// <summary>Precio fijo definido en el catálogo.</summary>
    Fixed = 0,
    /// <summary>El monto se indica al agendar la cita.</summary>
    AtBooking = 1,
    /// <summary>El monto se indica al facturar.</summary>
    AtBilling = 2
}
