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
//[Authorize] // All authenticated users can access, but some endpoints are admin-only
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

        // Non-admin users can only see their own tasks
        //if (!User.IsInRole("Admin") && task.UserId.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
        //{
        //    return Forbid();
        //}

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
    {
        try
        {
            // This will now work because we've ensured authentication
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

        // Non-admin users can only update their own tasks
        //if (!User.IsInRole("Admin"))
        //{
            var task = await _taskService.GetTaskByIdAsync(id);
            //if (task == null || task.UserId.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            //{
            //    return Forbid();
            //}
        //}

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
    //[Authorize(Roles = "Admin")] // Only admins can delete tasks
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

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByUser(int userId)
    {
        // Non-admin users can only see their own tasks
        //if (!User.IsInRole("Admin") && userId.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
        //{
        //    return Forbid();
        //}

        var tasks = await _taskService.GetTasksByUserIdAsync(userId);
        return Ok(tasks);
    }

    [HttpPost("{taskId}/assign/{userId}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTask(int taskId, int userId)
    {
        try
        {
            await _taskService.AssignTaskAsync(taskId, userId);
            return Ok(new { Message = "Task assigned successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{taskId}/unassign")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnassignTask(int taskId)
    {
        try
        {
            await _taskService.UnassignTaskAsync(taskId);
            return Ok(new { Message = "Task unassigned successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{taskId}/can-assign")]
    public async Task<IActionResult> CanAssignTask(int taskId)
    {
        var canAssign = await _taskService.CanAssignTaskAsync(taskId);
        return Ok(new { CanAssign = canAssign });
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
        // Verify IDs match
        //if (detailId != taskDetailDto.Id)
        //{
        //    return BadRequest("ID mismatch");
        //}

        // Verify task exists and detail belongs to task
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
        // Verify task exists and detail belongs to task
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
}
