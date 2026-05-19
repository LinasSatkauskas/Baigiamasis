using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactApp1.Server.Data;

namespace ReactApp1.Server.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<AppDbContext>();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var config = services.GetRequiredService<IConfiguration>();
            var env = services.GetRequiredService<IHostEnvironment>();

            // Ensure required roles
            string[] roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Read admin credentials from configuration
            var adminEmail = config["Admin:Email"];
            var adminPassword = config["Admin:Password"];

            // Provide a Development fallback if not configured
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                if (env.IsDevelopment())
                {
                    adminEmail = adminEmail ?? "admin@local";
                    adminPassword = adminPassword ?? "Passw0rd!";
                }
                else
                {
                    // In non-dev, do not auto-create an admin without explicit credentials
                    return;
                }
            }

            // Create or update admin user
            var user = await userManager.FindByEmailAsync(adminEmail!);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var create = await userManager.CreateAsync(user, adminPassword!);
                if (!create.Succeeded)
                {
                    // If user creation failed due to password policy etc., stop here
                    return;
                }
            }

            // Ensure admin is in Admin role
            if (!await userManager.IsInRoleAsync(user, "Admin"))
                await userManager.AddToRoleAsync(user, "Admin");

            // Do NOT remove other users on startup — keep any users created at runtime.
            // Previous behavior deleted all non-admin users each startup which
            // caused newly created users to disappear after a server restart.
        }
    }
}