using ProteinOnWheelsAPI.Models;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace ProteinOnWheelsAPI.Data;

public static class DbSeeder
{
    public static void SeedAdmin(AppDbContext context,IConfiguration config)
    {

        // 🔥 DELETE OLD ADMINS
        context.Users.RemoveRange(context.Users.Where(u => u.Role == "Admin"));
        context.SaveChanges();

        var admin1Email = config["AdminSettings:Admin1Email"];
        var admin1Password = config["AdminSettings:Admin1Password"];

        var admin2Email = config["AdminSettings:Admin2Email"];
        var admin2Password = config["AdminSettings:Admin2Password"];
        // Admin 1
        if (!context.Users.Any(u => u.Email == admin1Email))
        {
            var admin1 = new User
            {
                Name = "Asit Singh",
                Email = admin1Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin1Password),
                Role = "Admin",
                IsEmailVerified = true
            };

            context.Users.Add(admin1);
        }

        // Admin 2
        if (!context.Users.Any(u => u.Email == admin2Email))
        {
            var admin2 = new User
            {
                Name = "Souvik Pradhan",
                Email = admin2Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin2Password),
                Role = "Admin",
                IsEmailVerified = true
            };

            context.Users.Add(admin2);
        }

        context.SaveChanges();
    }
}