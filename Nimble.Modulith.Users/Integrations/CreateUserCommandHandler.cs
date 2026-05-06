using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Users.Contracts;
using Nimble.Modulith.Users.UseCases.Commands;

namespace Nimble.Modulith.Users.Integrations;

public class CreateUserCommandHandler(IMediator mediator)
    : ICommandHandler<CreateUserCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var internalCommand = new CreateUserInternalCommand(
            command.Email,
            command.Password
        );

        return await mediator.Send(internalCommand, cancellationToken);
    }
}
