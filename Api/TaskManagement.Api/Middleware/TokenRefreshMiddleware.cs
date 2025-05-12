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

    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(accessToken))
        {
            var isExpired = IsTokenExpired(accessToken);

            // Prevent multiple refreshes in the same request pipeline
            if (isExpired && !context.Items.ContainsKey("TokenRefreshed"))
            {
                var userId = GetUserIdFromToken(accessToken);

                if (!string.IsNullOrEmpty(userId))
                {
                    var newAccessToken = await GetNewAccessTokenAsync(userId, context);

                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        // Replace the expired token with the new one
                        context.Request.Headers["Authorization"] = "Bearer " + newAccessToken;

                        // Store in context to prevent re-refreshing during this request
                        context.Items["TokenRefreshed"] = true;
                    }
                }
            }
        }

        await _next(context);
    }

    private string? GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
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

        var requestObj = new { userId };
        var content = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("/api/login/refresh", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var newToken = doc.RootElement.GetProperty("accessToken").GetString();

                if (doc.RootElement.TryGetProperty("refreshToken", out var newRefreshToken))
                {
                    context.Response.Cookies.Append("refreshToken", newRefreshToken.GetString(), new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                }

                return newToken;
            }
        }
        catch
        {
            // Optional: log or handle failure
        }

        return null;
    }
}
