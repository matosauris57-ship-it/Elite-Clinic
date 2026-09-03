namespace Clinic_System.Application.Features.MedicalConditions.Commands.Models
{
    public class UpdateMedicalConditionCommand : IRequest<Response<MedicalConditionDTO>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
