namespace Clinic_System.Application.Features.MedicalConditions.Commands.Models
{
    public class CreateMedicalConditionCommand : IRequest<Response<MedicalConditionDTO>>
    {
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
