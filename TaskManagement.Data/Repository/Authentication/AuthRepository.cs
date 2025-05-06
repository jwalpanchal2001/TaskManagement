using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Entity.Model;

namespace TaskManagement.Data.Repository.Authentication;


public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;
    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<User> GetUserByUsernameAsync(string username)
        {
        return await _context.Users
            .SingleOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
    }


    public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
    {
        return await _context.RefreshTokens
            .Include(r => r.User) // <-- THIS IS IMPORTANT
            .FirstOrDefaultAsync(r => r.Token == refreshToken);
    }


    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        // Try to parse the string to int (safely)
      
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        

        // Return null if parsing fails (invalid ID format)
    }

    public async Task<RefreshToken> GetLatestRefreshTokenAsync(int userId)
    {
        return await _context.RefreshTokens
           .Where(rt => rt.UserId == userId
                     && rt.RevokedAt == null // Not revoked
                     && rt.ExpiresAt > DateTime.UtcNow) // Still valid
           .OrderByDescending(rt => rt.CreatedAt)
           .FirstOrDefaultAsync();
    }

}
