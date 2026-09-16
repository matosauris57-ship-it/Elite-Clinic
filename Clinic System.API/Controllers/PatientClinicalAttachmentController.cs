using Clinic_System.Core.Authorization;
using Clinic_System.Core.Entities;
using Clinic_System.Infrastructure.Authorization;

namespace Clinic_System.API.Controllers;

[Route("api/patients/{patientId:int}/clinical-attachments")]
[ApiController]
[Authorize]
public class PatientClinicalAttachmentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IClinicalAttachmentStorage _storage;
    private readonly ICurrentUserService _currentUser;
    private readonly IClinicDataScopeService _clinicScope;

    public PatientClinicalAttachmentController(
        AppDbContext db,
        IClinicalAttachmentStorage storage,
        ICurrentUserService currentUser,
        IClinicDataScopeService clinicScope)
    {
        _db = db;
        _storage = storage;
        _currentUser = currentUser;
        _clinicScope = clinicScope;
    }

    [HttpGet]
    public async Task<IActionResult> List(int patientId, CancellationToken ct)
    {
        if (!CanView())
            return Forbid();

        var scopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (scopeDenied != null)
            return scopeDenied;

        var exists = await _db.Patients.AnyAsync(p => p.Id == patientId, ct);
        if (!exists)
            return NotFound(new { succeeded = false, message = "Paciente no encontrado." });

        var items = await _db.PatientClinicalAttachments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.CapturedOn ?? a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .ToListAsync(ct);

        return Ok(new { succeeded = true, data = items.Select(Map) });
    }

    [HttpGet("{id:int}/file")]
    public async Task<IActionResult> Download(int patientId, int id, CancellationToken ct)
    {
        if (!CanView())
            return Forbid();

        var downloadScopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (downloadScopeDenied != null)
            return downloadScopeDenied;

        var attachment = await _db.PatientClinicalAttachments
            .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId, ct);
        if (attachment == null)
            return NotFound(new { succeeded = false, message = "Adjunto no encontrado." });

        try
        {
            var stream = _storage.OpenRead(patientId, attachment.StoredFileName);
            var inline = attachment.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(attachment.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
            if (inline)
                return File(stream, attachment.ContentType);

            return File(stream, attachment.ContentType, attachment.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { succeeded = false, message = "El archivo ya no está en el servidor." });
        }
    }

    [HttpPost]
    [RequestSizeLimit(FileClinicalAttachmentStorage.DefaultMaxFileBytes + (1024 * 1024))]
    [RequestFormLimits(MultipartBodyLengthLimit = FileClinicalAttachmentStorage.DefaultMaxFileBytes + (1024 * 1024))]
    public async Task<IActionResult> Upload(int patientId, [FromForm] UploadClinicalAttachmentRequest request, CancellationToken ct)
    {
        if (!CanEdit())
            return Forbid();

        var uploadScopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (uploadScopeDenied != null)
            return uploadScopeDenied;

        if (request.File == null || request.File.Length <= 0)
            return BadRequest(new { succeeded = false, message = "Seleccione un archivo." });

        if (!_storage.IsAllowed(request.File.FileName, request.File.ContentType))
            return BadRequest(new { succeeded = false, message = "Formato no permitido. Use JPG, PNG, WEBP, GIF, PDF o Word." });

        if (request.File.Length > _storage.MaxFileBytes)
            return BadRequest(new { succeeded = false, message = "El archivo no puede superar 20 MB." });

        var patientExists = await _db.Patients.AnyAsync(p => p.Id == patientId, ct);
        if (!patientExists)
            return NotFound(new { succeeded = false, message = "Paciente no encontrado." });

        StoredClinicalAttachment stored;
        try
        {
            await using var input = request.File.OpenReadStream();
            stored = await _storage.SaveAsync(patientId, request.File.FileName, request.File.ContentType, input, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { succeeded = false, message = ex.Message });
        }

        var kind = Enum.IsDefined(request.Kind) ? request.Kind : InferKind(stored.ContentType);
        var title = string.IsNullOrWhiteSpace(request.Title)
            ? DefaultTitle(kind, request.Subtype, stored.OriginalFileName)
            : Clamp(request.Title.Trim(), 200);

        var attachment = new PatientClinicalAttachment
        {
            PatientId = patientId,
            Kind = kind,
            Title = title,
            Subtype = Trim(request.Subtype, 80),
            Notes = Trim(request.Notes, 1000),
            ToothNumber = NormalizeTooth(request.ToothNumber),
            CapturedOn = NormalizeDate(request.CapturedOn),
            OriginalFileName = stored.OriginalFileName,
            StoredFileName = stored.StoredFileName,
            ContentType = stored.ContentType,
            FileSizeBytes = stored.FileSizeBytes,
            RecordedByUserId = _currentUser.UserId
        };

        var clinicalEvent = new DentalClinicalEvent
        {
            PatientId = patientId,
            ToothNumber = attachment.ToothNumber,
            Type = DentalClinicalEventType.Attachment,
            Title = TimelineTitle(kind),
            Description = TimelineDescription(attachment),
            ReferenceType = nameof(PatientClinicalAttachment),
            RecordedByUserId = _currentUser.UserId,
            RecordedAt = attachment.CapturedOn ?? DateTime.Now
        };

        _db.PatientClinicalAttachments.Add(attachment);
        _db.DentalClinicalEvents.Add(clinicalEvent);
        await _db.SaveChangesAsync(ct);

        clinicalEvent.ReferenceId = attachment.Id.ToString();
        await _db.SaveChangesAsync(ct);

        return Ok(new { succeeded = true, data = Map(attachment) });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int patientId, int id, [FromBody] UpdateClinicalAttachmentRequest request, CancellationToken ct)
    {
        if (!CanEdit())
            return Forbid();

        var updateScopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (updateScopeDenied != null)
            return updateScopeDenied;

        var attachment = await _db.PatientClinicalAttachments
            .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId, ct);
        if (attachment == null)
            return NotFound(new { succeeded = false, message = "Adjunto no encontrado." });

        if (!string.IsNullOrWhiteSpace(request.Title))
            attachment.Title = Clamp(request.Title.Trim(), 200);
        if (request.Kind.HasValue && Enum.IsDefined(request.Kind.Value))
            attachment.Kind = request.Kind.Value;
        attachment.Subtype = Trim(request.Subtype, 80);
        attachment.Notes = Trim(request.Notes, 1000);
        attachment.ToothNumber = NormalizeTooth(request.ToothNumber);
        attachment.CapturedOn = NormalizeDate(request.CapturedOn);

        await _db.SaveChangesAsync(ct);
        return Ok(new { succeeded = true, data = Map(attachment) });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int patientId, int id, CancellationToken ct)
    {
        if (!CanEdit())
            return Forbid();

        var deleteScopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (deleteScopeDenied != null)
            return deleteScopeDenied;

        var attachment = await _db.PatientClinicalAttachments
            .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId, ct);
        if (attachment == null)
            return NotFound(new { succeeded = false, message = "Adjunto no encontrado." });

        attachment.SoftDelete();
        _storage.Delete(patientId, attachment.StoredFileName);

        var linked = await _db.DentalClinicalEvents
            .Where(e => e.PatientId == patientId
                && e.ReferenceType == nameof(PatientClinicalAttachment)
                && e.ReferenceId == id.ToString())
            .ToListAsync(ct);
        foreach (var item in linked)
            item.Void(_currentUser.UserId);

        await _db.SaveChangesAsync(ct);
        return Ok(new { succeeded = true });
    }

    private async Task<IActionResult?> DenyIfOutOfScope(int patientId, CancellationToken ct)
    {
        if (await _clinicScope.AllowsPatientAsync(patientId, ct))
            return null;

        return StatusCode(StatusCodes.Status403Forbidden, new
        {
            succeeded = false,
            message = "Solo puede consultar pacientes que haya atendido."
        });
    }

    private bool CanView() =>
        _currentUser.IsAdmin
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("pacientes", AdminPermissionCatalog.Actions.View))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.View))
        || IsDoctor();

    private bool CanEdit() =>
        _currentUser.IsAdmin
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("pacientes", AdminPermissionCatalog.Actions.Edit))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Create))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Edit))
        || IsDoctor();

    private bool IsDoctor() =>
        User.IsInRole(AdminPermissionCatalog.SystemRoles.Doctor)
        || AdminRoleAuthorization.GetRoleValues(User)
            .Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Doctor, StringComparison.OrdinalIgnoreCase));

    private static object Map(PatientClinicalAttachment a) => new
    {
        a.Id,
        a.PatientId,
        a.Kind,
        a.Title,
        a.Subtype,
        a.Notes,
        a.ToothNumber,
        a.CapturedOn,
        a.OriginalFileName,
        a.ContentType,
        a.FileSizeBytes,
        a.CreatedAt,
        DateDisplay = FormatDate(a.CapturedOn ?? a.CreatedAt),
        a.RecordedByUserId,
        IsImage = a.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase),
        IsPdf = string.Equals(a.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
    };

    private static ClinicalAttachmentKind InferKind(string contentType) =>
        contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            ? ClinicalAttachmentKind.Radiograph
            : ClinicalAttachmentKind.ClinicalDocument;

    private static string DefaultTitle(ClinicalAttachmentKind kind, string? subtype, string originalName)
    {
        if (!string.IsNullOrWhiteSpace(subtype))
            return Clamp(subtype.Trim(), 200);
        return kind switch
        {
            ClinicalAttachmentKind.Radiograph => "Radiografía",
            _ => Clamp(Path.GetFileNameWithoutExtension(originalName), 200)
        };
    }

    private static string TimelineTitle(ClinicalAttachmentKind kind) => kind switch
    {
        ClinicalAttachmentKind.Radiograph => "Radiografía adjunta",
        ClinicalAttachmentKind.ClinicalDocument => "Documento clínico adjunto",
        _ => "Archivo clínico adjunto"
    };

    private static string TimelineDescription(PatientClinicalAttachment attachment)
    {
        var parts = new List<string> { attachment.Title };
        if (!string.IsNullOrWhiteSpace(attachment.Subtype))
            parts.Add(attachment.Subtype);
        parts.Add(attachment.OriginalFileName);
        return string.Join(" · ", parts);
    }

    private static string Clamp(string value, int max) =>
        value.Length <= max ? value : value[..max];

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return Clamp(value.Trim(), max);
    }

    private static int? NormalizeTooth(int? tooth) =>
        tooth is >= 11 and <= 85 ? tooth : null;

    private static DateTime? NormalizeDate(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        var date = value.Value.Kind == DateTimeKind.Utc
            ? value.Value.ToLocalTime().Date
            : value.Value.Date;
        return DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
    }

    private static string FormatDate(DateTime value) =>
        (value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value).ToString("dd/MM/yyyy");
}

public sealed class UploadClinicalAttachmentRequest
{
    public IFormFile? File { get; set; }
    public ClinicalAttachmentKind Kind { get; set; } = ClinicalAttachmentKind.Radiograph;
    public string? Title { get; set; }
    public string? Subtype { get; set; }
    public string? Notes { get; set; }
    public int? ToothNumber { get; set; }
    public DateTime? CapturedOn { get; set; }
}

public sealed class UpdateClinicalAttachmentRequest
{
    public ClinicalAttachmentKind? Kind { get; set; }
    public string? Title { get; set; }
    public string? Subtype { get; set; }
    public string? Notes { get; set; }
    public int? ToothNumber { get; set; }
    public DateTime? CapturedOn { get; set; }
}
