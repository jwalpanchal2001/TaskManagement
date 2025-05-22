using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Business.User;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManager _userManager;

    public UsersController(IUserManager userService)
    {
        _userManager = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(bool includeDeleted = false)
    {
        try
        {
            var users = await _userManager.GetAllUsersAsync(includeDeleted);
            return Ok(users);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { success = false, message = "Unauthorized access. Token may be expired or invalid." });
        }
      
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userManager.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }




    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var userDto = await _userManager.CreateUserAsync(createUserDto);
            var response = new ApiResponseModel(true, "User created successfully.", userDto);
            return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponseModel(false, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponseModel(false, "Unexpected error: " + ex.Message));
        }
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
    {
        if (id != updateUserDto.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            await _userManager.UpdateUserAsync(id, updateUserDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {   
            await _userManager.DeleteUserAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

}
