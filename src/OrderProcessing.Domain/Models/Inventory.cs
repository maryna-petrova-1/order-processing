using System.ComponentModel.DataAnnotations;

namespace OrderProcessing.Domain.Models;

public class Inventory
{
    [Key]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int QuantityAvailable { get; set; }
}
