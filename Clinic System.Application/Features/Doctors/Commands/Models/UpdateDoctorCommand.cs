namespace Clinic_System.Application.Features.Doctors.Commands.Models
{
    public class UpdateDoctorCommand : IRequest<Response<UpdateDoctorDTO>>
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Specialization { get; set; }
        public string? SignatureImageUrl { get; set; }
        public bool ClearSignatureImage { get; set; }
        public bool? CanViewAllClinicData { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
