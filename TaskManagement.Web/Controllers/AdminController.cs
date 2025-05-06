using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;
using TaskManagement.Service.Admin;

namespace TaskManagement.Web.Controllers;
//[Authorize]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ITaskService _taskService;

    public AdminController(IUserService userService , ITaskService taskService)
    {
        _userService = userService;
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllUsersAsync(); 
        return View(users);
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_CreateUserPartial", createUserDto); // or whatever your partial view is
        }

        var result = await _userService.CreateUserAsync(createUserDto);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction("Index"); // or wherever you want to go next
        }

        ModelState.AddModelError("", result.Message ?? "Failed to create user.");
        return View(createUserDto);
    }

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
        catch (Exception ex)
        {
            return PartialView("_ViewUserPartial", new UserDto
            {
                FullName = $"Error: {ex.Message}"
            });
        }
    }



    [HttpGet]
    public async Task<IActionResult> GetUserForEdit(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }

        return PartialView("_EditUserPartial", user);
    }


    [HttpPost]
    public async Task<IActionResult> UpdateUser(UpdateUserDto model)
    {
     
        try
        {
            // Update user in database
            var result = await _userService.UpdateUserAsync(model);

            if (result)
            {
                return Json(new { success = true });
            }

            ModelState.AddModelError("", "Failed to update user");
            return PartialView("_EditUserPartial", model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return PartialView("_EditUserPartial", model);
        }
    }

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
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }



    #region Task 

    [HttpGet]
    public async Task<IActionResult> TaskIndex()
    {
        try
        {
            ViewBag.Users = _userService.GetAllUsersAsync().Result;
            var tasks = await _taskService.GetAllTasksAsync();
                return View(tasks);
        }
        catch (Exception ex)
        {
            // Log error
            return StatusCode(500, "Error loading tasks");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = await _userService.GetAllUsersAsync();
            //ViewBag.TaskStatuses = await _taskService.GetTaskStatusesAsync();
            return PartialView("_CreateTaskPartial", model);
        }

        var result = await _taskService.CreateTaskAsync(model);
        if (result.IsSuccess)
        {
            return RedirectToAction("TaskIndex");
        }

        ModelState.AddModelError("", result.Message);
        ViewBag.Users = await _userService.GetAllUsersAsync();
        //ViewBag.TaskStatuses = await _taskService.GetTaskStatusesAsync();
        return PartialView("_CreateTaskPartial", model);
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
        catch (Exception ex)
        {
            return StatusCode(500, "Error loading task details");
        }
    }


    [HttpGet]
    public async Task<IActionResult> GetTaskForEdit(int id)
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


    [HttpPost]
    public async Task<IActionResult> UpdateTask(UpdateTaskDto model)
    {

        try
        {
            // Update user in database
            var result = await _taskService.UpdateTaskAsync(model);

            if (result)
            {
                return Redirect("TaskIndex");
            }

            ModelState.AddModelError("", "Failed to update user");
            return PartialView("_EditTaskPartial", model);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while deleting the task detail");
        }
    }


}
