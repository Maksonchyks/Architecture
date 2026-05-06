using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.Infrastructure;
using Nimble.Modulith.Customers.UseCases.Customers.Queries;
using Nimble.Modulith.Customers.UseCases.Orders.Queries;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class GetById(IMediator mediator, ICustomerAuthorizationService authService) : EndpointWithoutRequest<OrderResponse>
{
    public override void Configure()
    {
        Get("/orders/{id}");
        Tags("orders");
        Summary(s =>
        {
            s.Summary = "Get an order by ID";
            s.Description = "Returns a single order with all its items";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await mediator.Send(new GetOrderByIdQuery(id), ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Verify ownership via customer record
        var customerResult = await mediator.Send(new GetCustomerByIdQuery(result.Value.CustomerId), ct);
        if (customerResult.IsSuccess && !authService.IsAdminOrOwner(User, customerResult.Value.Email))
        {
            AddError("You can only view your own orders");
            await Send.ErrorsAsync(statusCode: 403, cancellation: ct);
            return;
        }

        Response = new OrderResponse(
            result.Value.Id, result.Value.CustomerId, result.Value.OrderNumber,
            result.Value.OrderDate, result.Value.Status, result.Value.TotalAmount,
            result.Value.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
        );
    }
}
