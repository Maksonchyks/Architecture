using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Customers.Contracts;
using Nimble.Modulith.Reporting.Data;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Ingest;

public class OrderCreatedEventHandler(
    ReportingDbContext dbContext,
    ILogger<OrderCreatedEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Ingesting order {OrderNumber} into reporting database...", notification.OrderNumber);
        
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                // 1. Ensure DimCustomer exists
                var customer = await dbContext.Customers.FindAsync([notification.CustomerId], ct);
                if (customer == null)
                {
                    customer = new DimCustomer
                    {
                        CustomerId = notification.CustomerId,
                        Name = notification.CustomerName,
                        Email = notification.CustomerEmail
                    };
                    dbContext.Customers.Add(customer);
                }

                // 2. Insert Fact Orders
                int dateKey = ConvertToDateKey(notification.OrderDate);
                
                foreach (var item in notification.Items)
                {
                    // Ensure DimProduct exists
                    var product = await dbContext.Products.FindAsync([item.ProductId], ct);
                    if (product == null)
                    {
                        product = new DimProduct
                        {
                            ProductId = item.ProductId,
                            Name = item.ProductName
                        };
                        dbContext.Products.Add(product);
                    }

                    // Check for existing FactOrder to ensure idempotency
                    var existingFact = await dbContext.FactOrders
                        .FirstOrDefaultAsync(f => f.OrderId == notification.OrderId && f.OrderItemId == item.Id, ct);
                        
                    if (existingFact == null)
                    {
                        var factOrder = new FactOrder
                        {
                            OrderId = notification.OrderId,
                            OrderItemId = item.Id,
                            OrderNumber = notification.OrderNumber,
                            DateKey = dateKey,
                            CustomerId = notification.CustomerId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.TotalPrice,
                            OrderTotalAmount = notification.TotalAmount
                        };
                        dbContext.FactOrders.Add(factOrder);
                    }
                }

                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                logger.LogInformation("Successfully ingested order {OrderNumber}", notification.OrderNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                logger.LogError(ex, "Failed to ingest order {OrderNumber} into reporting db", notification.OrderNumber);
                throw;
            }
        });
    }

    private static int ConvertToDateKey(DateOnly date)
    {
        return date.Year * 10000 + date.Month * 100 + date.Day;
    }
}
