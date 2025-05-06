namespace TaskManagement.Api.Middleware;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    //public async Task InvokeAsync(HttpContext context)
    //{
    //    var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

    //    if (!string.IsNullOrEmpty(accessToken) && IsTokenExpired(accessToken))
    //    {
    //        // Get refresh token from cookie or header (choose your method)
    //        var refreshToken = context.Request.Cookies["refreshToken"];
    //        if (string.IsNullOrEmpty(refreshToken))
    //        {
    //            // You can also fetch from headers if you're not using cookies
    //            refreshToken = context.Request.Headers["X-Refresh-Token"];
    //        }

    //        if (!string.IsNullOrEmpty(refreshToken))
    //        {
    //            var newAccessToken = await GetNewAccessTokenAsync(refreshToken, context);

    //            if (!string.IsNullOrEmpty(newAccessToken))
    //            {
    //                // Replace the old access token in the request header
    //                context.Request.Headers["Authorization"] = "Bearer " + newAccessToken;
    //            }
    //        }
    //    }

    //    await _next(context); // Proceed to the next middleware
    //}

    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(accessToken) && IsTokenExpired(accessToken))
        {
            var userId = GetUserIdFromToken(accessToken);

            if (!string.IsNullOrEmpty(userId))
            {
                var newAccessToken = await GetNewAccessTokenAsync(userId, context);

                if (!string.IsNullOrEmpty(newAccessToken))
                {
                    context.Request.Headers["Authorization"] = "Bearer " + newAccessToken;
                }
            }
        }

        await _next(context);
    }

    private string? GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "id");
        return userIdClaim?.Value;
    }

    private bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        return jwt.ValidTo < DateTime.UtcNow;
    }

    private async Task<string?> GetNewAccessTokenAsync(string userId, HttpContext context)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri($"{context.Request.Scheme}://{context.Request.Host}");

        var requestObj = new { userId = userId };
        var content = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("/api/login/refresh", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var newToken = doc.RootElement.GetProperty("accessToken").GetString();

                var newRefresh = doc.RootElement.TryGetProperty("refreshToken", out var newRefreshToken)
                    ? newRefreshToken.GetString()
                    : null;

                if (!string.IsNullOrEmpty(newRefresh))
                {
                    context.Response.Cookies.Append("refreshToken", newRefresh,
                        new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
                }

                return newToken;
            }
        }
        catch
        {
            // Handle error
        }

        return null;
    }

    //private async Task<string?> GetNewAccessTokenAsync(string refreshToken, HttpContext context)
    //{
    //    using var client = new HttpClient();

    //    // Use base address from the current request
    //    client.BaseAddress = new Uri($"{context.Request.Scheme}://{context.Request.Host}");

    //    var requestObj = new { refreshToken = refreshToken };
    //    var content = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");

    //    try
    //    {
    //        var response = await client.PostAsync("/api/login/refresh", content);
    //        if (response.IsSuccessStatusCode)
    //        {
    //            var responseString = await response.Content.ReadAsStringAsync();
    //            using var doc = JsonDocument.Parse(responseString);
    //            var newToken = doc.RootElement.GetProperty("accessToken").GetString();

    //            // Optionally update refresh token cookie if you return a new one
    //            var newRefresh = doc.RootElement.TryGetProperty("refreshToken", out var newRefreshToken)
    //                ? newRefreshToken.GetString()
    //                : null;

    //            if (!string.IsNullOrEmpty(newRefresh))
    //            {
    //                context.Response.Cookies.Append("refreshToken", newRefresh,
    //                    new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });
    //            }

    //            return newToken;
    //        }
    //    }
    //    catch
    //    {
    //        return "Error here";
    //    }

    //    return null;
    //}
}
