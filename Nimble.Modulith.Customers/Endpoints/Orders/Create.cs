using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Orders.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Orders;

public class Create(IMediator mediator) : Endpoint<CreateOrderRequest, OrderResponse>
{
    public override void Configure()
    {
        Post("/orders");
        AllowAnonymous();
        Tags("orders");
        Summary(s =>
        {
            s.Summary = "Create a new order";
            s.Description = "Creates a new order with the provided items";
        });
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            req.CustomerId,
            req.OrderDate,
            req.Items.Select(i => new CreateOrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList()
        );

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.CreatedAtAsync<GetById>(
            new { id = result.Value.Id },
            MapToResponse(result.Value),
            generateAbsoluteUrl: false,
            cancellation: ct
        );
    }

    private static OrderResponse MapToResponse(UseCases.Orders.OrderDto dto) => new(
        dto.Id, dto.CustomerId, dto.OrderNumber, dto.OrderDate, dto.Status, dto.TotalAmount,
        dto.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
    );
}
