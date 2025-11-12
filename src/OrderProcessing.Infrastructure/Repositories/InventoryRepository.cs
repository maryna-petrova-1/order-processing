using Microsoft.EntityFrameworkCore;
using OrderProcessing.Domain;
using OrderProcessing.Domain.Models;

namespace OrderProcessing.Infrastructure;

public class InventoryRepository(OrdersDbContext context) : IInventoryRepository
{
    public async Task<IReadOnlyList<Inventory>> GetByProductIdsAsync(IEnumerable<int> productIds, CancellationToken token = default)
    {
        return await context.Inventories
            .Include(i => i.Product)
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(token);
    }
}
