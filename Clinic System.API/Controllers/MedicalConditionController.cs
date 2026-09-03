namespace Clinic_System.API.Controllers
{
[Route("api/medical-conditions")]
    [ApiController]
    public class MedicalConditionController : AppControllerBase
    {
        public MedicalConditionController(IMediator mediator) : base(mediator) { }
[HttpGet]
        public async Task<IActionResult> GetList([FromQuery] bool activeOnly = false)
        {
            var response = await mediator.Send(new GetMedicalConditionListQuery { ActiveOnly = activeOnly });
            return NewResult(response);
        }
[HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await mediator.Send(new GetMedicalConditionByIdQuery { Id = id });
            return NewResult(response);
        }
[HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicalConditionCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalConditionCommand command)
        {
            command.Id = id;
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await mediator.Send(new SoftDeleteMedicalConditionCommand { Id = id });
            return NewResult(response);
        }
    }
}
