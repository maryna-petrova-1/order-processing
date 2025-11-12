namespace OrderProcessing.Domain.Models;

public class Client
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}
