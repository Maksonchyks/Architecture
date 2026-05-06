using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Nimble.Modulith.Users.Events;

namespace Nimble.Modulith.Users.Endpoints;

public class AddRoleToUserRequest
{
    public string RoleName { get; set; } = string.Empty;
}

public class AddRoleToUserResponse
{
    public string Message { get; set; } = string.Empty;
}

public class AddRoleToUser(
    UserManager<IdentityUser> userManager,
    IMediator mediator) : Endpoint<AddRoleToUserRequest, AddRoleToUserResponse>
{
    public override void Configure()
    {
        Post("/users/{id}/roles");
        Roles("Admin");
        Tags("users");
        Summary(s =>
        {
            s.Summary = "Add a user to a role";
            s.Description = "Assigns a user to Admin or Customer role (Admin only)";
        });
    }

    public override async Task HandleAsync(AddRoleToUserRequest req, CancellationToken ct)
    {
        var userId = Route<string>("id")!;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            AddError($"User with ID '{userId}' not found");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        // Normalize: "admin" -> "Admin"
        var normalizedRoleName = char.ToUpper(req.RoleName[0]) + req.RoleName[1..].ToLower();

        if (normalizedRoleName != "Admin" && normalizedRoleName != "Customer")
        {
            AddError($"Role '{normalizedRoleName}' does not exist. Valid roles: Admin, Customer");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (await userManager.IsInRoleAsync(user, normalizedRoleName))
        {
            AddError($"User is already in the '{normalizedRoleName}' role");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var result = await userManager.AddToRoleAsync(user, normalizedRoleName);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                AddError(error.Description);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await mediator.Publish(new UserAddedToRoleEvent(user.Id, user.Email!, normalizedRoleName), ct);

        Response.Message = $"User '{user.Email}' successfully added to role '{normalizedRoleName}'";
    }
}
