using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Business.Tasks;

public interface ITaskManager
{
    Task<TaskDto> GetTaskByIdAsync(int id);
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(bool includeDeleted = false);
    Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId);
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int createdById);
    Task UpdateTaskAsync(int id, UpdateTaskDto taskDto);
    Task DeleteTaskAsync(int id);
    Task<IEnumerable<TaskStateDto>> GetAllTaskStatesAsync();
    Task<bool> CanAssignTaskAsync(int taskId);
    Task AssignTaskAsync(int taskId, int userId);
    Task UnassignTaskAsync(int taskId);
    Task<bool> TaskExistsAsync(int taskId);


}
