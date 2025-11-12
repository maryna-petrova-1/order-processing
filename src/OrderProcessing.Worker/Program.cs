using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderProcessing.Domain;
using OrderProcessing.Infrastructure;
using OrderProcessing.Services;
using OrderProcessing.Services.Interfaces;
using OrderProcessing.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitSettings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

        cfg.Host(rabbitSettings.Host, rabbitSettings.VHost, h =>
        {
            h.Username(rabbitSettings.Username);
            h.Password(rabbitSettings.Password);
        });

        cfg.ReceiveEndpoint("orders_queue", e =>
        {
            e.ConfigureConsumer<OrderConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
