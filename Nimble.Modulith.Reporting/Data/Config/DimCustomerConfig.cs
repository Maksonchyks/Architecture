using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data.Config;

public class DimCustomerConfig : IEntityTypeConfiguration<DimCustomer>
{
    public void Configure(EntityTypeBuilder<DimCustomer> builder)
    {
        builder.ToTable("DimCustomer");
        builder.HasKey(c => c.CustomerId);
        
        // Do NOT use identity - we control the IDs from source systems
        builder.Property(c => c.CustomerId)
            .ValueGeneratedNever();
            
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(100);
    }
}
