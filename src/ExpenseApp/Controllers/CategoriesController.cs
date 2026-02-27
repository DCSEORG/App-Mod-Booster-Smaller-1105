using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

/// <summary>Expense category management – all data access via stored procedures.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IExpenseDatabase _db;

    public CategoriesController(IExpenseDatabase db) => _db = db;

    /// <summary>List expense categories.</summary>
    /// <param name="activeOnly">When true (default), returns only active categories.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<ExpenseCategory>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] bool activeOnly = true)
    {
        var categories = await _db.GetCategoriesAsync(activeOnly);
        return Ok(categories);
    }

    /// <summary>Get a single category by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ExpenseCategory), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _db.GetCategoryByIdAsync(id);
        return category is null ? NotFound(new { message = $"Category {id} not found." }) : Ok(category);
    }

    /// <summary>Create a new expense category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return BadRequest(new { message = "CategoryName is required." });

        try
        {
            var newId = await _db.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { id = newId }, new { categoryId = newId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update an existing expense category.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return BadRequest(new { message = "CategoryName is required." });

        try
        {
            var rows = await _db.UpdateCategoryAsync(id, request);
            return rows == 0 ? NotFound(new { message = $"Category {id} not found." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Deactivate (soft-delete) an expense category.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var rows = await _db.DeleteCategoryAsync(id);
            return rows == 0 ? NotFound(new { message = $"Category {id} not found." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
