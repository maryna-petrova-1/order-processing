namespace OrderProcessing.Domain.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }
    
    public int ClientId { get; set; }
    
    public Client Client { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public ICollection<Item> Items { get; set; } = [];
}
