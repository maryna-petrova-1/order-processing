using OrderProcessing.Domain;
using OrderProcessing.Domain.Models;
using OrderProcessing.Contracts.DTOs.Requests;
using OrderProcessing.Services.Interfaces;
using MassTransit;
using OrderProcessing.Contracts;
using OrderProcessing.Contracts.DTOs.Responses;

namespace OrderProcessing.Services;

public class OrderService(IOrderRepository orderRepository,
    IInventoryRepository inventoryRepository,
    IPublishEndpoint publishEndpoint) : IOrderService
{
    public async Task<OrderDetailsResponse?> GetByIdAsync(int orderId, CancellationToken token = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, token);

        if (order == null)
            return null;

        return new OrderDetailsResponse
        {
            OrderId = order.Id,
            ClientId = order.ClientId,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            Items = [.. order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                Price = i.Product.Price
            })]
        };
    }

    public async Task<IReadOnlyList<OrderDetailsResponse>> GetAllAsync(CancellationToken token = default)
    {
        var orders = await orderRepository.GetAllAsync(token);

        return [.. orders.Select(order => new OrderDetailsResponse
        {
            OrderId = order.Id,
            ClientId = order.ClientId,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            Items = [.. order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                Price = i.Product.Price
            })]
        })];
    }

    public async Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request)
    {
        var order = new Order
        {
            ClientId = request.ClientId,
            Items = [.. request.Items.Select(i => new Item
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            })]
        };

        int orderId = await orderRepository.AddAsync(order);

        await publishEndpoint.Publish(new OrderCreatedMessage { OrderId = orderId });

        return new CreateOrderResponse { OrderId = orderId };
    }

    public async Task ProcessOrderAsync(int orderId)
    {
        var order = await orderRepository.GetByIdAsync(orderId)
            ?? throw new InvalidOperationException($"Order with ID {orderId} not found.");

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.Product.Price);

        if (order.TotalAmount <= 0)
        {
            order.Status = OrderStatus.Cancelled;
        }
        else
        {
            var inventories = await inventoryRepository.GetByProductIdsAsync(order.Items.Select(i => i.ProductId));

            var inventoriesByProductId = inventories.ToDictionary(i => i.ProductId);

            foreach (var item in order.Items)
            {
                if (!inventoriesByProductId.TryGetValue(item.ProductId, out var inventory))
                {
                    order.Status = OrderStatus.MissingProducts;
                    break;
                }

                if (inventory.QuantityAvailable < item.Quantity)
                {
                    order.Status = OrderStatus.MissingProducts;
                    break;
                }

                inventory.QuantityAvailable -= item.Quantity;
            }

            order.Status = OrderStatus.Processed;
        }

        await orderRepository.UpdateAsync(order);
    }
}
