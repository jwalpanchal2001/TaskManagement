using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Dto;

namespace TaskManagement.Business.Authentication;

public interface ILoginManager 
{
    Task<AuthResponse> LoginAsync(LoginRequestModel request);
    Task<bool> LogoutAsync(string token);
    Task<bool> RegisterAsync(RegisterRequestModel request); 
}
