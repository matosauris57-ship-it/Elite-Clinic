namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class InventoryItemRepository : GenericRepository<InventoryItem>, IInventoryItemRepository
    {
        public InventoryItemRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<InventoryItem>> GetAllAsync(
            bool activeOnly,
            bool lowStockOnly,
            CancellationToken cancellationToken = default)
        {
            IQueryable<InventoryItem> query = context.InventoryItems.AsNoTracking();

            if (activeOnly)
                query = query.Where(i => i.IsActive);

            if (lowStockOnly)
                query = query.Where(i => i.QuantityOnHand <= i.MinimumStock);

            return await query
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync(cancellationToken);
        }

        public Task<InventoryItem?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return context.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Sku == sku, cancellationToken);
        }

        public Task<InventoryItem?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
        {
            return context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public Task<int> CountLowStockAsync(CancellationToken cancellationToken = default)
        {
            return context.InventoryItems
                .AsNoTracking()
                .CountAsync(i => i.IsActive && i.QuantityOnHand <= i.MinimumStock, cancellationToken);
        }
    }
}
