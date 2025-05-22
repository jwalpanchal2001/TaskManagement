using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;

namespace TaskManagement.Service.BaseService;

public abstract class BaseServices
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl = "https://localhost:7154"; 

    protected BaseServices(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // For requests that do NOT require token (e.g., Login)
    protected IFlurlRequest GetFlurlRequestWithoutToken(string controller, string action)
    {
        return $"{_baseUrl}/api/{controller}/{action}".WithTimeout(100);
    }

    // For authenticated requests with Bearer token
    protected IFlurlRequest GetFlurlRequestWithToken(string controller, string action)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];

        // Check if the token is missing or expired
        if (string.IsNullOrEmpty(token))
        {
            // You can throw a specific exception or handle this gracefully
            throw new UnauthorizedAccessException("Access token is missing or expired.");
        }

        // Return the request with Bearer token
        return $"{_baseUrl}/api/{controller}/{action}"
        .WithHeader("Authorization", $"Bearer {token}");

    }

    protected async Task<T> GetJsonWithAutoRefreshAsync<T>(string controller, string actionWithQuery)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
        //var userId = _httpContextAccessor.HttpContext?.Request.Cookies["user_id"];
        var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];

        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Access token is missing.");

        var requestUrl = $"{_baseUrl}/api/{controller}/{actionWithQuery}";

        try
        {
            return await requestUrl
                .WithHeader("Authorization", $"Bearer {token}")
                .WithTimeout(100)
                .GetJsonAsync<T>();
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401 && !string.IsNullOrEmpty(refreshToken))
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Request.Cookies["user_id"];
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Missing user ID.");

            var refreshResponse = await $"{_baseUrl}/api/login/refresh"
                .PostJsonAsync(new RefreshTokenRequest { UserId = userId })
                .ReceiveJson<TokenResponse>();

            var newAccessToken = refreshResponse.AccessToken;
            var newRefreshToken = refreshResponse.RefreshToken;

            var response = _httpContextAccessor.HttpContext?.Response;
            if (response != null)
            {
                response.Cookies.Append("access_token", newAccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
            }

            // Retry the original request with the new token
            return await requestUrl
                .WithHeader("Authorization", $"Bearer {newAccessToken}")
                .WithTimeout(100)
                .GetJsonAsync<T>();
        }
    }


    protected async Task<T> SendAuthorizedRequestAsync<T>(string controller, string action,
       Func<IFlurlRequest, Task<T>> requestFunc)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
        var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];

        if (string.IsNullOrEmpty(token))
            throw new UnauthorizedAccessException("Access token is missing.");

        var url = $"{_baseUrl}/api/{controller}/{action}";
        var flurlRequest = url.WithHeader("Authorization", $"Bearer {token}").WithTimeout(100);

        try
        {
            return await requestFunc(flurlRequest);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401 && !string.IsNullOrEmpty(refreshToken))
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Request.Cookies["user_id"];
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Missing user ID.");

            try
            {
                var refreshResponse = await $"{_baseUrl}/api/login/refresh"
                    .PostJsonAsync(new RefreshTokenRequest { UserId = userId })
                    .ReceiveJson<TokenResponse>();

                var newAccessToken = refreshResponse.AccessToken;
                var newRefreshToken = refreshResponse.RefreshToken;

                var response = _httpContextAccessor.HttpContext?.Response;
                if (response != null)
                {
                    response.Cookies.Append("access_token", newAccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });

                    response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                }

                var retryRequest = url.WithHeader("Authorization", $"Bearer {newAccessToken}").WithTimeout(100);
                return await requestFunc(retryRequest);
            }
            catch (FlurlHttpException exp) when (exp.Call.Response?.StatusCode == 401)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
        }
    }




    // Optionally, you can add a method to handle token refresh or prompt re-login here
    protected void HandleTokenExpiry()
    {
        throw new UnauthorizedAccessException("Token has expired. Please log in again.");
    }

}
