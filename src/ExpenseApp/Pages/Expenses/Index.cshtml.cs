using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class IndexModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public List<Expense>       Expenses       { get; private set; } = [];
    public List<ExpenseStatus> Statuses       { get; private set; } = [];
    public List<User>          Users          { get; private set; } = [];
    public int?                FilterStatusId { get; private set; }
    public int?                FilterUserId   { get; private set; }
    public string?             DbError        { get; private set; }

    public IndexModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync(int? statusId, int? userId)
    {
        FilterStatusId = statusId;
        FilterUserId   = userId;

        Expenses = await _db.GetExpensesAsync(userId, statusId);
        Statuses = await _db.GetExpenseStatusesAsync();
        Users    = await _db.GetUsersAsync(activeOnly: false);
        DbError  = _db.LastError;
    }
}
