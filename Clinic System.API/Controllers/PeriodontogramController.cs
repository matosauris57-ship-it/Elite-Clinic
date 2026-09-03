namespace Clinic_System.API.Controllers;

[Route("api/dental/periodontogram")]
[ApiController]
[Authorize]
public class PeriodontogramController : AppControllerBase
{
    public PeriodontogramController(IMediator mediator) : base(mediator) { }

    [HttpGet("patient/{patientId:int}/exams")]
    [Authorize(Policy = "periodontograma.view+doctor+patient")]
    public async Task<IActionResult> List(int patientId)
    {
        var response = await mediator.Send(new ListPeriodontalExamsQuery { PatientId = patientId });
        return NewResult(response);
    }

    [HttpGet("exams/{examId:int}")]
    [Authorize(Policy = "periodontograma.view+doctor+patient")]
    public async Task<IActionResult> Get(int examId)
    {
        var response = await mediator.Send(new GetPeriodontalExamQuery { ExamId = examId });
        return NewResult(response);
    }

    [HttpGet("compare")]
    [Authorize(Policy = "periodontograma.view+doctor+patient")]
    public async Task<IActionResult> Compare([FromQuery] int previousExamId, [FromQuery] int currentExamId)
    {
        var response = await mediator.Send(new ComparePeriodontalExamsQuery
        {
            PreviousExamId = previousExamId,
            CurrentExamId = currentExamId
        });
        return NewResult(response);
    }

    [HttpPost("patient/{patientId:int}/exams")]
    [Authorize(Policy = "periodontograma.edit+doctor")]
    public async Task<IActionResult> Create(int patientId, [FromBody] CreatePeriodontalExamCommand? command)
    {
        command ??= new CreatePeriodontalExamCommand();
        command.PatientId = patientId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }

    [HttpPut("exams/{examId:int}")]
    [Authorize(Policy = "periodontograma.edit+doctor")]
    public async Task<IActionResult> Save(int examId, [FromBody] SavePeriodontalExamCommand command)
    {
        command.ExamId = examId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }

    [HttpDelete("exams/{examId:int}")]
    [Authorize(Policy = "periodontograma.edit+doctor")]
    public async Task<IActionResult> Delete(int examId)
    {
        var response = await mediator.Send(new DeletePeriodontalExamCommand { ExamId = examId });
        return NewResult(response);
    }
}
