using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;

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

        // Optionally, you can add a method to handle token refresh or prompt re-login here
        protected void HandleTokenExpiry()
        {
            // Redirect the user to login page or show an appropriate message
            // e.g., _httpContextAccessor.HttpContext.RedirectToAction("Login", "Account");
            throw new UnauthorizedAccessException("Token has expired. Please log in again.");
        }

    }
}
