namespace OrderProcessing.Contracts.DTOs.Requests;

public class CreateOrderRequest
{
    public int ClientId { get; set; }

    public IEnumerable<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
}
