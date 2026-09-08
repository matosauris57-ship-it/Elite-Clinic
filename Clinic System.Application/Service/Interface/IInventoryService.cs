namespace Clinic_System.Application.Service.Interface
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryItem>> GetItemsAsync(bool activeOnly, bool lowStockOnly, CancellationToken cancellationToken = default);
        Task<InventoryItem> GetItemByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<InventoryItem> CreateItemAsync(
            string sku,
            string name,
            string category,
            string unit,
            decimal minimumStock,
            decimal initialQuantity,
            bool isActive,
            string? recordedByUserId,
            CancellationToken cancellationToken = default);
        Task<InventoryItem> UpdateItemAsync(
            int id,
            string sku,
            string name,
            string category,
            string unit,
            decimal minimumStock,
            bool isActive,
            CancellationToken cancellationToken = default);
        Task SoftDeleteItemAsync(int id, CancellationToken cancellationToken = default);

        Task<StockMovement> RegisterEntryAsync(
            int inventoryItemId,
            decimal quantity,
            string? reason,
            string? notes,
            string? recordedByUserId,
            CancellationToken cancellationToken = default);

        Task<StockMovement> RegisterAdjustmentAsync(
            int inventoryItemId,
            decimal newQuantityOnHand,
            string? reason,
            string? notes,
            string? recordedByUserId,
            CancellationToken cancellationToken = default);

        Task<List<StockMovement>> GetMovementsAsync(int inventoryItemId, int take = 50, CancellationToken cancellationToken = default);

        Task<List<ProcedureMaterial>> GetProcedureBomAsync(int treatmentProcedureId, CancellationToken cancellationToken = default);
        Task ReplaceProcedureBomAsync(
            int treatmentProcedureId,
            IEnumerable<ProcedureMaterialInput> materials,
            CancellationToken cancellationToken = default);

        Task<MaterialConsumptionProposalDTO> ProposeConsumptionAsync(int dentalTreatmentId, CancellationToken cancellationToken = default);
        Task<TreatmentMaterialConsumption> ConfirmConsumptionAsync(
            int dentalTreatmentId,
            IEnumerable<MaterialConsumptionLineInput>? lines,
            string? notes,
            string? recordedByUserId,
            bool allowInsufficientStock = false,
            CancellationToken cancellationToken = default);
        Task<TreatmentMaterialConsumption?> GetConsumptionByTreatmentAsync(int dentalTreatmentId, CancellationToken cancellationToken = default);
        Task<TreatmentMaterialConsumption> ReplaceConsumptionAsync(
            int dentalTreatmentId,
            IEnumerable<MaterialConsumptionLineInput> lines,
            string? notes,
            string? recordedByUserId,
            bool allowInsufficientStock = false,
            CancellationToken cancellationToken = default);

        Task<LowStockAlertDTO> GetLowStockAsync(CancellationToken cancellationToken = default);

        InventoryItemDTO ToItemDto(InventoryItem item);
        List<InventoryItemDTO> ToItemDtos(IEnumerable<InventoryItem> items);
        StockMovementDTO ToMovementDto(StockMovement movement, string? itemName = null);
        ProcedureMaterialDTO ToBomDto(ProcedureMaterial material);
        TreatmentMaterialConsumptionDTO ToConsumptionDto(TreatmentMaterialConsumption consumption);
    }
}
