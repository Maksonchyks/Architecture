using FastEndpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints.Reports;

public class OrdersReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Format { get; set; }
}

public class OrdersReport(IReportService reportService) : Endpoint<OrdersReportRequest, OrdersReportResult>
{
    public override void Configure()
    {
        Get("/reports/orders");
        AllowAnonymous(); // For testing, restrict in prod
        Summary(s => 
        {
            s.Summary = "Get orders report";
            s.Description = "Returns a report of orders between dates";
        });
    }

    public override async Task HandleAsync(OrdersReportRequest req, CancellationToken ct)
    {
        var result = await reportService.GetOrdersReportAsync(req.StartDate, req.EndDate, ct);
        
        bool wantsCsv = req.Format?.Equals("csv", StringComparison.OrdinalIgnoreCase) == true || 
                        HttpContext.Request.Headers.Accept.Contains("text/csv");

        if (wantsCsv)
        {
            await this.SendCsvAsync(result.Orders, ct);
        }
        else
        {
            Response = result;
        }
    }
}
