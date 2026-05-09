using Microsoft.EntityFrameworkCore;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data;

public class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<DimDate> Dates => Set<DimDate>();
    public DbSet<DimCustomer> Customers => Set<DimCustomer>();
    public DbSet<DimProduct> Products => Set<DimProduct>();
    public DbSet<FactOrder> FactOrders => Set<FactOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);
    }
}
