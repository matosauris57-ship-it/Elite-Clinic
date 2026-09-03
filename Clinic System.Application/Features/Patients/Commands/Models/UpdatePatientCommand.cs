namespace Clinic_System.Application.Features.Patients.Commands.Models
{
    public class UpdatePatientCommand : IRequest<Response<UpdatePatientDTO>>
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? MobilePhone { get; set; }
        public string? NationalId { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public bool? OptOutEmailCampaigns { get; set; }
    }
}
