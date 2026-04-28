using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DocSystem.Configuration;
using DocSystem.Dtos;
using DocSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserContract = DocSystem.TypeContracts.User;

namespace DocSystem.Services;

public class AuthService(
    AppDbContext db,
    IConfiguration configuration,
    IPasswordHasher<User> passwordHasher)
{
    public async Task<LoginResultDto?> Login(LoginDto dto, CancellationToken ct = default)
    {
        var userName = dto.UserName.Trim();

        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserName == userName, ct);

        if (user is null)
            return null;

        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return null;

        var expiresAt = DateTime.UtcNow.AddMinutes(GetExpirationMinutes());

        return new LoginResultDto
        {
            Token = GenerateJwtToken(user, expiresAt),
            ExpiresAt = expiresAt,
            User = MapUser(user)
        };
    }

    public string GenerateJwtToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(GetExpirationMinutes());

        return GenerateJwtToken(user, expiresAt);
    }

    private string GenerateJwtToken(User user, DateTime expiresAt)
    {
        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(GetSecretKey()));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Sid, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private double GetExpirationMinutes()
    {
        return configuration.GetValue<double?>("Jwt:ExpirationMinutes") ?? 60;
    }

    private string GetSecretKey()
    {
        var secretKey = configuration["Jwt:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("JWT secret key is not configured.");

        return secretKey;
    }

    private static UserContract MapUser(User user)
    {
        return new UserContract
        {
            Id = user.Id,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Name = user.Name,
            UserName = user.UserName
        };
    }
}
