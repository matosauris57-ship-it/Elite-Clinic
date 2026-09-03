using Clinic_System.Core.Entities;

namespace Clinic_System.API.Controllers
{
    [Route("api/patients/{patientId:int}/emergency-contacts")]
    [ApiController]
    [Authorize]
    public class EmergencyContactController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmergencyContactController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int patientId, CancellationToken ct)
        {
            var contacts = await _db.EmergencyContacts
                .Where(c => c.PatientId == patientId)
                .OrderBy(c => c.Id)
                .Select(c => new
                {
                    c.Id,
                    c.PatientId,
                    c.FullName,
                    c.Phone,
                    c.Relationship,
                    c.Notes
                })
                .ToListAsync(ct);

            return Ok(new { succeeded = true, data = contacts });
        }

        [HttpPost]
        public async Task<IActionResult> Create(int patientId, [FromBody] EmergencyContactRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Relationship))
                return BadRequest(new { succeeded = false, message = "Nombre, teléfono y parentesco son obligatorios." });

            var patient = await _db.Patients.AnyAsync(p => p.Id == patientId, ct);
            if (!patient)
                return NotFound(new { succeeded = false, message = "Paciente no encontrado." });

            var contact = new EmergencyContact
            {
                PatientId = patientId,
                FullName = request.FullName.Trim(),
                Phone = request.Phone.Trim(),
                Relationship = request.Relationship.Trim(),
                Notes = request.Notes?.Trim()
            };

            _db.EmergencyContacts.Add(contact);
            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                succeeded = true,
                data = new { contact.Id, contact.PatientId, contact.FullName, contact.Phone, contact.Relationship, contact.Notes }
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int patientId, int id, [FromBody] EmergencyContactRequest request, CancellationToken ct)
        {
            var contact = await _db.EmergencyContacts.FirstOrDefaultAsync(c => c.Id == id && c.PatientId == patientId, ct);
            if (contact == null)
                return NotFound(new { succeeded = false, message = "Contacto no encontrado." });

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Relationship))
                return BadRequest(new { succeeded = false, message = "Nombre, teléfono y parentesco son obligatorios." });

            contact.FullName = request.FullName.Trim();
            contact.Phone = request.Phone.Trim();
            contact.Relationship = request.Relationship.Trim();
            contact.Notes = request.Notes?.Trim();

            await _db.SaveChangesAsync(ct);

            return Ok(new { succeeded = true, message = "Contacto actualizado." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int patientId, int id, CancellationToken ct)
        {
            var contact = await _db.EmergencyContacts.FirstOrDefaultAsync(c => c.Id == id && c.PatientId == patientId, ct);
            if (contact == null)
                return NotFound(new { succeeded = false, message = "Contacto no encontrado." });

            _db.EmergencyContacts.Remove(contact);
            await _db.SaveChangesAsync(ct);

            return Ok(new { succeeded = true, message = "Contacto eliminado." });
        }
    }

    public class EmergencyContactRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
