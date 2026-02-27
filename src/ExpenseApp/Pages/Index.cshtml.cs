using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages;

public class IndexModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public List<ExpenseSummary> Summary        { get; private set; } = [];
    public List<Expense>        RecentExpenses { get; private set; } = [];
    public string?              DbError        { get; private set; }

    public IndexModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync()
    {
        Summary        = await _db.GetExpenseSummaryAsync();
        RecentExpenses = await _db.GetExpensesAsync(page: 1, pageSize: 10);
        DbError        = _db.LastError;
    }
}
