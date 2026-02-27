using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

/// <summary>User management endpoints – all data access via stored procedures.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IExpenseDatabase _db;

    public UsersController(IExpenseDatabase db) => _db = db;

    /// <summary>List all users.</summary>
    /// <param name="activeOnly">When true (default), returns only active users.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] bool activeOnly = true)
    {
        var users = await _db.GetUsersAsync(activeOnly);
        return Ok(users);
    }

    /// <summary>Get a single user by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _db.GetUserByIdAsync(id);
        return user is null ? NotFound(new { message = $"User {id} not found." }) : Ok(user);
    }

    /// <summary>Create a new user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest(new { message = "UserName is required." });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required." });

        try
        {
            var newId = await _db.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = newId }, new { userId = newId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update an existing user.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest(new { message = "UserName is required." });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required." });

        try
        {
            var rows = await _db.UpdateUserAsync(id, request);
            return rows == 0 ? NotFound(new { message = $"User {id} not found." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Deactivate (soft-delete) a user.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var rows = await _db.DeleteUserAsync(id);
            return rows == 0 ? NotFound(new { message = $"User {id} not found." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>List all roles.</summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(List<Role>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.GetRolesAsync();
        return Ok(roles);
    }
}
