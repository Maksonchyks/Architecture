using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.Infrastructure;
using Nimble.Modulith.Customers.UseCases.Customers.Queries;

namespace Nimble.Modulith.Customers.Endpoints.Customers;

public class GetById(IMediator mediator, ICustomerAuthorizationService authService) : EndpointWithoutRequest<CustomerResponse>
{
    public override void Configure()
    {
        Get("/customers/{id}");
        Tags("customers");
        Summary(s =>
        {
            s.Summary = "Get a customer by ID";
            s.Description = "Returns a single customer by their ID";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var result = await mediator.Send(new GetCustomerByIdQuery(id), ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!authService.IsAdminOrOwner(User, result.Value.Email))
        {
            AddError("You can only view your own customer record");
            await Send.ErrorsAsync(statusCode: 403, cancellation: ct);
            return;
        }

        Response = new CustomerResponse(
            result.Value.Id,
            result.Value.FirstName,
            result.Value.LastName,
            result.Value.Email,
            result.Value.PhoneNumber,
            new AddressResponse(result.Value.Address.Street, result.Value.Address.City, result.Value.Address.State, result.Value.Address.PostalCode, result.Value.Address.Country)
        );
    }
}
