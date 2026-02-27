using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class DetailModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public Expense? Expense      { get; private set; }
    public string?  Message      { get; private set; }
    public string   MessageClass { get; private set; } = "info";

    public DetailModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync(int id)
    {
        Expense = await _db.GetExpenseByIdAsync(id);
    }

    public async Task<IActionResult> OnPostSubmitAsync(int id)
    {
        try
        {
            await _db.SubmitExpenseAsync(id);
            Message      = "Expense submitted for review.";
            MessageClass = "success";
        }
        catch (Exception ex)
        {
            Message      = $"Error: {ex.Message}";
            MessageClass = "danger";
        }
        Expense = await _db.GetExpenseByIdAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _db.DeleteExpenseAsync(id);
            return RedirectToPage("/Expenses/Index");
        }
        catch (Exception ex)
        {
            Message      = $"Error: {ex.Message}";
            MessageClass = "danger";
            Expense = await _db.GetExpenseByIdAsync(id);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, int reviewedBy)
    {
        try
        {
            await _db.ApproveExpenseAsync(id, reviewedBy);
            Message      = "Expense approved.";
            MessageClass = "success";
        }
        catch (Exception ex)
        {
            Message      = $"Error: {ex.Message}";
            MessageClass = "danger";
        }
        Expense = await _db.GetExpenseByIdAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, int reviewedBy)
    {
        try
        {
            await _db.RejectExpenseAsync(id, reviewedBy);
            Message      = "Expense rejected.";
            MessageClass = "warning";
        }
        catch (Exception ex)
        {
            Message      = $"Error: {ex.Message}";
            MessageClass = "danger";
        }
        Expense = await _db.GetExpenseByIdAsync(id);
        return Page();
    }
}
