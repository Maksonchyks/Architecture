using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.Infrastructure;
using Nimble.Modulith.Customers.UseCases.Customers.Queries;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;
using Nimble.Modulith.Customers.UseCases.Orders.Queries;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class AddItem(IMediator mediator, ICustomerAuthorizationService authService) : Endpoint<AddOrderItemRequest, OrderResponse>
{
    public override void Configure()
    {
        Post("/orders/{id}/items");
        Tags("orders");
        Summary(s =>
        {
            s.Summary = "Add an item to an order";
            s.Description = "Adds a new item to an existing order";
        });
    }

    public override async Task HandleAsync(AddOrderItemRequest req, CancellationToken ct)
    {
        var orderId = Route<int>("id");

        // Verify order exists and user has permission
        var orderResult = await mediator.Send(new GetOrderByIdQuery(orderId), ct);
        if (!orderResult.IsSuccess)
        {
            AddError($"Order with ID {orderId} not found");
            await Send.ErrorsAsync(statusCode: 404, cancellation: ct);
            return;
        }

        var customerResult = await mediator.Send(new GetCustomerByIdQuery(orderResult.Value.CustomerId), ct);
        if (customerResult.IsSuccess && !authService.IsAdminOrOwner(User, customerResult.Value.Email))
        {
            AddError("You can only modify your own orders");
            await Send.ErrorsAsync(statusCode: 403, cancellation: ct);
            return;
        }

        var result = await mediator.Send(new AddOrderItemCommand(orderId, req.ProductId, req.Quantity), ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        Response = new OrderResponse(
            result.Value.Id, result.Value.CustomerId, result.Value.OrderNumber,
            result.Value.OrderDate, result.Value.Status, result.Value.TotalAmount,
            result.Value.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
        );
    }
}
