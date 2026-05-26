using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOperations.Data.Configurations;

public class RepaymentPlanConfiguration : IEntityTypeConfiguration<RepaymentPlan>
{
    public void Configure(EntityTypeBuilder<RepaymentPlan> builder)
    {
        builder.Property(rp => rp.MonthlyInstallment)
            .HasPrecision(18, 2);

        builder.HasOne(rp => rp.Credit)
            .WithOne(c => c.RepaymentPlan)
            .HasForeignKey<RepaymentPlan>(rp => rp.CreditId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(rp => rp.Installments)
            .WithOne(ri => ri.RepaymentPlan)
            .HasForeignKey(ri => ri.RepaymentPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
