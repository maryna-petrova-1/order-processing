using Microsoft.EntityFrameworkCore;
using OrderProcessing.Domain;
using OrderProcessing.Domain.Models;

namespace OrderProcessing.Infrastructure;

public class OrderRepository(OrdersDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int id, CancellationToken token = default)
    {
        return await context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, token);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken token = default)
    {
        return await context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .ToListAsync(token);
    }

    public async Task<int> AddAsync(Order order)
    {
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
        return order.Id;
    }

    public async Task UpdateAsync(Order order)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync();
    }
}