using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Nimble.Modulith.Users.Endpoints;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

public class ListUsersResponse
{
    public List<UserResponse> Users { get; set; } = [];
}

public class ListUsers(
    UserManager<IdentityUser> userManager) : Endpoint<EmptyRequest, ListUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
        Roles("Admin");
        Tags("users");
        Summary(s =>
        {
            s.Summary = "Get all users";
            s.Description = "Returns a list of all registered users with their roles (Admin only)";
        });
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var users = await userManager.Users.ToListAsync(ct);
        var response = new ListUsersResponse();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            Response.Users.Add(new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }
    }
}
