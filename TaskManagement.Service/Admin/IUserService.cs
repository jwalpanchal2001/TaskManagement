using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;

namespace TaskManagement.Service.Admin;

public interface IUserService
{
    Task<ApiResponseModel> CreateUserAsync(CreateUserDto createUserDto);
    Task<List<UserDto>> GetAllUsersAsync(bool includeDeleted = false);
    Task<UserDto> GetUserByIdAsync(int id);
    Task<bool> UpdateUserAsync(UpdateUserDto model);
    Task<bool> DeleteUserAsync(int id);



}
