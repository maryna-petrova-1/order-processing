using Microsoft.Extensions.Logging;
using OrderProcessing.Domain.Models;

namespace OrderProcessing.Infrastructure;

public static class DbSeeder
{
    public static void Seed(OrdersDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting database seeding...");

        if (!context.Clients.Any())
        {
            context.Clients.AddRange(
                new Client { Name = "John Doe", Email = "john@example.com" },
                new Client { Name = "Jane Smith", Email = "jane@example.com" }
            );
            logger.LogInformation("Seeded clients: John Doe, Jane Smith");
        }
        else
        {
            logger.LogInformation("Clients already exist, skipping...");
        }

        if (!context.Products.Any())
        {
            var laptop = new Product { Name = "Laptop", Price = 1200 };
            var phone = new Product { Name = "Phone", Price = 800 };
            var headphones = new Product { Name = "Headphones", Price = 150 };

            context.Products.AddRange(laptop, phone, headphones);
            context.SaveChanges();

            logger.LogInformation("Seeded products: Laptop, Phone, Headphones");

            context.Inventories.AddRange(
                new Inventory { ProductId = laptop.Id, QuantityAvailable = 10 },
                new Inventory { ProductId = phone.Id, QuantityAvailable = 20 },
                new Inventory { ProductId = headphones.Id, QuantityAvailable = 50 }
            );
            logger.LogInformation("Seeded inventories for products");
        }
        else
        {
            logger.LogInformation("Products already exist, skipping inventory seeding...");
        }

        context.SaveChanges();

        logger.LogInformation("Database seeding completed successfully.");
    }
}