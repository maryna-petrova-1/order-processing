using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderProcessing.Domain.Models;

namespace OrderProcessing.Infrastructure;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Client)
            .HasForeignKey(o => o.ClientId);
    }
}
