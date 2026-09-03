namespace Clinic_System.Application.Features.TreatmentProcedures.Commands.Models
{
    public class SoftDeleteTreatmentProcedureCommand : IRequest<Response<string>>
    {
        public int Id { get; set; }
    }
}
