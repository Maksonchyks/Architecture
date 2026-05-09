using FastEndpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints.Reports;

public class CustomerOrdersReportRequest
{
    public int CustomerId { get; set; }
    public string? Format { get; set; }
}

public class CustomerOrdersReport(IReportService reportService) : Endpoint<CustomerOrdersReportRequest, CustomerLifetimeMetrics>
{
    public override void Configure()
    {
        Get("/reports/customers/{CustomerId}/orders");
        AllowAnonymous(); // For testing, restrict in prod
        Summary(s => 
        {
            s.Summary = "Get customer orders report";
            s.Description = "Returns a report of all orders for a specific customer";
        });
    }

    public override async Task HandleAsync(CustomerOrdersReportRequest req, CancellationToken ct)
    {
        var result = await reportService.GetCustomerOrdersReportAsync(req.CustomerId, ct);
        
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

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
