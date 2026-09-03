namespace Clinic_System.Application.DTOs.Dental
{
    public class TreatmentProcedureDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string PriceDisplay { get; set; } = string.Empty;
        public string PriceRaw { get; set; } = string.Empty;
        public string PriceRangeDisplay { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DoctorProcedurePriceDTO> DoctorPrices { get; set; } = [];
    }

    public class DoctorProcedurePriceDTO
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PriceDisplay { get; set; } = string.Empty;
        public string PriceRaw { get; set; } = string.Empty;
    }

    public class DoctorProcedurePriceInput
    {
        public int DoctorId { get; set; }
        public decimal Price { get; set; }
    }
}
