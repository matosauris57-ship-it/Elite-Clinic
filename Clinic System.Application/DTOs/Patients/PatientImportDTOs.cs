namespace Clinic_System.Application.DTOs.Patients;

public sealed class PatientImportPreviewDTO
{
    public int TotalRows { get; set; }
    public int ValidCount { get; set; }
    public int ErrorCount { get; set; }
    public List<PatientImportRowDTO> Rows { get; set; } = [];
    public string TemplateHint { get; set; } =
        "Columnas: FullName, Gender, DateOfBirth, Address, Phone, NationalId, Email, MobilePhone "
        + "(también Nombre, Genero, FechaNacimiento, Direccion, Telefono, Cedula, Correo, Celular).";
}

public sealed class PatientImportResultDTO
{
    public int Requested { get; set; }
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<PatientImportRowDTO> Rows { get; set; } = [];
}

public sealed class PatientImportRowDTO
{
    public int RowNumber { get; set; }
    public bool IsValid { get; set; }
    public bool Imported { get; set; }
    public int? PatientId { get; set; }
    public string? Error { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
}

public sealed class PatientImportConfirmRequest
{
    public List<PatientImportConfirmRow> Rows { get; set; } = [];
}

public sealed class PatientImportConfirmRow
{
    public int RowNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
}
