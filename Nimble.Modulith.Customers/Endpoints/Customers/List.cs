using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Customers.Queries;

namespace Nimble.Modulith.Customers.Endpoints.Customers;

public class List(IMediator mediator) : EndpointWithoutRequest<List<CustomerResponse>>
{
    public override void Configure()
    {
        Get("/customers");
        Roles("Admin");
        Tags("customers");
        Summary(s =>
        {
            s.Summary = "List all customers";
            s.Description = "Returns a list of all customers (Admin only)";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new ListCustomersQuery(), ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        Response = result.Value.Select(c => new CustomerResponse(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            c.PhoneNumber,
            new AddressResponse(c.Address.Street, c.Address.City, c.Address.State, c.Address.PostalCode, c.Address.Country)
        )).ToList();
    }
}
