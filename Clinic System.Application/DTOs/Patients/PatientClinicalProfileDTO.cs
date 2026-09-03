namespace Clinic_System.Application.DTOs.Patients
{
    public class PatientClinicalProfileDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string DateOfBirth { get; set; } = null!;
        public string DateOfBirthIso { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? NationalId { get; set; }
        public string? MobilePhone { get; set; }
        public string? Email { get; set; }
        public bool OptOutEmailCampaigns { get; set; }
        public bool IsActive { get; set; } = true;
        public DentalHistoryDTO? DentalHistory { get; set; }
    }
}
