using FastEndpoints;
using Nimble.Modulith.Reporting.Services;

namespace Nimble.Modulith.Reporting.Endpoints.Reports;

public class ProductSalesReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Format { get; set; }
}

public class ProductSalesReport(IReportService reportService) : Endpoint<ProductSalesReportRequest, IEnumerable<ProductSalesSummary>>
{
    public override void Configure()
    {
        Get("/reports/product-sales");
        AllowAnonymous(); // For testing, restrict in prod
        Summary(s => 
        {
            s.Summary = "Get product sales report";
            s.Description = "Returns a report of product sales between dates";
        });
    }

    public override async Task HandleAsync(ProductSalesReportRequest req, CancellationToken ct)
    {
        var result = await reportService.GetProductSalesReportAsync(req.StartDate, req.EndDate, ct);
        
        bool wantsCsv = req.Format?.Equals("csv", StringComparison.OrdinalIgnoreCase) == true || 
                        HttpContext.Request.Headers.Accept.Contains("text/csv");

        if (wantsCsv)
        {
            await this.SendCsvAsync(result, ct);
        }
        else
        {
            Response = result;
        }
    }
}
