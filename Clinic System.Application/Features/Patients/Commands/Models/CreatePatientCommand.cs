namespace Clinic_System.Application.Features.Patients.Commands.Models
{
    public class CreatePatientCommand : IRequest<Response<CreatePatientDTO>>
    {
        public string FullName { get; set; } = null!;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = null!;
        public string? NationalId { get; set; }
        public string? MobilePhone { get; set; }
        public string? Email { get; set; }
        public string Address { get; set; } = null!;
    }
}
