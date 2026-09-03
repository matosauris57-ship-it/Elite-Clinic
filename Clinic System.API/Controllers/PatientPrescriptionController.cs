namespace Clinic_System.API.Controllers;

[Route("api/dental/prescriptions")]
[ApiController]
[Authorize]
public class PatientPrescriptionController : AppControllerBase
{
    public PatientPrescriptionController(IMediator mediator) : base(mediator) { }

    [HttpGet("templates")]
    public async Task<IActionResult> Templates()
    {
        var response = await mediator.Send(new ListPrescriptionTemplatesQuery());
        return NewResult(response);
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<IActionResult> List(int patientId)
    {
        var response = await mediator.Send(new ListPatientPrescriptionsQuery { PatientId = patientId });
        return NewResult(response);
    }

    [HttpGet("{prescriptionId:int}")]
    public async Task<IActionResult> Get(int prescriptionId)
    {
        var response = await mediator.Send(new GetPatientPrescriptionQuery { PrescriptionId = prescriptionId });
        return NewResult(response);
    }

    [HttpPost("patient/{patientId:int}")]
    [Authorize(Policy = "recetas.edit+doctor")]
    public async Task<IActionResult> Create(int patientId, [FromBody] CreatePatientPrescriptionCommand? command)
    {
        command ??= new CreatePatientPrescriptionCommand();
        command.PatientId = patientId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }

    [HttpPut("{prescriptionId:int}")]
    [Authorize(Policy = "recetas.edit+doctor")]
    public async Task<IActionResult> Update(int prescriptionId, [FromBody] UpdatePatientPrescriptionCommand command)
    {
        command.PrescriptionId = prescriptionId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }

    [HttpDelete("{prescriptionId:int}")]
    [Authorize(Policy = "recetas.edit+doctor")]
    public async Task<IActionResult> Delete(int prescriptionId)
    {
        var response = await mediator.Send(new DeletePatientPrescriptionCommand { PrescriptionId = prescriptionId });
        return NewResult(response);
    }
}
