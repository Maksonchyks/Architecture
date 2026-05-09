namespace Nimble.Modulith.Reporting.Services;

public record OrderSummary(
    string OrderNumber,
    DateTime OrderDate,
    string CustomerName,
    int ItemsCount,
    decimal TotalAmount
);

public record OrdersReportResult(
    List<OrderSummary> Orders,
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue
);

public record ProductSalesSummary(
    int ProductId,
    string ProductName,
    int TotalQuantitySold,
    int OrderCount,
    decimal TotalRevenue
);

public record CustomerOrderSummary(
    string OrderNumber,
    DateTime OrderDate,
    int ItemsCount,
    decimal TotalAmount
);

public record CustomerLifetimeMetrics(
    string CustomerName,
    decimal TotalSpent,
    DateTime FirstOrderDate,
    DateTime LastOrderDate,
    List<CustomerOrderSummary> Orders
);
