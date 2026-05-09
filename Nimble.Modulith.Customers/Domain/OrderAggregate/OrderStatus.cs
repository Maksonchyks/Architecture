namespace Nimble.Modulith.Customers.Domain.OrderAggregate;

public enum OrderStatus
{
    Pending = 0,      // Can add/remove items
    Confirmed = 1,    // Locked for reporting - NO changes allowed
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
