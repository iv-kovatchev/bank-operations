using BankOperations.Entities.Credits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOperations.Data.Configurations;

public class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    public void Configure(EntityTypeBuilder<Credit> builder)
    {
        builder.Property(c => c.Amount)
            .HasPrecision(18, 2);

        builder.HasOne(c => c.Client)
            .WithMany(cl => cl.Credits)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreditService)
            .WithMany(cs => cs.Credits)
            .HasForeignKey(c => c.CreditServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConsumerCreditConfiguration : IEntityTypeConfiguration<ConsumerCredit>
{
    public void Configure(EntityTypeBuilder<ConsumerCredit> builder)
    {
        builder.ToTable("ConsumerCredits");
    }
}

public class MortgageCreditConfiguration : IEntityTypeConfiguration<MortgageCredit>
{
    public void Configure(EntityTypeBuilder<MortgageCredit> builder)
    {
        builder.ToTable("MortgageCredits");
    }
}
