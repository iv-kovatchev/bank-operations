using BankOperations.Data;
using BankOperations.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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

        var employees = new[]
            {
                new { Email = "employee1@bank.com", FirstName = "Employee", LastName = "One" },
                new { Email = "employee2@bank.com", FirstName = "Employee", LastName = "Two" }
            };

        foreach (var emp in employees)
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

        var dummySeeded = await context.ActivityLogs
            .AnyAsync(al => al.Details != null && al.Details.StartsWith("[DUMMY]"));

        if (!dummySeeded)
        {
            var employee1 = await userManager.FindByEmailAsync("employee1@bank.com");
            var employee2 = await userManager.FindByEmailAsync("employee2@bank.com");

            if (employee1 != null && employee2 != null)
            {
                var now = DateTime.UtcNow;

                var dummyLogs = new List<ActivityLog>
                {
                    new()
                    {
                        UserId = admin.Id, Action = "CreateEmployee", EntityType = "Employee", EntityId = Guid.NewGuid(),
                        Timestamp = now, Details = "[DUMMY] Created employee employee1@bank.com"
                    },
                    new()
                    {
                        UserId = admin.Id, Action = "DeactivateEmployee", EntityType = "Employee", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddHours(-2), Details = "[DUMMY] Deactivated employee employee2@bank.com"
                    },
                    new()
                    {
                        UserId = admin2.Id, Action = "ActivateEmployee", EntityType = "Employee", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-3), Details = "[DUMMY] Activated employee employee2@bank.com"
                    },
                    new()
                    {
                        UserId = employee1.Id, Action = "CreateClient", EntityType = "Client", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-3).AddHours(-5), Details = "[DUMMY] Created individual client jdoe@example.com"
                    },
                    new()
                    {
                        UserId = employee1.Id, Action = "OpenAccount", EntityType = "BankAccount", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-3).AddHours(-6), Details = "[DUMMY] Opened account IBAN BG00TEST00000001"
                    },
                    new()
                    {
                        UserId = employee2.Id, Action = "Deposit", EntityType = "BankAccount", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-14), Details = "[DUMMY] Deposited 500.00 BGN"
                    },
                    new()
                    {
                        UserId = employee2.Id, Action = "GrantCredit", EntityType = "Credit", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-14).AddHours(-3), Details = "[DUMMY] Granted consumer credit of 2000.00 BGN"
                    },
                    new()
                    {
                        UserId = employee1.Id, Action = "CreateClient", EntityType = "Client", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-14).AddHours(-8), Details = "[DUMMY] Created corporate client acme@example.com"
                    },
                    new()
                    {
                        UserId = admin.Id, Action = "CreateEmployee", EntityType = "Employee", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-30), Details = "[DUMMY] Created employee jdoe.test@bank.com"
                    },
                    new()
                    {
                        UserId = employee2.Id, Action = "OpenAccount", EntityType = "BankAccount", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-30).AddHours(-4), Details = "[DUMMY] Opened account IBAN BG00TEST00000002"
                    },
                    new()
                    {
                        UserId = employee1.Id, Action = "Deposit", EntityType = "BankAccount", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-30).AddHours(-9), Details = "[DUMMY] Deposited 1200.50 BGN"
                    },
                    new()
                    {
                        UserId = admin2.Id, Action = "GrantCredit", EntityType = "Credit", EntityId = Guid.NewGuid(),
                        Timestamp = now.AddDays(-30).AddHours(-12), Details = "[DUMMY] Granted mortgage credit of 80000.00 BGN"
                    }
                };

                await context.ActivityLogs.AddRangeAsync(dummyLogs);
                await context.SaveChangesAsync();
            }
        }
    }
}