using Clinic_System.Application.DTOs.Inventory;

namespace Clinic_System.Application.Features.Inventory.Queries.Models
{
    public class GetInventoryItemListQuery : IRequest<Response<List<InventoryItemDTO>>>
    {
        public bool ActiveOnly { get; set; }
        public bool LowStockOnly { get; set; }
    }

    public class GetInventoryItemByIdQuery : IRequest<Response<InventoryItemDTO>>
    {
        public int Id { get; set; }
    }

    public class GetStockMovementsQuery : IRequest<Response<List<StockMovementDTO>>>
    {
        public int InventoryItemId { get; set; }
        public int Take { get; set; } = 50;
    }

    public class GetProcedureBomQuery : IRequest<Response<List<ProcedureMaterialDTO>>>
    {
        public int TreatmentProcedureId { get; set; }
    }

    public class ProposeMaterialConsumptionQuery : IRequest<Response<MaterialConsumptionProposalDTO>>
    {
        public int DentalTreatmentId { get; set; }
    }

    public class GetMaterialConsumptionQuery : IRequest<Response<TreatmentMaterialConsumptionDTO>>
    {
        public int DentalTreatmentId { get; set; }
    }

    public class GetLowStockQuery : IRequest<Response<LowStockAlertDTO>>
    {
    }
}
