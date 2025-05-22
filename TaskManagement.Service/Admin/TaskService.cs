using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;
using TaskManagement.Service.BaseService;

namespace TaskManagement.Service.Admin;

public class TaskService : BaseServices, ITaskService
{
    public TaskService(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    public async Task<ApiResponseModel> CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", "")
                .PostJsonAsync(createTaskDto)
                .ReceiveJson<TaskDto>();

            return new ApiResponseModel(true, "Created Succesfully.", response);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (Exception ex)
        {
            return new ApiResponseModel(false, ex.Message);
        }
    }




    public async Task<ApiResponseModel> CreateUserTaskAsync(CreateTaskDto createTaskDto)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", "create-and-assign-to-me")
                .PostJsonAsync(createTaskDto)
                .ReceiveJson<TaskDto>();

            return new ApiResponseModel(true, "Created Succesfully.", response);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (Exception ex)
        {
            return new ApiResponseModel(false, ex.Message);
        }
    }


    public async Task<List<TaskDto>> GetAllTasksAsync(bool includeDeleted = false)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", $"?includeDeleted={includeDeleted}")
                .GetJsonAsync<List<TaskDto>>();

            return response ?? new List<TaskDto>();
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            // Log error if needed
            return new List<TaskDto>();
        }
    }

    public async Task<TaskDto> GetTaskByIdAsync(int id)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", $"{id}")
                .GetJsonAsync<TaskDto>();

            return response;
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Task not found");
            }

            var error = await ex.GetResponseJsonAsync<TaskDto>();
            return error;
        }
    }

    public async Task<bool> UpdateTaskAsync(UpdateTaskDto model)
    {
        try
        {
            await GetFlurlRequestWithToken("Tasks", $"{model.Id}")
                .PutJsonAsync(model);

            return true;
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Task not found");
            }

            return false;
        }
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        try
        {
            await GetFlurlRequestWithToken("Tasks", $"{id}")
                .DeleteAsync();

            return true;
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Task not found");
            }

            return false;
        }
    }

    // Additional task-specific methods
    public async Task<List<TaskDto>> GetTasksByUserIdAsync(int userId)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", $"user/{userId}")
                .GetJsonAsync<List<TaskDto>>();

            return response ?? new List<TaskDto>();
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch
        {
            return new List<TaskDto>();
        }
    }

    public async Task<bool> UpdateTaskStatusAsync(int taskId, int statusId)
    {
        try
        {
            await GetFlurlRequestWithToken("Tasks", $"{taskId}/status/{statusId}")
                .PatchAsync();

            return true;
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<TaskStateDto>> GetTaskStatus()
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", "statuses").
                GetJsonAsync<List<TaskStateDto>>();
            return response ?? new List<TaskStateDto>();
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            throw ex;
        }
    }

    public async Task<TaskDetails> AddTaskDetailAsync(CreateTaskDetailDto details)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", $"{details.TaskId}/details")
                .PostJsonAsync(details)
                .ReceiveJson<TaskDetails>();

            return response;
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            throw ex;
        }

    }

    public async Task<bool> DeleteDetailAsync(int id)
    {
        try
        {
            await GetFlurlRequestWithToken("Tasks", $"detailsdelete/{id}")
                .DeleteAsync();

            return true;
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Task not found");
            }

            return false;
        }
    }
    public async Task<List<TaskDto>> GetTasksForCurrentUserAsync()
    {
        try
        {
            var request = GetFlurlRequestWithToken("tasks", "user/tasks");
            var response = await request.GetAsync();

            if (response.StatusCode == (int)HttpStatusCode.NoContent)
            {
                return new List<TaskDto>();
            }

            return await response.GetJsonAsync<List<TaskDto>>();
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {

            if (ex.StatusCode == (int)HttpStatusCode.Unauthorized ||
                ex.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                HandleTokenExpiry();
                throw; // Re-throw after handling token expiry
            }

            return new List<TaskDto>();
        }
        catch (UnauthorizedAccessException)
        {
            HandleTokenExpiry();
            throw;
        }
    }

    public async Task<List<TaskDto>> GetFilteredTasksAsync(TaskFilterModel filter)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", "FilterApply")
                .PostJsonAsync(filter)
                .ReceiveJson<List<TaskDto>>();

            return response ?? new List<TaskDto>();
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            // Log error if needed
            return new List<TaskDto>();
        }
    }


    public async Task<bool> UpdateStatusAsync(int taskId, int statusId)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Tasks", "UpdateStatusById")
                .SetQueryParams(new { taskId = taskId, statusId = statusId })
                .PostAsync(null); // You can use PostAsync(null) for empty body

            return true;
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == 401)
        {
            throw new UnauthorizedAccessException("Unauthorized");
        }
        catch (FlurlHttpException ex)
        {
            if (ex.StatusCode == (int)HttpStatusCode.NotFound)
                throw new KeyNotFoundException("Task not found");

            return false;
        }
    }

}
