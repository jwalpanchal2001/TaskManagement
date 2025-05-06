using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Data.Repository.Authentication;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;

namespace TaskManagement.Business.Authentication;

public class AuthManager : IAuthManager
{
    private readonly JwtSettings _jwtSettings;
    private readonly IAuthRepository _authRepository;

    public AuthManager(IOptions<JwtSettings> options , IAuthRepository authRepository)
    {
        _jwtSettings = options.Value;
        _authRepository = authRepository;
    }

    public string GenerateJwtToken(TaskManagement.Entity.Model.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
           {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim("id", user.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Add this line
        new Claim("isAdmin", user.IsAdmin.ToString())
    };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(TaskManagement.Entity.Model.User user)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };
    }

    public async Task<TaskManagement.Entity.Model.User> ValidateRefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);
        if (storedToken == null || !storedToken.IsActive)
            return null;

        return storedToken.User;
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(string refreshToken)
    {
        var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);
        if (storedToken == null || !storedToken.IsActive)
            throw new SecurityTokenException("Invalid or expired refresh token");

        var user = storedToken.User;

        // Optionally revoke the old token (mark as used)
        storedToken.RevokedAt = DateTime.UtcNow;
        await _authRepository.UpdateRefreshTokenAsync(storedToken);

        // Generate new tokens
        var newAccessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken(user);

        await _authRepository.AddRefreshTokenAsync(newRefreshToken);
        await _authRepository.SaveChangesAsync();

        return (newAccessToken, newRefreshToken.Token);
    }

}

