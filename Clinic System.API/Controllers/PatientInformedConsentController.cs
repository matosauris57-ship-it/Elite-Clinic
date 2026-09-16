using Clinic_System.Core.Authorization;
using Clinic_System.Core.Catalog;
using Clinic_System.Core.Entities;
using Clinic_System.Infrastructure.Authorization;

namespace Clinic_System.API.Controllers;

[Route("api/patients/{patientId:int}/informed-consents")]
[ApiController]
[Authorize]
public class PatientInformedConsentController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IClinicalAttachmentStorage _storage;
    private readonly ICurrentUserService _currentUser;
    private readonly IClinicDataScopeService _clinicScope;

    public PatientInformedConsentController(
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

    [HttpGet("/api/informed-consents/templates")]
    public IActionResult Templates()
    {
        if (!CanView())
            return Forbid();

        var data = InformedConsentCatalog.Templates.Select(t => new
        {
            Type = t.Type,
            t.Title,
            t.Purpose,
            BodyText = InformedConsentCatalog.NormalizeBody(t.BodyText)
        });

        return Ok(new { succeeded = true, data });
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

        var items = await _db.PatientInformedConsents
            .AsNoTracking()
            .Include(c => c.Doctor)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.SignedOn)
            .ThenByDescending(c => c.Id)
            .ToListAsync(ct);

        return Ok(new { succeeded = true, data = items.Select(Map) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int patientId, int id, CancellationToken ct)
    {
        if (!CanView())
            return Forbid();

        var scopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (scopeDenied != null)
            return scopeDenied;

        var consent = await _db.PatientInformedConsents
            .AsNoTracking()
            .Include(c => c.Doctor)
            .FirstOrDefaultAsync(c => c.Id == id && c.PatientId == patientId, ct);
        if (consent == null)
            return NotFound(new { succeeded = false, message = "Consentimiento no encontrado." });

        return Ok(new { succeeded = true, data = Map(consent) });
    }

    [HttpGet("{id:int}/file")]
    public async Task<IActionResult> Download(int patientId, int id, CancellationToken ct)
    {
        if (!CanView())
            return Forbid();

        var scopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (scopeDenied != null)
            return scopeDenied;

        var consent = await _db.PatientInformedConsents
            .FirstOrDefaultAsync(c => c.Id == id && c.PatientId == patientId, ct);
        if (consent == null)
            return NotFound(new { succeeded = false, message = "Consentimiento no encontrado." });

        try
        {
            var stream = _storage.OpenRead(patientId, consent.StoredFileName);
            var inline = consent.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(consent.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
            if (inline)
                return File(stream, consent.ContentType);

            return File(stream, consent.ContentType, consent.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { succeeded = false, message = "El archivo firmado ya no está en el servidor." });
        }
    }

    [HttpPost]
    [RequestSizeLimit(FileClinicalAttachmentStorage.DefaultMaxFileBytes + (1024 * 1024))]
    [RequestFormLimits(MultipartBodyLengthLimit = FileClinicalAttachmentStorage.DefaultMaxFileBytes + (1024 * 1024))]
    public async Task<IActionResult> Upload(int patientId, [FromForm] UploadInformedConsentRequest request, CancellationToken ct)
    {
        if (!CanCreate())
            return Forbid();

        var scopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (scopeDenied != null)
            return scopeDenied;

        if (request.File == null || request.File.Length <= 0)
            return BadRequest(new { succeeded = false, message = "Suba el consentimiento firmado (escaneo o foto)." });

        if (!_storage.IsAllowed(request.File.FileName, request.File.ContentType))
            return BadRequest(new { succeeded = false, message = "Formato no permitido. Use JPG, PNG, WEBP, GIF, PDF o Word." });

        if (request.File.Length > _storage.MaxFileBytes)
            return BadRequest(new { succeeded = false, message = "El archivo no puede superar 20 MB." });

        if (!Enum.IsDefined(request.ConsentType))
            return BadRequest(new { succeeded = false, message = "Tipo de consentimiento no válido." });

        var patientExists = await _db.Patients.AnyAsync(p => p.Id == patientId, ct);
        if (!patientExists)
            return NotFound(new { succeeded = false, message = "Paciente no encontrado." });

        if (request.DoctorId is not > 0)
            return BadRequest(new { succeeded = false, message = "Debe seleccionar el odontólogo." });

        var doctorExists = await _db.Doctors.AnyAsync(d => d.Id == request.DoctorId.Value, ct);
        if (!doctorExists)
            return BadRequest(new { succeeded = false, message = "Médico no encontrado." });

        var template = InformedConsentCatalog.GetOrDefault(request.ConsentType);
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

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? template.Title
            : Clamp(request.Title.Trim(), 200);

        var bodyText = InformedConsentCatalog.NormalizeBody(
            !string.IsNullOrWhiteSpace(request.BodyText)
                ? request.BodyText
                : !string.IsNullOrWhiteSpace(request.ProcedureExplanation)
                    ? request.ProcedureExplanation
                    : template.BodyText);

        if (string.IsNullOrWhiteSpace(bodyText))
            return BadRequest(new { succeeded = false, message = "El texto del consentimiento no puede estar vacío." });

        var consent = new PatientInformedConsent
        {
            PatientId = patientId,
            ConsentType = request.ConsentType,
            Title = title,
            ProcedureExplanation = Clamp(bodyText, 8000),
            Benefits = "",
            Risks = "",
            Alternatives = "",
            AuthorizationText = "",
            Notes = Trim(request.Notes, 1000),
            ToothNumber = NormalizeTooth(request.ToothNumber),
            DoctorId = request.DoctorId,
            SignedOn = NormalizeDate(request.SignedOn) ?? DateTime.Today,
            OriginalFileName = stored.OriginalFileName,
            StoredFileName = stored.StoredFileName,
            ContentType = stored.ContentType,
            FileSizeBytes = stored.FileSizeBytes,
            RecordedByUserId = _currentUser.UserId
        };

        var clinicalEvent = new DentalClinicalEvent
        {
            PatientId = patientId,
            ToothNumber = consent.ToothNumber,
            Type = DentalClinicalEventType.InformedConsent,
            Title = "Consentimiento informado",
            Description = $"{consent.Title} · firmado {FormatDate(consent.SignedOn)}",
            ReferenceType = nameof(PatientInformedConsent),
            RecordedByUserId = _currentUser.UserId,
            RecordedAt = consent.SignedOn
        };

        _db.PatientInformedConsents.Add(consent);
        _db.DentalClinicalEvents.Add(clinicalEvent);
        await _db.SaveChangesAsync(ct);

        clinicalEvent.ReferenceId = consent.Id.ToString();
        await _db.SaveChangesAsync(ct);

        await _db.Entry(consent).Reference(c => c.Doctor).LoadAsync(ct);
        return Ok(new { succeeded = true, data = Map(consent) });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int patientId, int id, [FromBody] UpdateInformedConsentRequest request, CancellationToken ct)
    {
        if (!CanEdit())
            return Forbid();

        var scopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (scopeDenied != null)
            return scopeDenied;

        var consent = await _db.PatientInformedConsents
            .FirstOrDefaultAsync(c => c.Id == id && c.PatientId == patientId, ct);
        if (consent == null)
            return NotFound(new { succeeded = false, message = "Consentimiento no encontrado." });

        if (!string.IsNullOrWhiteSpace(request.Title))
            consent.Title = Clamp(request.Title.Trim(), 200);
        if (!string.IsNullOrWhiteSpace(request.Notes) || request.ClearNotes)
            consent.Notes = Trim(request.Notes, 1000);
        consent.ToothNumber = NormalizeTooth(request.ToothNumber);
        if (request.SignedOn.HasValue)
            consent.SignedOn = NormalizeDate(request.SignedOn) ?? consent.SignedOn;
        if (request.DoctorId.HasValue)
            consent.DoctorId = request.DoctorId > 0 ? request.DoctorId : null;

        await _db.SaveChangesAsync(ct);
        await _db.Entry(consent).Reference(c => c.Doctor).LoadAsync(ct);
        return Ok(new { succeeded = true, data = Map(consent) });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int patientId, int id, CancellationToken ct)
    {
        if (!CanDelete())
            return Forbid();

        var scopeDenied = await DenyIfOutOfScope(patientId, ct);
        if (scopeDenied != null)
            return scopeDenied;

        var consent = await _db.PatientInformedConsents
            .FirstOrDefaultAsync(c => c.Id == id && c.PatientId == patientId, ct);
        if (consent == null)
            return NotFound(new { succeeded = false, message = "Consentimiento no encontrado." });

        consent.SoftDelete();
        _storage.Delete(patientId, consent.StoredFileName);

        var linked = await _db.DentalClinicalEvents
            .Where(e => e.PatientId == patientId
                && e.ReferenceType == nameof(PatientInformedConsent)
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
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("consentimientos", AdminPermissionCatalog.Actions.View))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("pacientes", AdminPermissionCatalog.Actions.View))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.View))
        || IsDoctor();

    private bool CanCreate() =>
        _currentUser.IsAdmin
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("consentimientos", AdminPermissionCatalog.Actions.Create))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("pacientes", AdminPermissionCatalog.Actions.Edit))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Create))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Edit))
        || IsDoctor();

    private bool CanEdit() =>
        _currentUser.IsAdmin
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("consentimientos", AdminPermissionCatalog.Actions.Edit))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("pacientes", AdminPermissionCatalog.Actions.Edit))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Edit))
        || IsDoctor();

    private bool CanDelete() =>
        _currentUser.IsAdmin
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("consentimientos", AdminPermissionCatalog.Actions.Delete))
        || _currentUser.HasPermission(AdminPermissionCatalog.Build("pacientes", AdminPermissionCatalog.Actions.Edit))
        || IsDoctor();

    private bool IsDoctor() =>
        User.IsInRole(AdminPermissionCatalog.SystemRoles.Doctor)
        || AdminRoleAuthorization.GetRoleValues(User)
            .Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Doctor, StringComparison.OrdinalIgnoreCase));

    private static object Map(PatientInformedConsent c)
    {
        var body = ResolveBody(c);
        return new
        {
            c.Id,
            c.PatientId,
            c.ConsentType,
            ConsentTypeLabel = InformedConsentCatalog.GetOrDefault(c.ConsentType).Title,
            c.Title,
            BodyText = body,
            ProcedureExplanation = body,
            Benefits = c.Benefits,
            Risks = c.Risks,
            Alternatives = c.Alternatives,
            AuthorizationText = c.AuthorizationText,
            c.Notes,
            c.ToothNumber,
            c.DoctorId,
            DoctorName = c.Doctor?.FullName,
            DoctorSignatureImageUrl = c.Doctor?.SignatureImageUrl,
            c.SignedOn,
            DateDisplay = FormatDate(c.SignedOn),
            c.OriginalFileName,
            c.ContentType,
            c.FileSizeBytes,
            c.CreatedAt,
            c.RecordedByUserId,
            IsImage = c.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase),
            IsPdf = string.Equals(c.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string ResolveBody(PatientInformedConsent c)
    {
        if (!string.IsNullOrWhiteSpace(c.ProcedureExplanation)
            && string.IsNullOrWhiteSpace(c.Benefits)
            && string.IsNullOrWhiteSpace(c.Risks)
            && string.IsNullOrWhiteSpace(c.Alternatives)
            && string.IsNullOrWhiteSpace(c.AuthorizationText))
            return InformedConsentCatalog.NormalizeBody(c.ProcedureExplanation);

        var parts = new[]
        {
            c.ProcedureExplanation,
            c.Benefits,
            c.Risks,
            c.Alternatives,
            c.AuthorizationText
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        return InformedConsentCatalog.NormalizeBody(string.Join("\n\n", parts));
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

public sealed class UploadInformedConsentRequest
{
    public IFormFile? File { get; set; }
    public InformedConsentType ConsentType { get; set; } = InformedConsentType.EvaluationDiagnosis;
    public string? Title { get; set; }
    public string? BodyText { get; set; }
    public string? ProcedureExplanation { get; set; }
    public string? Notes { get; set; }
    public int? ToothNumber { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? SignedOn { get; set; }
}

public sealed class UpdateInformedConsentRequest
{
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public bool ClearNotes { get; set; }
    public int? ToothNumber { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? SignedOn { get; set; }
}
