using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Nimble.Modulith.Reporting.Services;

public class ReportService(IConfiguration configuration) : IReportService
{
    private readonly string _connectionString = configuration.GetConnectionString("reportingdb") 
        ?? throw new InvalidOperationException("Connection string 'reportingdb' not found.");

    public async Task<OrdersReportResult> GetOrdersReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        var dateStartKey = ConvertToDateKey(startDate);
        var dateEndKey = ConvertToDateKey(endDate);

        var sql = @"
            SELECT 
                f.OrderNumber,
                MIN(d.Date) as OrderDate,
                MIN(c.Name) as CustomerName,
                SUM(f.Quantity) as ItemsCount,
                MIN(f.OrderTotalAmount) as TotalAmount
            FROM FactOrders f
            JOIN DimDate d ON f.DateKey = d.DateKey
            JOIN DimCustomer c ON f.CustomerId = c.CustomerId
            WHERE f.DateKey >= @DateStartKey AND f.DateKey <= @DateEndKey
            GROUP BY f.OrderNumber
            ORDER BY OrderDate DESC";

        var orders = (await connection.QueryAsync<OrderSummary>(sql, new { DateStartKey = dateStartKey, DateEndKey = dateEndKey })).ToList();

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        return new OrdersReportResult(orders, totalOrders, totalRevenue, averageOrderValue);
    }

    public async Task<IEnumerable<ProductSalesSummary>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        var dateStartKey = ConvertToDateKey(startDate);
        var dateEndKey = ConvertToDateKey(endDate);

        var sql = @"
            SELECT 
                p.ProductId,
                p.Name as ProductName,
                SUM(f.Quantity) as TotalQuantitySold,
                COUNT(DISTINCT f.OrderNumber) as OrderCount,
                SUM(f.TotalPrice) as TotalRevenue
            FROM FactOrders f
            JOIN DimProduct p ON f.ProductId = p.ProductId
            WHERE f.DateKey >= @DateStartKey AND f.DateKey <= @DateEndKey
            GROUP BY p.ProductId, p.Name
            ORDER BY TotalRevenue DESC";

        return await connection.QueryAsync<ProductSalesSummary>(sql, new { DateStartKey = dateStartKey, DateEndKey = dateEndKey });
    }

    public async Task<CustomerLifetimeMetrics?> GetCustomerOrdersReportAsync(int customerId, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        var customerSql = "SELECT Name FROM DimCustomer WHERE CustomerId = @CustomerId";
        var customerName = await connection.QueryFirstOrDefaultAsync<string>(customerSql, new { CustomerId = customerId });

        if (customerName == null) return null;

        var ordersSql = @"
            SELECT 
                f.OrderNumber,
                MIN(d.Date) as OrderDate,
                SUM(f.Quantity) as ItemsCount,
                MIN(f.OrderTotalAmount) as TotalAmount
            FROM FactOrders f
            JOIN DimDate d ON f.DateKey = d.DateKey
            WHERE f.CustomerId = @CustomerId
            GROUP BY f.OrderNumber
            ORDER BY OrderDate DESC";

        var orders = (await connection.QueryAsync<CustomerOrderSummary>(ordersSql, new { CustomerId = customerId })).ToList();

        if (orders.Count == 0) return new CustomerLifetimeMetrics(customerName, 0, DateTime.MinValue, DateTime.MinValue, orders);

        var totalSpent = orders.Sum(o => o.TotalAmount);
        var firstOrderDate = orders.Min(o => o.OrderDate);
        var lastOrderDate = orders.Max(o => o.OrderDate);

        return new CustomerLifetimeMetrics(customerName, totalSpent, firstOrderDate, lastOrderDate, orders);
    }

    private static int ConvertToDateKey(DateTime date)
    {
        return date.Year * 10000 + date.Month * 100 + date.Day;
    }
}
