using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;
using TaskManagement.Service.Admin;

namespace TaskManagement.Web.Controllers;
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ITaskService _taskService;

    public AdminController(IUserService userService , ITaskService taskService)
    {
        _userService = userService;
        _taskService = taskService;
    }
    [Authorize(Roles = "Admin")]

    #region User
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            // Return the partial view with validation messages
            return PartialView("_CreateUserPartial", createUserDto);
        }

        try
        {
            var result = await _userService.CreateUserAsync(createUserDto);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction("Index");
            }

            // Add error to model state and return form with existing data
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create user.");
            TempData["ErrorMsg"] = result.Message;
            return RedirectToAction("Index");
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You are not authorized to perform this action.";
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            // Log exception (optional)
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return PartialView("_CreateUserPartial", createUserDto);
        }
    }


    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetUserDetails(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return PartialView("_ViewUserPartial", new UserDto
                {
                    FullName = "User not found"
                });
            }

            return PartialView("_ViewUserPartial", user);
        }
        catch (UnauthorizedAccessException)
        {
            return PartialView("_ViewUserPartial", new UserDto
            {
                FullName = "Unauthorized access. Please log in."
            });
        }
        catch (Exception ex)
        {
            return PartialView("_ViewUserPartial", new UserDto
            {
                FullName = $"Error: {ex.Message}"
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetUserForEdit(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return PartialView("_EditUserPartial", user);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UpdateUser(UpdateUserDto model)
    {
        try
        {
            var result = await _userService.UpdateUserAsync(model);

            if (result)
            {
                return Json(new { success = true, message = "User updated successfully." });
            }

            // Return validation errors as partial view
            ModelState.AddModelError("", "Failed to update user");
            return PartialView("_EditUserPartial", model);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(401, new { success = false, message = "Unauthorized" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return PartialView("_EditUserPartial", model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);

            if (result)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to delete user" });
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

    #endregion

    #region Task 

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> TaskIndex(
      [FromQuery] int? assignedToId = null,
      [FromQuery] int? statusId = null,
      [FromQuery] DateTime? startDate = null,
      [FromQuery] DateTime? endDate = null,
      [FromQuery] string searchTerm = null,
      [FromQuery] string sortBy = "DueDate",
      [FromQuery] string sortOrder = "desc")
    {
        try
        {
            ViewBag.Users = await _userService.GetAllUsersAsync();
            ViewBag.TaskStatuses = await _taskService.GetTaskStatus();

            var tasks = await _taskService.GetFilteredTasksAsync(
                includeDeleted: false,
                assignedToId: assignedToId,
                statusId: statusId,
                startDate: startDate,
                endDate: endDate,
                searchTerm: searchTerm);

            var sortedTasks = SortTaskDtos(tasks, sortBy, sortOrder);
            return View(sortedTasks);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
        catch (Exception)
        {
            return StatusCode(500, "Error loading tasks");
        }
    }

    private IEnumerable<TaskDto> SortTaskDtos(IEnumerable<TaskDto> tasks, string sortBy, string sortOrder)
    {
        switch (sortBy.ToLower())
        {
            case "title":
                return sortOrder.ToLower() == "desc"
                    ? tasks.OrderByDescending(t => t.Title)
                    : tasks.OrderBy(t => t.Title);

            case "duedate":
                return sortOrder.ToLower() == "desc"
                    ? tasks.OrderByDescending(t => t.DueDate)
                    : tasks.OrderBy(t => t.DueDate);

            case "createdat":
                return sortOrder.ToLower() == "desc"
                    ? tasks.OrderByDescending(t => t.CreatedAt)
                    : tasks.OrderBy(t => t.CreatedAt);

            case "status":
                return sortOrder.ToLower() == "desc"
                    ? tasks.OrderByDescending(t => t.TaskStatus)
                    : tasks.OrderBy(t => t.TaskStatus);

            default:
                return sortOrder.ToLower() == "desc"
                    ? tasks.OrderByDescending(t => t.DueDate)
                    : tasks.OrderBy(t => t.DueDate);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _userService.GetAllUsersAsync();
                return PartialView("_CreateTaskPartial", model);
            }

            var result = await _taskService.CreateTaskAsync(model);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Task created successfully."; 
                return RedirectToAction("TaskIndex");
            }

            ModelState.AddModelError("", result.Message);
            ViewBag.Users = await _userService.GetAllUsersAsync();
            return PartialView("_CreateTaskPartial", model);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTaskDetails(int id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return PartialView("_ViewTaskPartial", task);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(401, "Unauthorized");
        }
        catch (Exception)
        {
            return StatusCode(500, "Error loading task details");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetTaskForEdit(int id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            ViewBag.Users = await _userService.GetAllUsersAsync();
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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UpdateTask(UpdateTaskDto model)
    {
        try
        {
            var result = await _taskService.UpdateTaskAsync(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Task Updated successfully.";

                return Redirect("TaskIndex");
            }

         

            ModelState.AddModelError("", "Failed to update user");
            return PartialView("_EditTaskPartial", model);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(401, new { success = false, message = "Unauthorized" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return PartialView("_EditTaskPartial", model);
        }
    }

    [Authorize(Roles = "Admin")]
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
            return Json(new { success = false, message = "Unauthorized" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    [HttpGet]
    public IActionResult AddComment(int taskId)
    {
        var model = new CreateTaskDetailDto { TaskId = taskId };
        return PartialView("_AddCommentPartial", model);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(CreateTaskDetailDto model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_AddCommentPartial", model);
        }

        try
        {
            await _taskService.AddTaskDetailAsync(model);
            return Json(new
            {
                success = true,
                taskId = model.TaskId,
                message = "Comment added successfully"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Json(new
            {
                success = false,
                message = "Unauthorized access. Please log in again."
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Error adding comment: " + ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDetail(int detailId)
    {
        try
        {
            await _taskService.DeleteDetailAsync(detailId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(401, "Unauthorized access. Please log in again.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while deleting the task detail");
        }
    }

}
