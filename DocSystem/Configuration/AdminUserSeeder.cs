using DocSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocSystem.Configuration;

public static class AdminUserSeeder
{
    private const string AdminName = "Admin";

    public static void Seed(DbContext context, IConfiguration configuration)
    {
        var settings = GetSettings(configuration);
        var db = (AppDbContext)context;

        var existingUser = db.Users.SingleOrDefault(x => x.UserName == settings.UserName);
        var passwordHasher = new PasswordHasher<User>();

        if (existingUser is null)
        {
            var user = new User
            {
                Name = AdminName,
                UserName = settings.UserName
            };

            user.Password = passwordHasher.HashPassword(user, settings.Password);

            db.Users.Add(user);
        }
        else
        {
            existingUser.Name = AdminName;
            existingUser.Password = passwordHasher.HashPassword(existingUser, settings.Password);
            db.Users.Update(existingUser);
        }

        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, IConfiguration configuration, CancellationToken ct = default)
    {
        var settings = GetSettings(configuration);
        var db = (AppDbContext)context;

        var existingUser = await db.Users.SingleOrDefaultAsync(x => x.UserName == settings.UserName, ct);
        var passwordHasher = new PasswordHasher<User>();

        if (existingUser is null)
        {
            var user = new User
            {
                Name = AdminName,
                UserName = settings.UserName
            };

            user.Password = passwordHasher.HashPassword(user, settings.Password);

            await db.Users.AddAsync(user, ct);
        }
        else
        {
            existingUser.Name = AdminName;
            existingUser.Password = passwordHasher.HashPassword(existingUser, settings.Password);
            db.Users.Update(existingUser);
        }

        await db.SaveChangesAsync(ct);
    }

    private static AdminUserSettings GetSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection("AdminUser");
        var userName = section["UserName"]?.Trim();
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(userName))
            throw new InvalidOperationException("AdminUser:UserName is not configured.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("AdminUser:Password is not configured.");

        return new AdminUserSettings
        {
            UserName = userName,
            Password = password
        };
    }

    private sealed class AdminUserSettings
    {
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
