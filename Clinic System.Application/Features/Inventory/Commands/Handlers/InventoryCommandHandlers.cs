using Clinic_System.Application.DTOs.Inventory;
using Clinic_System.Application.Features.Inventory.Commands.Models;

namespace Clinic_System.Application.Features.Inventory.Commands.Handlers
{
    public class CreateInventoryItemCommandHandler : AppRequestHandler<CreateInventoryItemCommand, InventoryItemDTO>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public CreateInventoryItemCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<InventoryItemDTO>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
        {
            if (!IsAdmin)
                return Unauthorized<InventoryItemDTO>("Solo administradores pueden gestionar el inventario.");

            var item = await inventoryService.CreateItemAsync(
                request.Sku, request.Name, request.Category, request.Unit,
                request.MinimumStock, request.InitialQuantity, request.IsActive,
                CurrentUserId, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Success(inventoryService.ToItemDto(item), "Ítem de inventario creado.");
        }
    }

    public class UpdateInventoryItemCommandHandler : AppRequestHandler<UpdateInventoryItemCommand, InventoryItemDTO>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public UpdateInventoryItemCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<InventoryItemDTO>> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
        {
            if (!IsAdmin)
                return Unauthorized<InventoryItemDTO>("Solo administradores pueden gestionar el inventario.");

            var item = await inventoryService.UpdateItemAsync(
                request.Id, request.Sku, request.Name, request.Category, request.Unit,
                request.MinimumStock, request.IsActive, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Success(inventoryService.ToItemDto(item), "Ítem de inventario actualizado.");
        }
    }

    public class SoftDeleteInventoryItemCommandHandler : AppRequestHandler<SoftDeleteInventoryItemCommand, string>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public SoftDeleteInventoryItemCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<string>> Handle(SoftDeleteInventoryItemCommand request, CancellationToken cancellationToken)
        {
            if (!IsAdmin)
                return Unauthorized<string>("Solo administradores pueden gestionar el inventario.");

            await inventoryService.SoftDeleteItemAsync(request.Id, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Success("Ítem eliminado.");
        }
    }

    public class RegisterStockEntryCommandHandler : AppRequestHandler<RegisterStockEntryCommand, StockMovementDTO>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public RegisterStockEntryCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<StockMovementDTO>> Handle(RegisterStockEntryCommand request, CancellationToken cancellationToken)
        {
            if (!IsAdmin)
                return Unauthorized<StockMovementDTO>("Solo administradores pueden registrar entradas.");

            var movement = await inventoryService.RegisterEntryAsync(
                request.InventoryItemId, request.Quantity, request.Reason, request.Notes,
                CurrentUserId, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            var item = await inventoryService.GetItemByIdAsync(request.InventoryItemId, cancellationToken);
            return Success(inventoryService.ToMovementDto(movement, item.Name), "Entrada registrada.");
        }
    }

    public class RegisterStockAdjustmentCommandHandler : AppRequestHandler<RegisterStockAdjustmentCommand, StockMovementDTO>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public RegisterStockAdjustmentCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<StockMovementDTO>> Handle(RegisterStockAdjustmentCommand request, CancellationToken cancellationToken)
        {
            if (!IsAdmin)
                return Unauthorized<StockMovementDTO>("Solo administradores pueden ajustar inventario.");

            var movement = await inventoryService.RegisterAdjustmentAsync(
                request.InventoryItemId, request.NewQuantityOnHand, request.Reason, request.Notes,
                CurrentUserId, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            var item = await inventoryService.GetItemByIdAsync(request.InventoryItemId, cancellationToken);
            return Success(inventoryService.ToMovementDto(movement, item.Name), "Ajuste registrado.");
        }
    }

    public class ReplaceProcedureBomCommandHandler : AppRequestHandler<ReplaceProcedureBomCommand, List<ProcedureMaterialDTO>>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public ReplaceProcedureBomCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<List<ProcedureMaterialDTO>>> Handle(ReplaceProcedureBomCommand request, CancellationToken cancellationToken)
        {
            if (!IsAdmin)
                return Unauthorized<List<ProcedureMaterialDTO>>("Solo administradores pueden editar el BOM.");

            await inventoryService.ReplaceProcedureBomAsync(request.TreatmentProcedureId, request.Materials, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            var bom = await inventoryService.GetProcedureBomAsync(request.TreatmentProcedureId, cancellationToken);
            return Success(bom.Select(inventoryService.ToBomDto).ToList(), "BOM actualizado.");
        }
    }

    public class ConfirmMaterialConsumptionCommandHandler
        : AppRequestHandler<ConfirmMaterialConsumptionCommand, TreatmentMaterialConsumptionDTO>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public ConfirmMaterialConsumptionCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<TreatmentMaterialConsumptionDTO>> Handle(
            ConfirmMaterialConsumptionCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<TreatmentMaterialConsumptionDTO>("Solo médicos o administradores pueden confirmar consumo.");

            var consumption = await inventoryService.ConfirmConsumptionAsync(
                request.DentalTreatmentId,
                request.Lines,
                request.Notes,
                CurrentUserId,
                request.AllowInsufficientStock,
                cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);

            var loaded = await inventoryService.GetConsumptionByTreatmentAsync(request.DentalTreatmentId, cancellationToken);
            return Success(inventoryService.ToConsumptionDto(loaded!), "Consumo de materiales confirmado.");
        }
    }

    public class ReplaceMaterialConsumptionCommandHandler
        : AppRequestHandler<ReplaceMaterialConsumptionCommand, TreatmentMaterialConsumptionDTO>
    {
        private readonly IInventoryService inventoryService;
        private readonly IUnitOfWork unitOfWork;

        public ReplaceMaterialConsumptionCommandHandler(
            ICurrentUserService currentUserService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.inventoryService = inventoryService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<TreatmentMaterialConsumptionDTO>> Handle(
            ReplaceMaterialConsumptionCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<TreatmentMaterialConsumptionDTO>("Solo médicos o administradores pueden modificar consumo.");

            var consumption = await inventoryService.ReplaceConsumptionAsync(
                request.DentalTreatmentId,
                request.Lines,
                request.Notes,
                CurrentUserId,
                request.AllowInsufficientStock,
                cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);

            var loaded = await inventoryService.GetConsumptionByTreatmentAsync(request.DentalTreatmentId, cancellationToken);
            return Success(inventoryService.ToConsumptionDto(loaded!), "Consumo de materiales actualizado.");
        }
    }
}
