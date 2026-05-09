namespace Nimble.Modulith.Reporting.Models;

public class FactOrder
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    
    // Foreign Keys to Dimensions
    public int DateKey { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    
    // Navigation Properties
    public DimDate Date { get; set; } = null!;
    public DimCustomer Customer { get; set; } = null!;
    public DimProduct Product { get; set; } = null!;
    
    // Measures
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal OrderTotalAmount { get; set; }
}
