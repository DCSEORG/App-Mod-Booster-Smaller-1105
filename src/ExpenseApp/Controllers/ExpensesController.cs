using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApp.Controllers;

/// <summary>Expense endpoints – all data access via stored procedures.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseDatabase _db;

    public ExpensesController(IExpenseDatabase db) => _db = db;

    /// <summary>List expenses. Optionally filter by user and/or status.</summary>
    /// <param name="userId">Filter by submitting user ID (optional).</param>
    /// <param name="statusId">Filter by status ID (optional).</param>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Results per page (default 50, max 200).</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<Expense>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] int? userId   = null,
        [FromQuery] int? statusId = null,
        [FromQuery] int  page     = 1,
        [FromQuery] int  pageSize = 50)
    {
        if (page < 1)     page     = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 200) pageSize = 200;

        var expenses = await _db.GetExpensesAsync(userId, statusId, page, pageSize);
        return Ok(expenses);
    }

    /// <summary>Get a single expense by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Expense), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExpenseById(int id)
    {
        var expense = await _db.GetExpenseByIdAsync(id);
        return expense is null ? NotFound(new { message = $"Expense {id} not found." }) : Ok(expense);
    }

    /// <summary>Create a new expense (always starts in Draft status).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Currency))
            request.Currency = "GBP";

        if (request.AmountMinor <= 0)
            return BadRequest(new { message = "AmountMinor must be greater than zero." });

        try
        {
            var newId = await _db.CreateExpenseAsync(request);
            return CreatedAtAction(nameof(GetExpenseById), new { id = newId }, new { expenseId = newId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update an expense (only allowed in Draft status).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
    {
        if (request.AmountMinor <= 0)
            return BadRequest(new { message = "AmountMinor must be greater than zero." });

        try
        {
            var rows = await _db.UpdateExpenseAsync(id, request);
            return rows == 0 ? NotFound(new { message = $"Expense {id} not found." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a Draft expense.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        try
        {
            var rows = await _db.DeleteExpenseAsync(id);
            return rows == 0 ? NotFound(new { message = $"Expense {id} not found." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Submit a Draft expense for manager review.</summary>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitExpense(int id)
    {
        try
        {
            var rows = await _db.SubmitExpenseAsync(id);
            return rows == 0 ? NotFound(new { message = $"Expense {id} not found or not in Draft status." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Approve a Submitted expense.</summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveExpense(int id, [FromBody] ApproveRejectRequest request)
    {
        try
        {
            var rows = await _db.ApproveExpenseAsync(id, request.ReviewedBy);
            return rows == 0 ? NotFound(new { message = $"Expense {id} not found or not in Submitted status." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Reject a Submitted expense.</summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectExpense(int id, [FromBody] ApproveRejectRequest request)
    {
        try
        {
            var rows = await _db.RejectExpenseAsync(id, request.ReviewedBy);
            return rows == 0 ? NotFound(new { message = $"Expense {id} not found or not in Submitted status." }) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get a summary of expenses grouped by status.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(List<ExpenseSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _db.GetExpenseSummaryAsync();
        return Ok(summary);
    }

    /// <summary>Get all available expense statuses.</summary>
    [HttpGet("statuses")]
    [ProducesResponseType(typeof(List<ExpenseStatus>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatuses()
    {
        var statuses = await _db.GetExpenseStatusesAsync();
        return Ok(statuses);
    }
}
