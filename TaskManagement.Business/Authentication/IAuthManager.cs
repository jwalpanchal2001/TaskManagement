using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;

namespace TaskManagement.Business.Authentication;


public interface IAuthManager { 
    string GenerateJwtToken(TaskManagement.Entity.Model.User user); 
    RefreshToken GenerateRefreshToken(TaskManagement.Entity.Model.User user);
    Task<TaskManagement.Entity.Model.User> ValidateRefreshTokenAsync(string refreshToken);
    Task<(string AccessToken, string RefreshToken)> RefreshTokensAsync(string refreshToken);
}