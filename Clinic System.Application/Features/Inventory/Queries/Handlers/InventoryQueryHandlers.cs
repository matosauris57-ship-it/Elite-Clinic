using Clinic_System.Application.DTOs.Inventory;
using Clinic_System.Application.Features.Inventory.Queries.Models;

namespace Clinic_System.Application.Features.Inventory.Queries.Handlers
{
    public class GetInventoryItemListQueryHandler : AppRequestHandler<GetInventoryItemListQuery, List<InventoryItemDTO>>
    {
        private readonly IInventoryService inventoryService;

        public GetInventoryItemListQueryHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
        }

        public override async Task<Response<List<InventoryItemDTO>>> Handle(
            GetInventoryItemListQuery request, CancellationToken cancellationToken)
        {
            var items = await inventoryService.GetItemsAsync(request.ActiveOnly, request.LowStockOnly, cancellationToken);
            return Success(inventoryService.ToItemDtos(items));
        }
    }

    public class GetInventoryItemByIdQueryHandler : AppRequestHandler<GetInventoryItemByIdQuery, InventoryItemDTO>
    {
        private readonly IInventoryService inventoryService;

        public GetInventoryItemByIdQueryHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
        }

        public override async Task<Response<InventoryItemDTO>> Handle(
            GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await inventoryService.GetItemByIdAsync(request.Id, cancellationToken);
            return Success(inventoryService.ToItemDto(item));
        }
    }

    public class GetStockMovementsQueryHandler : AppRequestHandler<GetStockMovementsQuery, List<StockMovementDTO>>
    {
        private readonly IInventoryService inventoryService;

        public GetStockMovementsQueryHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
        }

        public override async Task<Response<List<StockMovementDTO>>> Handle(
            GetStockMovementsQuery request, CancellationToken cancellationToken)
        {
            var item = await inventoryService.GetItemByIdAsync(request.InventoryItemId, cancellationToken);
            var movements = await inventoryService.GetMovementsAsync(request.InventoryItemId, request.Take, cancellationToken);
            return Success(movements.Select(m => inventoryService.ToMovementDto(m, item.Name)).ToList());
        }
    }

    public class GetProcedureBomQueryHandler : AppRequestHandler<GetProcedureBomQuery, List<ProcedureMaterialDTO>>
    {
        private readonly IInventoryService inventoryService;

        public GetProcedureBomQueryHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
        }

        public override async Task<Response<List<ProcedureMaterialDTO>>> Handle(
            GetProcedureBomQuery request, CancellationToken cancellationToken)
        {
            var bom = await inventoryService.GetProcedureBomAsync(request.TreatmentProcedureId, cancellationToken);
            return Success(bom.Select(inventoryService.ToBomDto).ToList());
        }
    }

    public class ProposeMaterialConsumptionQueryHandler
        : AppRequestHandler<ProposeMaterialConsumptionQuery, MaterialConsumptionProposalDTO>
    {
        private readonly IInventoryService inventoryService;

        public ProposeMaterialConsumptionQueryHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
        }

        public override async Task<Response<MaterialConsumptionProposalDTO>> Handle(
            ProposeMaterialConsumptionQuery request, CancellationToken cancellationToken)
        {
            var proposal = await inventoryService.ProposeConsumptionAsync(request.DentalTreatmentId, cancellationToken);
            return Success(proposal);
        }
    }

    public class GetMaterialConsumptionQueryHandler
        : AppRequestHandler<GetMaterialConsumptionQuery, TreatmentMaterialConsumptionDTO>
    {
        private readonly IInventoryService inventoryService;

        public GetMaterialConsumptionQueryHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
        }

        public override async Task<Response<TreatmentMaterialConsumptionDTO>> Handle(
            GetMaterialConsumptionQuery request, CancellationToken cancellationToken)
        {
            var consumption = await inventoryService.GetConsumptionByTreatmentAsync(request.DentalTreatmentId, cancellationToken);
            if (consumption == null)
                return NotFound<TreatmentMaterialConsumptionDTO>("No hay consumo registrado para este tratamiento.");
            return Success(inventoryService.ToConsumptionDto(consumption));
        }
    }

    public class GetLowStockQueryHandler : AppRequestHandler<GetLowStockQuery, LowStockAlertDTO>
    {
        private readonly IInventoryService inventoryService;

        public GetLowStockQueryHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
        }

        public override async Task<Response<LowStockAlertDTO>> Handle(
            GetLowStockQuery request, CancellationToken cancellationToken)
        {
            return Success(await inventoryService.GetLowStockAsync(cancellationToken));
        }
    }
}
