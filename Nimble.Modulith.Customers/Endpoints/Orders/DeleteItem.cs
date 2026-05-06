using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class DeleteItem(IMediator mediator) : EndpointWithoutRequest<OrderResponse>
{
    public override void Configure()
    {
        Delete("/orders/{id}/items/{itemId}");
        AllowAnonymous();
        Tags("orders");
        Summary(s =>
        {
            s.Summary = "Delete an item from an order";
            s.Description = "Removes an item from an existing order";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orderId = Route<int>("id");
        var itemId = Route<int>("itemId");
        var result = await mediator.Send(new DeleteOrderItemCommand(orderId, itemId), ct);

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
