using Microsoft.Extensions.Configuration; // For IConfiguration to read JWT settings
using Microsoft.IdentityModel.Tokens;
using ParkShare.Application.DTOs.Auth;
using ParkShare.Application.Interfaces;
using ParkShare.Core.Entities; // For User entity
using ParkShare.Infrastructure.Data; // For ParkShareDbContext
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Required for FirstOrDefaultAsync

namespace ParkShare.Application.Services;

public class AuthService : IAuthService
{
    private readonly ParkShareDbContext _dbContext;
    private readonly IConfiguration _configuration; 

    public AuthService(ParkShareDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<UserDto> GetOrCreateUserAsync(GoogleUserInfoDto googleUserInfo)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == googleUserInfo.Email);

        if (user == null)
        {
            user = new User
            {
                // Id = Guid.NewGuid().ToString(), // Core.User.Id is Guid, not string
                Id = Guid.NewGuid(),
                Email = googleUserInfo.Email,
                FirstName = googleUserInfo.FirstName,
                LastName = googleUserInfo.LastName,
                RegisteredAtUtc = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            bool updated = false;
            if (user.FirstName != googleUserInfo.FirstName) { user.FirstName = googleUserInfo.FirstName; updated = true; }
            if (user.LastName != googleUserInfo.LastName) { user.LastName = googleUserInfo.LastName; updated = true; }
            
            if (updated)
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        return new UserDto
        {
            Id = user.Id.ToString(), // UserDto.Id is string
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty
        };
    }

    public string GenerateJwtToken(UserDto user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var jwtDuration = _configuration["Jwt:DurationInMinutes"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) || string.IsNullOrEmpty(jwtDuration))
        {
            throw new InvalidOperationException("JWT settings (Key, Issuer, Audience, DurationInMinutes) are not configured properly in appsettings.json");
        }
        if (!int.TryParse(jwtDuration, out int durationInMinutes))
        {
            throw new InvalidOperationException("JWT DurationInMinutes is not a valid integer in appsettings.json");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), 
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
