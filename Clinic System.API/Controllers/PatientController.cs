namespace Clinic_System.API.Controllers

{
[Route("api/patients")]

    [ApiController]

    public class PatientController : AppControllerBase

    {

        public PatientController(IMediator mediator) : base(mediator)

        {

        }
[HttpGet]

        public async Task<IActionResult> GetPatientList([FromQuery] bool includeInactive = true)

        {

            var response = await mediator.Send(new GetPatientListQuery { IncludeInactive = includeInactive });

            return Ok(response);

        }
[HttpGet("paging")]

        public async Task<IActionResult> GetPatientListPaging([FromQuery] GetPatientListPagingQuery query)

        {

            var response = await mediator.Send(query);

            return Ok(response);

        }
[HttpGet("{id:int}/clinical-profile")]

        public async Task<IActionResult> GetClinicalProfile(int id)

        {

            var response = await mediator.Send(new GetPatientClinicalProfileQuery { PatientId = id });

            return NewResult(response);

        }
[HttpGet("{id:int}")]

        public async Task<IActionResult> GetPatientById(int id)

        {

            var response = await mediator.Send(new GetPatientByIdQuery { Id = id });

            return NewResult(response);

        }
[HttpGet("phone/{phone}")]

        public async Task<IActionResult> GetPatientByPhone(string phone)

        {

            var response = await mediator.Send(new GetPatientByPhoneQuery { Phone = phone });

            return NewResult(response);

        }
[HttpGet("name/{name}")]

        public async Task<IActionResult> GetPatientListByName(string name)

        {

            var response = await mediator.Send(new GetPatientListByNameQuery { FullName = name });

            return NewResult(response);

        }
[HttpGet("{id:int}/appointments")]

        public async Task<IActionResult> GetPatientWithAppointmentsById(int id)

        {

            var response = await mediator.Send(new GetPatientWithAppointmentsByIdQuery { Id = id });

            return NewResult(response);

        }



        [AllowAnonymous]

        [HttpPost("create")]

        [EnableRateLimiting("AuthLimiter")]

        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPost]
        [Authorize(Policy = "pacientes.create")]
        public async Task<IActionResult> CreatePatientAdmin([FromBody] CreatePatientCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPost("import/preview")]
        [Authorize(Policy = "pacientes.create")]
        [RequestSizeLimit(PatientCsvImport.MaxCsvBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = PatientCsvImport.MaxCsvBytes)]
        public async Task<IActionResult> PreviewPatientImport(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return NewResult(new Clinic_System.Application.Common.Bases.Response<PatientImportPreviewDTO>
                {
                    Succeeded = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Suba un archivo CSV."
                });

            if (file.Length > PatientCsvImport.MaxCsvBytes)
                return NewResult(new Clinic_System.Application.Common.Bases.Response<PatientImportPreviewDTO>
                {
                    Succeeded = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "El archivo no puede superar 2 MB."
                });

            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var csv = await reader.ReadToEndAsync(cancellationToken);

            var response = await mediator.Send(new PreviewPatientImportCommand { CsvContent = csv }, cancellationToken);
            return NewResult(response);
        }

        [HttpGet("import/template")]
        [Authorize(Policy = "pacientes.create")]
        public IActionResult DownloadPatientImportTemplate()
        {
            var csv = PatientCsvImport.BuildTemplateCsv();
            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(csv))
                .ToArray();
            return File(bytes, "text/csv", "plantilla-pacientes.csv");
        }

        [HttpPost("import")]
        [Authorize(Policy = "pacientes.create")]
        public async Task<IActionResult> ConfirmPatientImport(
            [FromBody] ConfirmPatientImportCommand command,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(command, cancellationToken);
            return NewResult(response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientCommand command)
        {
            if (id != command.Id)
                return BadRequest("Mismatched Patient ID");

            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDeletePatient(int id)

        {

            var response = await mediator.Send(new SoftDeletePatientCommand { Id = id });

            return NewResult(response);

        }
[HttpPut("{id:int}/enable")]

        public async Task<IActionResult> RestorePatient(int id)

        {

            var response = await mediator.Send(new RestorePatientCommand { Id = id });

            return NewResult(response);

        }
[HttpDelete("{id:int}/hard")]

        public async Task<IActionResult> HardDeletePatient(int id)

        {

            var response = await mediator.Send(new HardDeletePatientCommand { Id = id });

            return NewResult(response);

        }

    }

}


