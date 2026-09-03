namespace DentalCare.Admin.Models;

public class DoctorListItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? ApplicationUserId { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateDoctorRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male";
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UpdateDoctorRequest
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}

public class CreatedDoctorResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
