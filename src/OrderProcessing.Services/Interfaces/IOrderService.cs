using OrderProcessing.Contracts;
using OrderProcessing.Contracts.DTOs.Requests;
using OrderProcessing.Contracts.DTOs.Responses;

namespace OrderProcessing.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDetailsResponse?> GetByIdAsync(int orderId, CancellationToken token = default);

    Task<IReadOnlyList<OrderDetailsResponse>> GetAllAsync(CancellationToken token = default);

    Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request);

    Task ProcessOrderAsync(int orderId);
}
