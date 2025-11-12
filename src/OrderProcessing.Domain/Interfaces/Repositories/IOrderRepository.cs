using OrderProcessing.Domain.Models;

namespace OrderProcessing.Domain;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken token = default);

    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken token = default);

    Task<int> AddAsync(Order order);

    Task UpdateAsync(Order order);
}
