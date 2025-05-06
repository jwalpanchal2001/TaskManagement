using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Entity.Model;

namespace TaskManagement.Data.Repository.Authentication;

public interface IAuthRepository { 
    Task<User> GetUserByUsernameAsync(string username); 
    Task<RefreshToken> GetRefreshTokenAsync(string token); 
    Task<bool> UsernameExistsAsync(string username); 
    Task AddUserAsync(User user); 
    Task AddRefreshTokenAsync(RefreshToken refreshToken); 
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken); 
    Task SaveChangesAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<RefreshToken> GetLatestRefreshTokenAsync(int userId);
}
