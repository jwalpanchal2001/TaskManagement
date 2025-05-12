using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TaskManagement.Model.Api;

namespace TaskManagement.Service.BaseService
{
    public abstract class BaseServices
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl = "https://localhost:7154"; // 🔗 Directly set your API base URL here

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

        protected async Task<IFlurlRequest> GetFlurlRequestWithAutoRefreshAsync(string controller, string action)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
            var userId = _httpContextAccessor.HttpContext?.Request.Cookies["user_id"];
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Access token is missing.");

            var requestUrl = $"{_baseUrl}/api/{controller}/{action}";

            try
            {
                return requestUrl
                    .WithHeader("Authorization", $"Bearer {token}")
                    .WithTimeout(100);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401 && !string.IsNullOrEmpty(refreshToken))
            {
                // Attempt to refresh token
                var refreshResponse = await $"{_baseUrl}/api/login/refresh"
                 .PostJsonAsync(new { userId, refreshToken })
                 .ReceiveJson<TokenResponse>();

                var newAccessToken = refreshResponse.AccessToken;
                var newRefreshToken = refreshResponse.RefreshToken;

                // Update cookies
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

                // Retry the request with new token
                return requestUrl
                    .WithHeader("Authorization", $"Bearer {newAccessToken}")
                    .WithTimeout(100);
            }
            }



        // Optionally, you can add a method to handle token refresh or prompt re-login here
        protected void HandleTokenExpiry()
        {
            // Redirect the user to login page or show an appropriate message
            // e.g., _httpContextAccessor.HttpContext.RedirectToAction("Login", "Account");
            throw new UnauthorizedAccessException("Token has expired. Please log in again.");
        }

    }
}
