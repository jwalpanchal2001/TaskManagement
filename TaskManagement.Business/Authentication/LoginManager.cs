using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TaskManagement.Data.Repository.Authentication;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;

namespace TaskManagement.Business.Authentication;

public class LoginManager : ILoginManager
{
    private readonly IAuthRepository _authRepository; private readonly IAuthManager _authManager;
    private readonly IPasswordHasher<TaskManagement.Entity.Model.User> _passwordHasher;

    public LoginManager(IAuthRepository authRepository, IAuthManager authManager , IPasswordHasher<TaskManagement.Entity.Model.User> passwordHasher)
    {
        _authRepository = authRepository;
        _authManager = authManager;
        _passwordHasher = passwordHasher;
    }
    public async Task<AuthResponse> LoginAsync(LoginRequestModel request)
    {
        var user = await _authRepository.GetUserByUsernameAsync(request.Username);

        if (user == null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) != PasswordVerificationResult.Success)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var accessToken = _authManager.GenerateJwtToken(user);  
        var refreshToken = _authManager.GenerateRefreshToken(user);

        await _authRepository.AddRefreshTokenAsync(refreshToken);
        await _authRepository.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            isAdmin = user.IsAdmin,
            ExpiresIn = refreshToken.ExpiresAt,
            UserId = user.Id
        };
    }

    public async Task<bool> LogoutAsync(string token)
    {
        var storedToken = await _authRepository.GetRefreshTokenAsync(token);
        if (storedToken != null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            // Optionally set RevokedByIp if you have access to the client's IP
            // storedToken.RevokedByIp = GetClientIp(); // Implement as needed
            await _authRepository.UpdateRefreshTokenAsync(storedToken);
            await _authRepository.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> RegisterAsync(RegisterRequestModel request)
    {
        if (await _authRepository.UsernameExistsAsync(request.Username))
            throw new InvalidOperationException("Username already taken.");

        var user = new TaskManagement.Entity.Model.User
        {
            Username = request.Username,
            FullName = request.FullName,
            IsAdmin = request.IsAdmin
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);


        await _authRepository.AddUserAsync(user);
        await _authRepository.SaveChangesAsync();

        return true;
    }



}
