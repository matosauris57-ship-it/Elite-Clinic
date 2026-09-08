namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class StockMovementRepository : GenericRepository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(AppDbContext context) : base(context) { }

        public Task<List<StockMovement>> GetByItemIdAsync(
            int inventoryItemId, int take = 50, CancellationToken cancellationToken = default)
        {
            return context.StockMovements
                .AsNoTracking()
                .Where(m => m.InventoryItemId == inventoryItemId)
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public Task<List<StockMovement>> GetByReferenceAsync(
            string referenceType, string referenceId, CancellationToken cancellationToken = default)
        {
            return context.StockMovements
                .AsNoTracking()
                .Where(m => m.ReferenceType == referenceType && m.ReferenceId == referenceId)
                .OrderBy(m => m.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
