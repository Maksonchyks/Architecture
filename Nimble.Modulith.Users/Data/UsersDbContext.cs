using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Nimble.Modulith.Users.Data;

public class UsersDbContext : IdentityDbContext<IdentityUser>
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Auto-discover all IEntityTypeConfiguration<T> — picks up IdentityRoleConfig for role seeding
        builder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}
