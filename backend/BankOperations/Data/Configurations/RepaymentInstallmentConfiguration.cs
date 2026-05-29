using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOperations.Data.Configurations;

public class RepaymentInstallmentConfiguration : IEntityTypeConfiguration<RepaymentInstallment>
{
    public void Configure(EntityTypeBuilder<RepaymentInstallment> builder)
    {
        builder.Property(ri => ri.PrincipalPart)
            .HasPrecision(18, 2);

        builder.Property(ri => ri.InterestPart)
            .HasPrecision(18, 2);

        builder.Property(ri => ri.RemainingBalance)
            .HasPrecision(18, 2);

        builder.HasOne(ri => ri.CreatedByUser)
            .WithMany()
            .HasForeignKey(ri => ri.CreatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
