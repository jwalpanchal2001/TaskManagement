using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data.Repository;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Business.Tasks;

public class TaskManager : ITaskManager
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskManager(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<TaskDto> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetTaskByIdAsync(id);
        if (task == null) return null;

        var taskDto = task.Adapt<TaskDto>();
        taskDto.TaskDetails = task.TaskDetails?.Adapt<IEnumerable<TaskDetails>>();
        return taskDto;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(bool includeDeleted = false)
    {
        var tasks = await _taskRepository.GetAllTasksAsync(includeDeleted);
        var taskDtos = tasks.Adapt<IEnumerable<TaskDto>>();

        // If you want to include details in the list view (might impact performance)
        foreach (var taskDto in taskDtos)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskDto.Id);
            taskDto.TaskDetails = task?.TaskDetails?.Adapt<IEnumerable<TaskDetails>>();
        }

        return taskDtos;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId)
    {
        var tasks = await _taskRepository.GetTasksByUserIdAsync(userId);
        return tasks.Adapt<IEnumerable<TaskDto>>();
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, int createdById)
    {
        if (!await _userRepository.UserExistsAsync(createdById))
            throw new KeyNotFoundException("Creator user not found");

        if (taskDto.UserId.HasValue && !await _userRepository.UserExistsAsync(taskDto.UserId.Value))
            throw new KeyNotFoundException("Assigned user not found");
        if (taskDto.UserId.HasValue)
        {

            if (taskDto.TaskStatusId == 1)
            {
                taskDto.TaskStatusId = 2;

            }
        }


        var task = taskDto.Adapt<TaskManagement.Entity.Model.Tasks>();
        task.CreatedAt = DateTime.UtcNow;
        task.CreatedById = createdById;

        var createdTask = await _taskRepository.AddTaskAsync(task);
        var result = createdTask.Adapt<TaskDto>();

        var creator = await _userRepository.GetUserByIdAsync(createdById);
        result.CreatedByName = creator?.FullName;
        
        

        return result;
    }

    public async Task UpdateTaskAsync(int id, UpdateTaskDto taskDto)
    {
        var task = await _taskRepository.GetTaskByIdAsync(id);
        if (task == null)
            throw new KeyNotFoundException("Task not found");

        if (taskDto.UserId.HasValue)
        {
            // Validate user exists if one is being assigned
            if (!await _userRepository.UserExistsAsync(taskDto.UserId.Value))
                throw new KeyNotFoundException("Assigned user not found");

            // If task is being assigned to a user and is currently Unassigned,
            // default to Todo status (unless status is explicitly provided)
            if (task.TaskStatusId == 1 && taskDto.TaskStatusId == 1) // 1 = Unassigned
            {
                taskDto.TaskStatusId = 2; // 2 = Todo
            }
        }
        else
        {
            // If no user is assigned, set status to Unassigned
            taskDto.TaskStatusId = 1; // 1 = Unassigned
        }

        taskDto.Adapt(task);
        await _taskRepository.UpdateTaskAsync(task);
    }

    public async Task DeleteTaskAsync(int id)
    {
        if (!await _taskRepository.TaskExistsAsync(id))
            throw new KeyNotFoundException("Task not found");

        await _taskRepository.DeleteTaskAsync(id);
    }

    public async Task<IEnumerable<TaskStateDto>> GetAllTaskStatesAsync()
    {
        var taskStates = await _taskRepository.GetAllTaskStatesAsync();
        return taskStates.Adapt<IEnumerable<TaskStateDto>>();
    }

    public async Task<bool> CanAssignTaskAsync(int taskId)
    {
        return await _taskRepository.IsTaskUnassignedAsync(taskId);
    }

    public async Task AssignTaskAsync(int taskId, int userId)
    {
        if (!await _taskRepository.IsTaskUnassignedAsync(taskId))
            throw new InvalidOperationException("Task is already assigned or doesn't exist");

        await _taskRepository.AssignTaskToUserAsync(taskId, userId);
    }

    public async Task UnassignTaskAsync(int taskId)
    {
        await _taskRepository.UnassignTaskAsync(taskId);
    }

    public async Task<bool> TaskExistsAsync(int taskId)
    {
        return await _taskRepository.TaskExistsAsync(taskId);
    }


    public async Task<List<TaskDto>> GetFilteredTasksAsync(TaskFilterModel filter)
    {
        var tasks = await _taskRepository.GetFilteredTasksAsync(filter);
        var sortedTasks = ApplySorting(tasks, filter.SortBy, filter.SortOrder);

        return sortedTasks.Adapt<List<TaskDto>>();
    }


    private List<TaskDto1> ApplySorting(List<TaskDto1> tasks, string sortBy, string sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) sortBy = "DueDate";
        if (string.IsNullOrWhiteSpace(sortOrder)) sortOrder = "desc";
        var query = tasks.AsQueryable();

        switch (sortBy.ToLower())
        {
            case "title":
                query = sortOrder == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title);
                break;
            case "createdat":
                query = sortOrder == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt);
                break;
            case "duedate":
                query = sortOrder == "asc" ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate);
                break;
            case "status":
                query = sortOrder == "asc" ? query.OrderBy(t => t.TaskStatus) : query.OrderByDescending(t => t.TaskStatus);
                break;
            default:
                query = sortOrder == "asc" ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate);
                break;
        }

        return query.ToList();
    }
    public async Task<bool> UpdateStatusAsync(int taskId, int statusId)
    {
        var task = await _taskRepository.UpdateTaskStatus(taskId, statusId);
        return task;
    }


}