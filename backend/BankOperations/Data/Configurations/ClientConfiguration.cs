using BankOperations.Entities.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOperations.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.ClientId);

        builder.HasOne(c => c.User)
            .WithOne()
            .HasForeignKey<Client>(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class IndividualClientConfiguration : IEntityTypeConfiguration<IndividualClient>
{
    public void Configure(EntityTypeBuilder<IndividualClient> builder)
    {
        builder.ToTable("IndividualClients");
    }
}

public class CorporateClientConfiguration : IEntityTypeConfiguration<CorporateClient>
{
    public void Configure(EntityTypeBuilder<CorporateClient> builder)
    {
        builder.ToTable("CorporateClients");
    }
}
