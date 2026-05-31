using BankOperations.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace BankOperations.Data.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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

        var adminEmail2 = "c0dyyy921@gmail.com";
        var admin = await userManager.FindByEmailAsync(adminEmail2);
        if (admin == null) return;

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
    }
}
