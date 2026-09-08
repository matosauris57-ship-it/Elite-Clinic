namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IStockMovementRepository : IGenericRepository<StockMovement>
    {
        Task<List<StockMovement>> GetByItemIdAsync(int inventoryItemId, int take = 50, CancellationToken cancellationToken = default);
        Task<List<StockMovement>> GetByReferenceAsync(string referenceType, string referenceId, CancellationToken cancellationToken = default);
    }
}
