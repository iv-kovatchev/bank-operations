using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Entities.Credits;
using BankOperations.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Data.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        string[] roles = ["Admin", "Employee", "Client"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        var adminEmail = "c0dyyy921@gmail.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(newAdmin, "Admin123!");
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null) return;

        var adminEmail3 = "i.petarivanov03@gmail.com";
        if (await userManager.FindByEmailAsync(adminEmail3) is null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail3,
                Email = adminEmail3,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(newAdmin, "Admin123!");
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }

        var admin2 = await userManager.FindByEmailAsync(adminEmail3);
        if (admin2 == null) return;

        var employeeDefs = new[]
        {
            new { Email = "employee1@bank.com", FirstName = "Employee", LastName = "One" },
            new { Email = "employee2@bank.com", FirstName = "Employee", LastName = "Two" },
            new { Email = "employee3@bank.com", FirstName = "Employee", LastName = "Three" },
            new { Email = "employee4@bank.com", FirstName = "Employee", LastName = "Four" },
            new { Email = "employee5@bank.com", FirstName = "Employee", LastName = "Five" }
        };

        foreach (var emp in employeeDefs)
        {
            var existing = await userManager.FindByEmailAsync(emp.Email);
            if (existing == null)
            {
                var user = new ApplicationUser
                {
                    Email = emp.Email,
                    UserName = emp.Email,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    EmailConfirmed = true,
                    IsActive = true
                };
                await userManager.CreateAsync(user, "Employee@123");
                await userManager.AddToRoleAsync(user, "Employee");
            }
        }

        await SeedCreditServicesAsync(context);
        await SeedClientsAccountsAndCreditsAsync(context, userManager, employeeDefs.Select(e => e.Email).ToArray());
    }

    private static async Task SeedCreditServicesAsync(ApplicationDbContext context)
    {
        var creditServiceDefs = new[]
        {
            new { Name = "Стандартен потребителски", Type = CreditType.Consumer, InterestRate = 8.5m, MaxAmount = 50000m, MaxTermMonths = 84 },
            new { Name = "Експресен потребителски", Type = CreditType.Consumer, InterestRate = 12m, MaxAmount = 10000m, MaxTermMonths = 36 },
            new { Name = "Стандартна ипотека", Type = CreditType.Mortgage, InterestRate = 3.75m, MaxAmount = 500000m, MaxTermMonths = 360 },
            new { Name = "Ипотека 20г", Type = CreditType.Mortgage, InterestRate = 4.25m, MaxAmount = 300000m, MaxTermMonths = 240 }
        };

        foreach (var def in creditServiceDefs)
        {
            var exists = await context.CreditServices.AnyAsync(cs => cs.Name == def.Name);
            if (!exists)
            {
                context.CreditServices.Add(new CreditService
                {
                    Name = def.Name,
                    Type = def.Type,
                    InterestRate = def.InterestRate,
                    MaxAmount = def.MaxAmount,
                    MaxTermMonths = def.MaxTermMonths
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedClientsAccountsAndCreditsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        string[] employeeEmails)
    {
        var employees = new ApplicationUser[employeeEmails.Length];
        for (int i = 0; i < employeeEmails.Length; i++)
        {
            employees[i] = await userManager.FindByEmailAsync(employeeEmails[i])
                ?? throw new InvalidOperationException($"Employee {employeeEmails[i]} was not seeded.");
        }

        var clientSeeds = new[]
        {
            new ClientSeedDef(0, true, "ivan.petrov1@example.com", "Иван", "Петров", "9001011234"),
            new ClientSeedDef(0, true, "maria.georgieva1@example.com", "Мария", "Георгиева", "9205154321"),
            new ClientSeedDef(0, true, "georgi.ivanov1@example.com", "Георги", "Иванов", "8811223344"),
            new ClientSeedDef(0, true, "elena.todorova1@example.com", "Елена", "Тодорова", "9512301111"),
            new ClientSeedDef(0, false, "alpha.ood1@example.com", CompanyName: "Алфа ООД", Eik: "123456789", RepFirstName: "Петър", RepLastName: "Стоянов"),
            new ClientSeedDef(0, false, "beta.eood1@example.com", CompanyName: "Бета ЕООД", Eik: "987654321", RepFirstName: "Яна", RepLastName: "Димитрова"),

            new ClientSeedDef(1, true, "stefan.dimitrov2@example.com", "Стефан", "Димитров", "8703127890"),
            new ClientSeedDef(1, true, "petya.nikolova2@example.com", "Петя", "Николова", "9011059988"),
            new ClientSeedDef(1, true, "dimitar.hristov2@example.com", "Димитър", "Христов", "7705141234"),
            new ClientSeedDef(1, true, "silvia.angelova2@example.com", "Силвия", "Ангелова", "9402285566"),
            new ClientSeedDef(1, false, "gamma.ad2@example.com", CompanyName: "Гама АД", Eik: "112233445", RepFirstName: "Николай", RepLastName: "Петков"),
            new ClientSeedDef(1, false, "delta.ood2@example.com", CompanyName: "Делта ООД", Eik: "556677889", RepFirstName: "Боряна", RepLastName: "Илиева"),

            new ClientSeedDef(2, true, "kalin.yordanov3@example.com", "Калин", "Йорданов", "8506184321"),
            new ClientSeedDef(2, true, "vasilka.stoyanova3@example.com", "Василка", "Стоянова", "9108223344"),
            new ClientSeedDef(2, true, "hristo.marinov3@example.com", "Христо", "Маринов", "7902117654"),
            new ClientSeedDef(2, true, "tsvetelina.koleva3@example.com", "Цветелина", "Колева", "9609099988"),
            new ClientSeedDef(2, false, "epsilon.eood3@example.com", CompanyName: "Епсилон ЕООД", Eik: "223344556", RepFirstName: "Пламен", RepLastName: "Йотов"),
            new ClientSeedDef(2, false, "zita.ood3@example.com", CompanyName: "Зита ООД", Eik: "667788990", RepFirstName: "Диана", RepLastName: "Младенова"),

            new ClientSeedDef(3, true, "yordan.kostov4@example.com", "Йордан", "Костов", "8412055678"),
            new ClientSeedDef(3, true, "iliana.petkova4@example.com", "Илиана", "Петкова", "9307149999"),
            new ClientSeedDef(3, true, "rumen.angelov4@example.com", "Румен", "Ангелов", "7601233344"),
            new ClientSeedDef(3, true, "boryana.hristova4@example.com", "Боряна", "Христова", "9505056677"),
            new ClientSeedDef(3, false, "eta.ad4@example.com", CompanyName: "Ета АД", Eik: "334455667", RepFirstName: "Тодор", RepLastName: "Маринов"),
            new ClientSeedDef(3, false, "theta.ood4@example.com", CompanyName: "Тита ООД", Eik: "778899001", RepFirstName: "Галина", RepLastName: "Стоева"),

            new ClientSeedDef(4, true, "nikola.stoev5@example.com", "Никола", "Стоев", "8809124321"),
            new ClientSeedDef(4, true, "albena.yoncheva5@example.com", "Албена", "Йончева", "9204187654"),
            new ClientSeedDef(4, true, "emil.genchev5@example.com", "Емил", "Генчев", "7707095678"),
            new ClientSeedDef(4, true, "mariela.dobreva5@example.com", "Мариела", "Добрева", "9601019999"),
            new ClientSeedDef(4, false, "iota.eood5@example.com", CompanyName: "Йота ЕООД", Eik: "445566778", RepFirstName: "Стоян", RepLastName: "Илиев"),
            new ClientSeedDef(4, false, "kappa.ood5@example.com", CompanyName: "Каппа ООД", Eik: "889900112", RepFirstName: "Веселина", RepLastName: "Тонева")
        };

        // Pass 1 — clients (AspNetUsers + IndividualClient/CorporateClient)
        var clientIdsByEmail = new Dictionary<string, Guid>();

        foreach (var def in clientSeeds)
        {
            var existingUser = await userManager.FindByEmailAsync(def.Email);
            if (existingUser != null)
            {
                clientIdsByEmail[def.Email] = existingUser.Id;
                continue;
            }

            var employee = employees[def.EmployeeIndex];
            var user = new ApplicationUser
            {
                Email = def.Email,
                UserName = def.Email,
                FirstName = def.IsIndividual ? def.FirstName! : def.RepFirstName!,
                LastName = def.IsIndividual ? def.LastName! : def.RepLastName!,
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(user, "Client@123");
            await userManager.AddToRoleAsync(user, "Client");

            if (def.IsIndividual)
            {
                context.IndividualClients.Add(new IndividualClient
                {
                    ClientId = user.Id,
                    FirstName = def.FirstName!,
                    LastName = def.LastName!,
                    EGN = def.Egn!,
                    CreatedByUserId = employee.Id
                });
            }
            else
            {
                context.CorporateClients.Add(new CorporateClient
                {
                    ClientId = user.Id,
                    CompanyName = def.CompanyName!,
                    EIK = def.Eik!,
                    RepresentativeFirstName = def.RepFirstName!,
                    RepresentativeLastName = def.RepLastName!,
                    CreatedByUserId = employee.Id
                });
            }

            clientIdsByEmail[def.Email] = user.Id;
        }

        await context.SaveChangesAsync();

        // Pass 2 — bank accounts (2-3 per client)
        int ibanCounter = 1;

        for (int sequence = 0; sequence < clientSeeds.Length; sequence++)
        {
            var def = clientSeeds[sequence];
            var clientId = clientIdsByEmail[def.Email];
            var employee = employees[def.EmployeeIndex];

            int accountCount = sequence % 2 == 0 ? 2 : 3;
            for (int a = 0; a < accountCount; a++)
            {
                string iban = $"BG{10 + (ibanCounter % 90):D2}BANK{ibanCounter:D14}";
                var exists = await context.BankAccounts.AnyAsync(ba => ba.IBAN == iban);
                if (!exists)
                {
                    decimal balance = 1000m + (ibanCounter * 733 % 49000);
                    context.BankAccounts.Add(new BankAccount
                    {
                        IBAN = iban,
                        Balance = balance,
                        ClientId = clientId,
                        CreatedByUserId = employee.Id
                    });
                }
                ibanCounter++;
            }
        }

        await context.SaveChangesAsync();

        // Pass 3 — credits (2-3 per client) + repayment plans
        var consumerServiceStandard = await context.CreditServices.FirstAsync(cs => cs.Name == "Стандартен потребителски");
        var consumerServiceExpress = await context.CreditServices.FirstAsync(cs => cs.Name == "Експресен потребителски");
        var mortgageServiceStandard = await context.CreditServices.FirstAsync(cs => cs.Name == "Стандартна ипотека");
        var mortgageService20y = await context.CreditServices.FirstAsync(cs => cs.Name == "Ипотека 20г");

        for (int sequence = 0; sequence < clientSeeds.Length; sequence++)
        {
            var def = clientSeeds[sequence];
            var clientId = clientIdsByEmail[def.Email];
            var employee = employees[def.EmployeeIndex];

            var hasCredits = await context.Credits.AnyAsync(c => c.ClientId == clientId);
            if (hasCredits) continue;

            int creditCount = sequence % 2 == 0 ? 2 : 3;
            for (int c = 0; c < creditCount; c++)
            {
                bool isMortgage = c % 2 == 1;
                int stateIndex = (sequence + c) % 3; // 0 = fully paid, 1 = half paid, 2 = new

                Credit credit;
                CreditService creditServiceEntity;
                decimal amount;
                int termMonths;

                if (isMortgage)
                {
                    creditServiceEntity = c == 1 ? mortgageServiceStandard : mortgageService20y;
                    amount = Math.Min(80000m + 20000m * c, creditServiceEntity.MaxAmount - 1000m);
                    termMonths = Math.Min(180 + 12 * c, creditServiceEntity.MaxTermMonths - 1);
                    credit = new MortgageCredit
                    {
                        ClientId = clientId,
                        CreditServiceId = creditServiceEntity.Id,
                        Amount = amount,
                        TermMonths = termMonths,
                        CreatedByUserId = employee.Id,
                        PropertyAddress = $"ул. Витоша {10 + sequence}, гр. София",
                        PropertyType = c % 2 == 0 ? PropertyType.Apartment : PropertyType.House
                    };
                }
                else
                {
                    creditServiceEntity = c == 0 ? consumerServiceStandard : consumerServiceExpress;
                    amount = Math.Min(5000m + 1500m * c, creditServiceEntity.MaxAmount - 500m);
                    termMonths = Math.Min(24 + 6 * c, creditServiceEntity.MaxTermMonths - 1);
                    credit = new ConsumerCredit
                    {
                        ClientId = clientId,
                        CreditServiceId = creditServiceEntity.Id,
                        Amount = amount,
                        TermMonths = termMonths,
                        CreatedByUserId = employee.Id,
                        Purpose = (CreditPurpose)(c % 4)
                    };
                }

                int paidInstallments = stateIndex switch
                {
                    0 => termMonths,
                    1 => termMonths / 2,
                    _ => 0
                };
                credit.Status = paidInstallments == termMonths ? CreditStatus.PaidOff : CreditStatus.Active;

                context.Credits.Add(credit);

                var plan = GenerateRepaymentPlan(credit.Id, amount, termMonths, creditServiceEntity.InterestRate, paidInstallments, employee.Id);
                context.RepaymentPlans.Add(plan);
            }
        }

        await context.SaveChangesAsync();
    }

    private static RepaymentPlan GenerateRepaymentPlan(
        Guid creditId, decimal amount, int termMonths, decimal interestRate, int paidInstallmentsCount, Guid paidByUserId)
    {
        decimal monthlyRate = interestRate / 100 / 12;
        decimal monthlyInstallment = amount *
            (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), termMonths)) /
            ((decimal)Math.Pow((double)(1 + monthlyRate), termMonths) - 1);

        monthlyInstallment = Math.Round(monthlyInstallment, 2);

        var plan = new RepaymentPlan
        {
            CreditId = creditId,
            MonthlyInstallment = monthlyInstallment,
            GeneratedAt = DateTime.UtcNow
        };

        decimal remainingBalance = amount;
        var installments = new List<RepaymentInstallment>();
        int monthOffset = paidInstallmentsCount;

        for (int i = 1; i <= termMonths; i++)
        {
            decimal interest = Math.Round(remainingBalance * monthlyRate, 2);
            decimal principal = Math.Round(monthlyInstallment - interest, 2);

            if (i == termMonths)
                principal = remainingBalance;

            remainingBalance = Math.Round(remainingBalance - principal, 2);

            var dueDate = DateTime.UtcNow.AddMonths(i - monthOffset);
            bool isPaid = i <= paidInstallmentsCount;

            installments.Add(new RepaymentInstallment
            {
                RepaymentPlanId = plan.Id,
                InstallmentNumber = i,
                DueDate = dueDate,
                PrincipalPart = principal,
                InterestPart = interest,
                RemainingBalance = remainingBalance,
                PaidAt = isPaid ? dueDate : null,
                CreatedByUserId = isPaid ? paidByUserId : null
            });
        }

        plan.Installments = installments;
        return plan;
    }

    private sealed record ClientSeedDef(
        int EmployeeIndex,
        bool IsIndividual,
        string Email,
        string? FirstName = null,
        string? LastName = null,
        string? Egn = null,
        string? CompanyName = null,
        string? Eik = null,
        string? RepFirstName = null,
        string? RepLastName = null);
}
