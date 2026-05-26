using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Entities.Credits;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace BankOperations.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<IndividualClient> IndividualClients => Set<IndividualClient>();
    public DbSet<CorporateClient> CorporateClients => Set<CorporateClient>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<CreditService> CreditServices => Set<CreditService>();
    public DbSet<Credit> Credits => Set<Credit>();
    public DbSet<ConsumerCredit> ConsumerCredits => Set<ConsumerCredit>();
    public DbSet<MortgageCredit> MortgageCredits => Set<MortgageCredit>();
    public DbSet<RepaymentPlan> RepaymentPlans => Set<RepaymentPlan>();
    public DbSet<RepaymentInstallment> RepaymentInstallments => Set<RepaymentInstallment>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
