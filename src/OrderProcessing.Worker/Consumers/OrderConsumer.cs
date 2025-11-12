using MassTransit;
using OrderProcessing.Contracts;
using OrderProcessing.Services.Interfaces;

namespace OrderProcessing.Worker;

public class OrderConsumer(IOrderService orderService) : IConsumer<OrderCreatedMessage>
{
    public async Task Consume(ConsumeContext<OrderCreatedMessage> context)
    {
        var order = context.Message;
        Console.WriteLine($"Received order {order.OrderId}");

        // Simulate processing
        await Task.Delay(1000);
        await orderService.ProcessOrderAsync(order.OrderId);

        Console.WriteLine($"Processed order {order.OrderId}");
    }
}
