using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Business.Authentication;
using TaskManagement.Data.Repository.Authentication;
using TaskManagement.Entity.Helper;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;

namespace TaskManagement.Api.Controllers;
[Route("api/[controller]")]

public class LoginController : Controller
{
    private readonly ILoginManager _loginManager;
    private readonly IAuthManager _authManager;
    private readonly IAuthRepository _authRepository;

    public LoginController(ILoginManager loginManager , IAuthManager authManager , IAuthRepository authRepository )
    {
        _loginManager = loginManager;
        _authManager = authManager;
        _authRepository = authRepository;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
    {
        try
        {
            var authResponse = await _loginManager.LoginAsync(request);

            var result = new
            {
                accessToken = authResponse.AccessToken,
                refreshToken = authResponse.RefreshToken,
                isAdmin = authResponse.isAdmin,
                userId = authResponse.UserId,

            };

            return Ok(new ApiResponseModel
            {
                IsSuccess = true,
                Message = "Login successful.",
                Result = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponseModel
            {
                IsSuccess = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseModel
            {
                IsSuccess = false,
                Message = "An unexpected error occurred: " + ex.Message
            });
        }
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        var result = await _loginManager.LogoutAsync(refreshToken);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestModel request)
    {
        var result = await _loginManager.RegisterAsync(request);
        return result ? Ok() : BadRequest();
    }


    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var user = await _authRepository.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            return Unauthorized();
        }

        var latestRefreshToken = await _authRepository.GetLatestRefreshTokenAsync(user.Id);

        var nowInIST = TimeZoneHelper.GetIndianTime();

        if (latestRefreshToken == null || latestRefreshToken.ExpiresAt <= nowInIST || !latestRefreshToken.IsActive)
        {
            return Unauthorized();
        }
        // Revoke old refresh token
        latestRefreshToken.RevokedAt = DateTime.UtcNow;
        await _authRepository.UpdateRefreshTokenAsync(latestRefreshToken);
       

        var newAccessToken = _authManager.GenerateJwtToken(user);
        var newRefreshToken = _authManager.GenerateRefreshToken(user);

        await _authRepository.AddRefreshTokenAsync(newRefreshToken);
        await _authRepository.SaveChangesAsync();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }


}
