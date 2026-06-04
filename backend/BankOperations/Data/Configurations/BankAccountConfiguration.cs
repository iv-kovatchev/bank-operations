using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOperations.Data.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.Property(ba => ba.Balance)
            .HasPrecision(18, 2);

        builder.HasOne(ba => ba.Client)
            .WithMany(c => c.BankAccounts)
            .HasForeignKey(ba => ba.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ba => ba.CreatedByUser)
            .WithMany()
            .HasForeignKey(ba => ba.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ba => ba.IBAN).IsUnique();
    }
}
