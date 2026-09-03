namespace Clinic_System.Application.DTOs.MedicalConditions
{
    public class MedicalConditionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}
