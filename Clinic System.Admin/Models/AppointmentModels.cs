namespace DentalCare.Admin.Models;

public class CreatedPatientResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class AvailableSlot
{
    public TimeSpan SlotTime { get; set; }
}

public class BookAppointmentRequest
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public int? TreatmentProcedureId { get; set; }
    public decimal? QuotedAmount { get; set; }
}

public class BookAppointmentResult
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string AppointmentDateTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AppointmentDuration { get; set; }
}

public class CreatePatientRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male";
    public int Age { get; set; } = 30;
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
}

public class TreatmentCatalogItem
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceRangeDisplay { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public List<DoctorProcedurePriceItem> DoctorPrices { get; set; } = [];
}

public class AppointmentWizardState
{
    public int CurrentStep { get; set; } = 1;
    public PatientListItem? SelectedPatient { get; set; }
    public TreatmentCatalogItem? SelectedTreatment { get; set; }
    public DoctorListItem? SelectedDoctor { get; set; }
    public DateTime? SelectedDate { get; set; }
    public TimeSpan? SelectedTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool ReminderEnabled { get; set; } = true;
    public string ReminderChannel { get; set; } = "WhatsApp";
    public string Room { get; set; } = "Sala 1";
    public string QuotedPriceInput { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public BookAppointmentResult? BookedAppointment { get; set; }
}

public class AppointmentAgendaItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string? PatientEmail { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorPhone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string AppointmentDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime? ParsedDate =>
        DateTime.TryParseExact(AppointmentDate, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.None, out var dt)
            ? dt
            : null;
}

public class ConfirmAppointmentRequest
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string method { get; set; } = "Cash";
    public decimal? amount { get; set; }
    public string? Notes { get; set; }
}

public class CancelAppointmentRequest
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
}

public class RescheduleAppointmentRequest
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
}

public class CompleteAppointmentRequest
{
    public int AppointmentId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AdditionalNotes { get; set; }
    public List<PrescriptionItem> Medicines { get; set; } = [];
}

public class PrescriptionItem
{
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? SpecialInstructions { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
}

public class NoShowAppointmentRequest
{
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
}
