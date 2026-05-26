using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReactApp1.Server.Data;

namespace ReactApp1.Server.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            // Ensure required roles
            string[] roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Use configured credentials when present; otherwise fall back to a default admin so first deploy is usable.
            var adminEmail = config["Admin:Email"];
            var adminPassword = config["Admin:Password"];
            adminEmail = string.IsNullOrWhiteSpace(adminEmail) ? "admin@gmail.com" : adminEmail;
            adminPassword = string.IsNullOrWhiteSpace(adminPassword) ? "Admin123." : adminPassword;

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