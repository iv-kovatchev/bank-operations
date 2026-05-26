using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOperations.Data.Configurations;

public class CreditServiceConfiguration : IEntityTypeConfiguration<CreditService>
{
    public void Configure(EntityTypeBuilder<CreditService> builder)
    {
        builder.Property(cs => cs.InterestRate)
            .HasPrecision(5, 2);

        builder.Property(cs => cs.MaxAmount)
            .HasPrecision(18, 2);
    }
}
