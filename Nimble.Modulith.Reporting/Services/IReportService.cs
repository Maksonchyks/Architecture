namespace Nimble.Modulith.Reporting.Services;

public interface IReportService
{
    Task<OrdersReportResult> GetOrdersReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<IEnumerable<ProductSalesSummary>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<CustomerLifetimeMetrics?> GetCustomerOrdersReportAsync(int customerId, CancellationToken ct = default);
}
