using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Service.Admin;

public interface ITaskService
{
    Task<ApiResponseModel> CreateTaskAsync(CreateTaskDto createTaskDto);
    Task<List<TaskDto>> GetAllTasksAsync(bool includeDeleted = false);
    Task<TaskDto> GetTaskByIdAsync(int id);
    Task<bool> UpdateTaskAsync(UpdateTaskDto model);
    Task<bool> DeleteTaskAsync(int id);
    Task<List<TaskStateDto>> GetTaskStatus();
    Task<TaskDetails> AddTaskDetailAsync(CreateTaskDetailDto details);
    Task<bool> DeleteDetailAsync(int id);

    Task<List<TaskDto>> GetTasksForCurrentUserAsync();

    Task<ApiResponseModel> CreateUserTaskAsync(CreateTaskDto createTaskDto);

    Task<List<TaskDto>> GetFilteredTasksAsync(
    bool includeDeleted = false,
    int? assignedToId = null,
    int? statusId = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    string searchTerm = null);

    Task<bool> UpdateStatusAsync(int taskId, int statusId);


    //IEnumerable<Task> SortTasks(IEnumerable<Task> tasks, string sortBy, string sortOrder);

}
