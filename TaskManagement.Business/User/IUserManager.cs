using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Dto;

namespace TaskManagement.Business.User;

public interface IUserManager
{
    Task<UserDto> GetUserByIdAsync(int id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync(bool includeDeleted = false);
    Task<UserDto> CreateUserAsync(CreateUserDto userDto);
    Task UpdateUserAsync(int id, UpdateUserDto userDto);
    Task DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(int id);
}
