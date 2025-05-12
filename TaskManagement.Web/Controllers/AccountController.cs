using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskManagement.Model.Dto;
using TaskManagement.Model.ViewModel;
using TaskManagement.Service.Login;
using Flurl.Http;
using TaskManagement.Model.Api;

namespace TaskManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService _loginService;

        public AccountController(ILoginService loginService)
        {
            _loginService = loginService;
        }
        [HttpGet]
        public IActionResult Login(string? message = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.ErrorMessage = message;
            }
            return View();
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    try
        //    {
        //        var loginRequest = new LoginRequestModel
        //        {
        //            Username = model.Email,
        //            Password = model.Password
        //        };

        //        var response = await _loginService.ValidateLogin(loginRequest);


        //        if (response.IsSuccess)
        //        {
        //            var tokenResult = JsonConvert.DeserializeObject<AuthResponse>(response.Result.ToString());
        //            var resultObj = JObject.Parse(response.Result.ToString());
        //            bool isAdmin = resultObj["isAdmin"]?.ToObject<bool>() ?? false;

        //            // Store access token in session
        //            //HttpContext.Session.SetString("AccessToken", tokenResult.AccessToken);

        //            // Store refresh token in secure cookie (HttpOnly)
        //            Response.Cookies.Append("RefreshToken", tokenResult.RefreshToken, new CookieOptions
        //            {
        //                HttpOnly = true,
        //                Secure = true,
        //                Expires = DateTime.UtcNow.AddDays(7),
        //                SameSite = SameSiteMode.Strict
        //            });

        //            // Optionally use isAdmin to redirect
        //            if (isAdmin)
        //                return RedirectToAction("Index", "Admin");

        //            return RedirectToAction("Index", "User");
        //        }

        //        ModelState.AddModelError("", response.Message);
        //    }
        //    catch (FlurlHttpException ex)
        //    {
        //        ModelState.AddModelError("", "Login failed. Server error or invalid credentials.");
        //    }

        //    return View(model);
        //}




        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();

                return BadRequest(new { success = false, message = string.Join(" ", errors) });
            }

            var loginRequest = new LoginRequestModel
            {
                Username = model.Email,
                Password = model.Password
            };

            var response = await _loginService.ValidateLogin(loginRequest);

            if (response.IsSuccess && response.Result is not null)
            {
                var tokenResult = JsonConvert.DeserializeObject<AuthResponse>(response.Result.ToString());
                var resultObj = JObject.Parse(response.Result.ToString());
                bool isAdmin = resultObj["isAdmin"]?.ToObject<bool>() ?? false;

                if (tokenResult != null)
                {
                    // Create claims identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true // Set persistent cookie if needed
                    };

                    // Sign in user with claims
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Set tokens in cookies
                    HttpContext.Response.Cookies.Append("access_token", tokenResult.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                    });

                    HttpContext.Response.Cookies.Append("refresh_token", tokenResult.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(1)
                    });

                    var redirectUrl = isAdmin ? "/admin/index" : "/user/index";
                    return Ok(new { success = true, message = "Login successful", redirectUrl });
                }

                return BadRequest(new { success = false, message = "Invalid token response." });
            }

            return BadRequest(new { success = false, message = response.Message ?? "Invalid email or password." });
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }


    }
}
