namespace Clinic_System.API.Controllers;

[Route("api/dental/certificates")]
[ApiController]
[Authorize]
public class PatientMedicalCertificateController : AppControllerBase
{
    public PatientMedicalCertificateController(IMediator mediator) : base(mediator) { }

    [HttpGet("options")]
    public async Task<IActionResult> Options()
    {
        var response = await mediator.Send(new ListMedicalCertificateOptionsQuery());
        return NewResult(response);
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<IActionResult> List(int patientId)
    {
        var response = await mediator.Send(new ListPatientMedicalCertificatesQuery { PatientId = patientId });
        return NewResult(response);
    }

    [HttpGet("{certificateId:int}")]
    public async Task<IActionResult> Get(int certificateId)
    {
        var response = await mediator.Send(new GetPatientMedicalCertificateQuery { CertificateId = certificateId });
        return NewResult(response);
    }

    [HttpPost("patient/{patientId:int}")]
    [Authorize(Policy = "certificados.create+doctor")]
    public async Task<IActionResult> Create(int patientId, [FromBody] CreatePatientMedicalCertificateCommand? command)
    {
        command ??= new CreatePatientMedicalCertificateCommand();
        command.PatientId = patientId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }

    [HttpPut("{certificateId:int}")]
    [Authorize(Policy = "certificados.edit+doctor")]
    public async Task<IActionResult> Update(
        int certificateId,
        [FromBody] UpdatePatientMedicalCertificateCommand command)
    {
        command.CertificateId = certificateId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }

    [HttpDelete("{certificateId:int}")]
    [Authorize(Policy = "certificados.edit+doctor")]
    public async Task<IActionResult> Delete(int certificateId)
    {
        var response = await mediator.Send(new DeletePatientMedicalCertificateCommand { CertificateId = certificateId });
        return NewResult(response);
    }
}
