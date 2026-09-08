namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IInventoryItemRepository : IGenericRepository<InventoryItem>
    {
        Task<IEnumerable<InventoryItem>> GetAllAsync(bool activeOnly, bool lowStockOnly, CancellationToken cancellationToken = default);
        Task<InventoryItem?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<InventoryItem?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
        Task<int> CountLowStockAsync(CancellationToken cancellationToken = default);
    }
}
