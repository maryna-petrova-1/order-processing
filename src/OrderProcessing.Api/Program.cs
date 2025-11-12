using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessing.Contracts;
using OrderProcessing.Domain;
using OrderProcessing.Infrastructure;
using OrderProcessing.Services;
using OrderProcessing.Contracts.DTOs.Requests;
using OrderProcessing.Services.Interfaces;
using Microsoft.Extensions.Options;
using OrderProcessing.Contracts.DTOs.Responses;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

        cfg.Host(settings.Host, settings.VHost, h =>
        {
            h.Username(settings.Username);
            h.Password(settings.Password);
        });
    });
});

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    db.Database.EnsureCreated();
    DbSeeder.Seed(db, logger);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var ordersGroup = app.MapGroup("/api/orders")
    .WithTags("Orders")
    .WithOpenApi();

ordersGroup.MapGet("/", async (
    IOrderService orderService,
    CancellationToken token) =>
{
    var orders = await orderService.GetAllAsync();

    return Results.Ok(orders);
})
.WithName("GetAllOrders")
.WithSummary("Get all orders")
.WithDescription("Returns a list of all orders with their items and product info")
.Produces<List<OrderDetailsResponse>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status200OK);

ordersGroup.MapGet("/{id:int}", async (
    int id,
    IOrderService orderService,
    CancellationToken token) =>
{
    var order = await orderService.GetByIdAsync(id);

    if (order == null)
        return Results.NotFound(new { error = "Order not found" });

    return Results.Ok(order);
})
.WithName("GetOrderById")
.WithSummary("Get order by ID")
.WithDescription("Returns order details including items and product info")
.Produces<OrderDetailsResponse>(StatusCodes.Status200OK)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound);

ordersGroup.MapPost("/", async (
    CreateOrderRequest request,
    IOrderService orderService,
    ILogger<Program> logger,
    CancellationToken token) =>
{
    if (request.Items == null || !request.Items.Any())
    {
        return Results.BadRequest(new { error = "Order must contain at least one item" });
    }

    foreach (var item in request.Items)
    {
        if (item.Quantity <= 0)
        {
            return Results.BadRequest(new { error = "Quantity must be greater than zero" });
        }
    }

    try
    {
        var response = await orderService.CreateAsync(request);

        return Results.Accepted($"/api/orders/{response.OrderId}", response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating order");
        return Results.Problem("An error occurred while creating the order");
    }
})
.WithName("CreateOrder")
.WithSummary("Submit a new order")
.WithDescription("Creates a new order and queues it for asynchronous processing")
.Produces<CreateOrderResponse>(StatusCodes.Status202Accepted)
.Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

app.Run();