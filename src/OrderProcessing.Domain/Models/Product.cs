namespace OrderProcessing.Domain.Models;

public class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required decimal Price { get; set; }

    public Inventory Inventory { get; set; } = null!;

    public ICollection<Item> Items { get; set; } = [];
}
