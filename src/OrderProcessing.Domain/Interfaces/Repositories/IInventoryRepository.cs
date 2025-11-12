using OrderProcessing.Domain.Models;

namespace OrderProcessing.Domain;

public interface IInventoryRepository
{
    Task<IReadOnlyList<Inventory>> GetByProductIdsAsync(IEnumerable<int> productIds, CancellationToken token = default);
}
