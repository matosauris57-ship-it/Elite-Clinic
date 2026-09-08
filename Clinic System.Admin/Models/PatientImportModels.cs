namespace DentalCare.Admin.Models;

public class PatientImportPreview
{
    public int TotalRows { get; set; }
    public int ValidCount { get; set; }
    public int ErrorCount { get; set; }
    public List<PatientImportRow> Rows { get; set; } = [];
    public string? TemplateHint { get; set; }
}

public class PatientImportResult
{
    public int Requested { get; set; }
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<PatientImportRow> Rows { get; set; } = [];
}

public class PatientImportRow
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

public class PatientImportConfirmPayload
{
    public List<PatientImportConfirmRow> Rows { get; set; } = [];
}

public class PatientImportConfirmRow
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
