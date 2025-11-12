namespace OrderProcessing.Domain.Models;

public class Item
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int Quantity { get; set; }
}
