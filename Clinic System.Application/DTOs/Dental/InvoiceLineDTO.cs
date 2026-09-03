namespace Clinic_System.Application.DTOs.Dental
{
    public class InvoiceLineDTO
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string Description { get; set; } = null!;
        public int? ToothNumber { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitPriceDisplay { get; set; } = string.Empty;
        public decimal LineTotal { get; set; }
        public string LineTotalDisplay { get; set; } = string.Empty;
        public string MetaDisplay { get; set; } = string.Empty;
        public int? DentalTreatmentId { get; set; }
    }
}
