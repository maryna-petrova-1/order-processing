namespace OrderProcessing.Contracts.DTOs.Responses;

public class OrderDetailsResponse
{
    public int OrderId { get; set; }
    public int ClientId { get; set; }
    public IReadOnlyList<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
}
