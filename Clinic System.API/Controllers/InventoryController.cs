namespace Clinic_System.API.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    [Authorize]
    public class InventoryController : AppControllerBase
    {
        public InventoryController(IMediator mediator) : base(mediator) { }

        [HttpGet("items")]
        [Authorize(Policy = "inventario.view")]
        public async Task<IActionResult> GetItems([FromQuery] bool activeOnly = false, [FromQuery] bool lowStockOnly = false)
        {
            var response = await mediator.Send(new GetInventoryItemListQuery
            {
                ActiveOnly = activeOnly,
                LowStockOnly = lowStockOnly
            });
            return NewResult(response);
        }

        [HttpGet("items/{id:int}")]
        [Authorize(Policy = "inventario.view")]
        public async Task<IActionResult> GetItem(int id)
        {
            var response = await mediator.Send(new GetInventoryItemByIdQuery { Id = id });
            return NewResult(response);
        }

        [HttpPost("items")]
        [Authorize(Policy = "inventario.edit")]
        public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPut("items/{id:int}")]
        [Authorize(Policy = "inventario.edit")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateInventoryItemCommand command)
        {
            command.Id = id;
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpDelete("items/{id:int}")]
        [Authorize(Policy = "inventario.edit")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var response = await mediator.Send(new SoftDeleteInventoryItemCommand { Id = id });
            return NewResult(response);
        }

        [HttpPost("items/{id:int}/entries")]
        [Authorize(Policy = "inventario.edit")]
        public async Task<IActionResult> RegisterEntry(int id, [FromBody] RegisterStockEntryCommand command)
        {
            command.InventoryItemId = id;
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPost("items/{id:int}/adjustments")]
        [Authorize(Policy = "inventario.edit")]
        public async Task<IActionResult> RegisterAdjustment(int id, [FromBody] RegisterStockAdjustmentCommand command)
        {
            command.InventoryItemId = id;
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpGet("items/{id:int}/movements")]
        [Authorize(Policy = "inventario.view")]
        public async Task<IActionResult> GetMovements(int id, [FromQuery] int take = 50)
        {
            var response = await mediator.Send(new GetStockMovementsQuery { InventoryItemId = id, Take = take });
            return NewResult(response);
        }

        [HttpGet("low-stock")]
        [Authorize(Policy = "inventario.view")]
        public async Task<IActionResult> GetLowStock()
        {
            var response = await mediator.Send(new GetLowStockQuery());
            return NewResult(response);
        }

        [HttpGet("procedures/{procedureId:int}/bom")]
        [Authorize(Policy = "inventario.view")]
        public async Task<IActionResult> GetProcedureBom(int procedureId)
        {
            var response = await mediator.Send(new GetProcedureBomQuery { TreatmentProcedureId = procedureId });
            return NewResult(response);
        }

        [HttpPut("procedures/{procedureId:int}/bom")]
        [Authorize(Policy = "inventario.edit")]
        public async Task<IActionResult> ReplaceProcedureBom(int procedureId, [FromBody] ReplaceProcedureBomCommand command)
        {
            command.TreatmentProcedureId = procedureId;
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpGet("consumptions/propose/{treatmentId:int}")]
        [Authorize(Policy = "tratamientos.view+doctor")]
        public async Task<IActionResult> ProposeConsumption(int treatmentId)
        {
            var response = await mediator.Send(new ProposeMaterialConsumptionQuery { DentalTreatmentId = treatmentId });
            return NewResult(response);
        }

        [HttpGet("consumptions/by-treatment/{treatmentId:int}")]
        [Authorize(Policy = "tratamientos.view+doctor")]
        public async Task<IActionResult> GetConsumption(int treatmentId)
        {
            var response = await mediator.Send(new GetMaterialConsumptionQuery { DentalTreatmentId = treatmentId });
            return NewResult(response);
        }

        [HttpPost("consumptions/{treatmentId:int}")]
        [Authorize(Policy = "tratamientos.edit+doctor")]
        public async Task<IActionResult> ConfirmConsumption(int treatmentId, [FromBody] ConfirmMaterialConsumptionCommand command)
        {
            command.DentalTreatmentId = treatmentId;
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPut("consumptions/{treatmentId:int}")]
        [Authorize(Policy = "tratamientos.edit+doctor")]
        public async Task<IActionResult> ReplaceConsumption(int treatmentId, [FromBody] ReplaceMaterialConsumptionCommand command)
        {
            command.DentalTreatmentId = treatmentId;
            var response = await mediator.Send(command);
            return NewResult(response);
        }
    }
}
