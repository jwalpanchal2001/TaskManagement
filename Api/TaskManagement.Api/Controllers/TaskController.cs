using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Model.Dto;
using TaskManagement.Business.Tasks;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto.UserTask;
using TaskManagement.Business.TaskDetail;
using Microsoft.IdentityModel.JsonWebTokens;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All authenticated users can access, but some endpoints are admin-only
public class TasksController : ControllerBase
{
    private readonly ITaskManager _taskService;
    private readonly ITaskDetailManager _taskDetailService;

    public TasksController(ITaskManager taskService , ITaskDetailManager taskDetailManager)
    {
        _taskService = taskService;
        _taskDetailService = taskDetailManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(bool includeDeleted = false)
    {
        var isAdmin = User.IsInRole("Admin");
        var tasks = await _taskService.GetAllTasksAsync(isAdmin && includeDeleted);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var taskDto = await _taskService.CreateTaskAsync(createTaskDto, currentUserId);
            return CreatedAtAction(nameof(GetTask), new { id = taskDto.Id }, taskDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPost("create-and-assign-to-me")]
    public async Task<ActionResult<TaskDto>> CreateTaskAndAssignToMe(CreateTaskDto createTaskDto)
    {
        try
        {
            // Get current user ID from claims
            var currentUserId = GetCurrentUserId();

            // Automatically assign the task to the current user
            createTaskDto.UserId = currentUserId;

            // Status will be automatically set to 2 (Assigned) in the service layer
            var taskDto = await _taskService.CreateTaskAsync(createTaskDto, currentUserId);

            return CreatedAtAction(nameof(GetTask), new { id = taskDto.Id }, taskDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the exception here
            return StatusCode(500, new { message = "An error occurred while creating the task" });
        }
    }

    // [PUT], [DELETE], and other methods remain unchanged...

    private int GetCurrentUserId()
    {
        // More comprehensive auth check
        if (User?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        // Try multiple claim types (order matters)
        var userIdClaim = User.FindFirst("id") ??
                         User.FindFirst(ClaimTypes.NameIdentifier) ??
                         User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User identifier claim not found");
        }

        if (!int.TryParse(userIdClaim.Value, out var userId) || userId <= 0)
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto updateTaskDto)
    {
        if (id != updateTaskDto.Id)
        {
            return BadRequest("ID mismatch");
        }


            var task = await _taskService.GetTaskByIdAsync(id);


        try
        {
            await _taskService.UpdateTaskAsync(id, updateTaskDto);
            return NoContent();

        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            await _taskService.DeleteTaskAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("statuses")]
    public async Task<ActionResult<IEnumerable<TaskStateDto>>> GetTaskStatuses()
    {
        var statuses = await _taskService.GetAllTaskStatesAsync();
        return Ok(statuses);
    }

    [HttpGet("user/tasks")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByUser()
    {
        var userIdClaim = User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId == 0)
        {
            return Forbid();
        }

        var tasks = await _taskService.GetTasksByUserIdAsync(userId);
        return Ok(tasks);
    }



    #region TaskDetails Endpoints

    [HttpGet("{taskId}/details")]
    public async Task<ActionResult<IEnumerable<TaskDetails>>> GetTaskDetails(int taskId)
    {
        // Verify task exists first
        if (!await _taskService.TaskExistsAsync(taskId))
        {
            return NotFound("Task not found");
        }

        var details = await _taskDetailService.GetByTaskIdAsync(taskId);
        return Ok(details);
    }

    [HttpGet("{taskId}/details/{detailId}")]
    public async Task<ActionResult<TaskDetails>> GetTaskDetail(int taskId, int detailId)
    {
        // Verify task exists first
        if (!await _taskService.TaskExistsAsync(taskId))
        {
            return NotFound("Task not found");
        }

        var detail = await _taskDetailService.GetByIdAsync(detailId);

        // Verify detail belongs to task
        if (detail == null || detail.TaskId != taskId)
        {
            return NotFound("Task detail not found");
        }

        return Ok(detail);
    }

    [HttpPost("{taskId}/details")]
    public async Task<ActionResult<TaskDetails>> CreateTaskDetail(int taskId, CreateTaskDetailDto taskDetailDto)
    {
        // Verify task exists and matches
        if (!await _taskService.TaskExistsAsync(taskId) || taskDetailDto.TaskId != taskId)
        {
            return NotFound("Task not found");
        }

        try
        {
            var createdDetail = await _taskDetailService.CreateAsync(taskDetailDto);
            return CreatedAtAction(
                nameof(GetTaskDetail),
                new { taskId, detailId = createdDetail.Id },
                createdDetail);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{taskId}/details/{detailId}")]
    public async Task<IActionResult> UpdateTaskDetail(
        int taskId,
        int detailId,
        UpdateTaskDetailDto taskDetailDto)
    {

        var detail = await _taskDetailService.GetByIdAsync(detailId);
        if (detail == null || detail.TaskId != taskId)
        {
            return NotFound("Task detail not found");
        }

        try
        {
            await _taskDetailService.UpdateAsync(detailId, taskDetailDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("detailsdelete/{detailId}")]
    public async Task<IActionResult> DeleteTaskDetail(int detailId)
    {
        var detail = await _taskDetailService.GetByIdAsync(detailId);
        if (detail == null )
        {
            return NotFound("Task detail not found");
        }

        try
        {
            await _taskDetailService.DeleteAsync(detailId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    #endregion



    [HttpGet("FilterApply")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetFilteredTasks([FromQuery] TaskFilterModel filter)
    {
        try
        {
            var tasks = await _taskService.GetFilteredTasksAsync(filter);
            return Ok(tasks);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while filtering tasks");
        }
    }


    [HttpPost("UpdateStatusById")]
    public async Task<IActionResult> UpdateStatusById([FromQuery] int taskId, [FromQuery] int statusId)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return NotFound("Task not found");
            }

            // Update only the status
            await _taskService.UpdateStatusAsync(taskId, statusId);

            return Ok(new { success = true, newStatus = statusId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

}
