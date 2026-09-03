namespace Clinic_System.Application.Features.TreatmentProcedures.Commands.Models
{
    public class UpdateTreatmentProcedureCommand : IRequest<Response<TreatmentProcedureDTO>>
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public List<DoctorProcedurePriceInput> DoctorPrices { get; set; } = [];
    }
}
