namespace Nimble.Modulith.Reporting.Models;

public class DimCustomer
{
    public int CustomerId { get; set; } // Matches Customer Module ID
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
