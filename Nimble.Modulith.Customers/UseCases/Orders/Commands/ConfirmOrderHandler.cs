using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Contracts;
using Nimble.Modulith.Customers.Domain.CustomerAggregate;
using Nimble.Modulith.Customers.Domain.Interfaces;
using Nimble.Modulith.Customers.Domain.OrderAggregate;
using Nimble.Modulith.Email.Contracts;

namespace Nimble.Modulith.Customers.UseCases.Orders.Commands;

public class ConfirmOrderHandler(
    IRepository<Order> repository, 
    IReadRepository<Customer> customerRepository,
    IMediator mediator) 
    : ICommandHandler<ConfirmOrderCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(ConfirmOrderCommand command, CancellationToken ct)
    {
        var spec = new OrderByIdSpec(command.OrderId);
        var order = await repository.FirstOrDefaultAsync(spec, ct);

        if (order is null)
        {
            return Result<OrderDto>.NotFound($"Order with ID {command.OrderId} not found");
        }

        var customer = await customerRepository.GetByIdAsync(order.CustomerId, ct);
        if (customer is null)
        {
            return Result<OrderDto>.NotFound($"Customer with ID {order.CustomerId} not found");
        }

        // Change order status to Confirmed
        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(order, ct);
        await repository.SaveChangesAsync(ct);

        var items = order.Items.Select(i => new OrderItemDetails(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice
        )).ToList();

        await mediator.Publish(new OrderCreatedEvent(
            order.Id,
            customer.Id,
            customer.FullName,
            customer.Email,
            order.OrderNumber,
            order.OrderDate,
            order.TotalAmount,
            items
        ), ct);

        // Send confirmation email
        var emailBody = $@"
Dear {customer.FullName},

Your order has been confirmed!

Order Number: {order.OrderNumber}
Order Date: {order.OrderDate:yyyy-MM-dd}
Total Amount: ${order.TotalAmount:F2}

Items:
{string.Join("\n", order.Items.Select(i => $"- {i.ProductName} x {i.Quantity} @ ${i.UnitPrice:F2} = ${i.TotalPrice:F2}"))}

Thank you for your order!
";

        await mediator.Send(new SendEmailCommand(
            customer.Email,
            $"Order Confirmation - {order.OrderNumber}",
            emailBody
        ), ct);

        var dto = new OrderDto(
            order.Id,
            order.CustomerId,
            order.OrderNumber,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice
            )).ToList(),
            order.CreatedAt,
            order.UpdatedAt
        );

        return Result<OrderDto>.Success(dto);
    }
}
