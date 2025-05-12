using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using TaskManagement.Data.Repository;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;

namespace TaskManagement.Business.User;
public class UserManager : IUserManager
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<Entity.Model.User> _passwordHasher;
    private readonly ITaskRepository _taskRepository;

    public UserManager(IUserRepository userRepository, IMapper mapper, IPasswordHasher<TaskManagement.Entity.Model.User> passwordHasher , ITaskRepository taskRepository)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _taskRepository = taskRepository;
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(bool includeDeleted = false)
    {
        var users = await _userRepository.GetAllUsersAsync(includeDeleted);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
    {
        if (await _userRepository.UsernameExistsAsync(userDto.Username))
        {
            throw new ArgumentException("Username already exists");
        }

        var user = _mapper.Map<TaskManagement.Entity.Model.User>(userDto);
        user.PasswordHash = _passwordHasher.HashPassword(user, userDto.Password);


        var createdUser = await _userRepository.AddUserAsync(user);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task UpdateUserAsync(int id, UpdateUserDto userDto)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        _mapper.Map(userDto, user);
        await _userRepository.UpdateUserAsync(user);
    }

    public async Task DeleteUserAsync(int id)
    {
        if (!await _userRepository.UserExistsAsync(id))
        {
            throw new KeyNotFoundException("User not found");
        }

        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return;

        user.IsDeleted = true;
        await _userRepository.UpdateUserAsync(user);

        // Soft delete tasks created by the user
        var createdTasks = await _taskRepository.GetTasksCreatedByUserAsync(id);
        foreach (var task in createdTasks)
        {
            task.IsDeleted = true;
            await _taskRepository.UpdateTaskAsync(task);
        }

        // Unassign tasks assigned to the user
        var assignedTasks = await _taskRepository.GetTasksByUserIdAsync(id);
        foreach (var task in assignedTasks)
        {
            task.UserId = null;
            await _taskRepository.UpdateTaskAsync(task);
        }
    }

    public async Task<bool> UserExistsAsync(int id)
    {
        return await _userRepository.UserExistsAsync(id);
    }
}
