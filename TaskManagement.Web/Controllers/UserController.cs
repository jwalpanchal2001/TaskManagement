using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagement.Model.Dto;
using TaskManagement.Service.Admin;

namespace TaskManagement.Web.Controllers;
[Authorize(Roles = "User")]
public class UserController : Controller
{
    private readonly ITaskService _taskService;
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(ITaskService taskService, ILogger<UserController> logger, IUserService userService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userService = userService;
    }
    public async Task<IActionResult> Index()
    {
        try
        {
            var tasks = await _taskService.GetTasksForCurrentUserAsync();
            ViewBag.TaskStatuses = await _taskService.GetTaskStatus();

            return View(tasks);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for current user");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskDto model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_CreateTaskPartial", model);
        }

        try
        {
            var result = await _taskService.CreateUserTaskAsync(model);

            if (result.IsSuccess)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", result.Message);
            return PartialView("_CreateTaskPartial", model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTaskForEdit(int id)
    {
        try
        {
            ViewBag.Users = await _userService.GetAllUsersAsync();
            var task = await _taskService.GetTaskByIdAsync(id);
            ViewBag.TaskStatuses = await _taskService.GetTaskStatus();

            if (task == null)
            {
                return NotFound("User not found");
            }

            return PartialView("_EditTaskPartial", task);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTask(UpdateTaskDto model)
    {
        try
        {
            var result = await _taskService.UpdateTaskAsync(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Task updated Sucessfully.";

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to update user");
            return PartialView("_EditTaskPartial", model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return PartialView("_EditTaskPartial", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var result = await _taskService.DeleteTaskAsync(id);

            if (result)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to delete task" });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(401, new { success = false, message = "Unauthorized" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> TaskStatus(int id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            ViewBag.TaskStatuses = await _taskService.GetTaskStatus();

            if (task == null)
            {
                return NotFound("Task not found");
            }

            return PartialView("_TaskStatusPartial", task);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, int TaskStatusId)
    {
        try
        {
            bool success = await _taskService.UpdateStatusAsync(id, TaskStatusId);
            return Ok(new { success = true, newStatus = TaskStatusId });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(401, new { success = false, message = "Unauthorized" });
        }
        catch
        {
            return StatusCode(500, new { success = false });
        }
    }


}