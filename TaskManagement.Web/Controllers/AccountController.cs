using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TaskManagement.Model.Dto;
using TaskManagement.Model.ViewModel;
using TaskManagement.Service.Login;

namespace TaskManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService _loginService;

        public AccountController(ILoginService loginService)
        {
            _loginService = loginService;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
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

                if (tokenResult != null)
                {
                    HttpContext.Response.Cookies.Append("access_token", tokenResult.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        //Expires = DateTime.UtcNow.AddMinutes(15) 
                    });

                    HttpContext.Response.Cookies.Append("refresh_token", tokenResult.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        //Expires = DateTime.UtcNow.AddDays(7)
                    });

                    return Ok(new { success = true, message = "Login successful" , redirectUrl = "/admin/index" });
                }

                return BadRequest(new { success = false, message = "Invalid token response." });
            }

            return BadRequest(new { success = false, message = response.Message ?? "Invalid email or password." });
        }



    }
}
