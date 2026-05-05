using DocSystem.Configuration;
using DocSystem.Dtos;
using DocSystem.Errors;
using DocSystem.Exceptions;
using DocSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocSystem.Services;

public class UserService(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    ILogger<UserService> logger)
{
    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Users
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.UserName)
            .ToListAsync(ct);
    }

    public async Task<User> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var userName = dto.UserName.Trim();

        var exists = await db.Users
            .AnyAsync(x => x.UserName == userName, ct);

        if (exists)
            throw new DomainException(new DomainError
            {
                Code = "user.username_already_exists",
                Message = "A user with this username already exists.",
                StatusCode = StatusCodes.Status409Conflict,
                Details = new
                {
                    userName
                },
                LogMessage = "Attempted to create a user with an existing username."
            });

        var user = new User
        {
            Name = dto.Name.Trim(),
            UserName = userName
        };

        user.Password = passwordHasher.HashPassword(user, dto.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "User created successfully. UserId: {UserId}; UserName: {UserName}",
            user.Id,
            user.UserName);

        return user;
    }
}
