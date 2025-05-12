using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Data.Repository;

public interface ITaskRepository
{
    Task<Tasks> GetTaskByIdAsync(int id);
    Task<IEnumerable<Tasks>> GetAllTasksAsync(bool includeDeleted = false);
    Task<IEnumerable<Tasks>> GetTasksByUserIdAsync(int userId);
    Task<Tasks> AddTaskAsync(Tasks task);
    Task UpdateTaskAsync(Tasks task);
    Task DeleteTaskAsync(int id);
    Task<bool> TaskExistsAsync(int id);
    Task<IEnumerable<TaskState>> GetAllTaskStatesAsync();
    Task<bool> IsTaskUnassignedAsync(int taskId);
    Task AssignTaskToUserAsync(int taskId, int userId);
    Task UnassignTaskAsync(int taskId);

    Task<List<TaskDto1>> GetFilteredTasksAsync(TaskFilterModel filter);
    Task<bool> UpdateTaskStatus(int taskId, int statusId);
    Task<IEnumerable<Tasks>> GetTasksCreatedByUserAsync(int userId);

}