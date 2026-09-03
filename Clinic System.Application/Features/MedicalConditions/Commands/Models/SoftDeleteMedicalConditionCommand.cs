namespace Clinic_System.Application.Features.MedicalConditions.Commands.Models
{
    public class SoftDeleteMedicalConditionCommand : IRequest<Response<string>>
    {
        public int Id { get; set; }
    }
}
