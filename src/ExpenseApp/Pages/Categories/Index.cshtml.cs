using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public List<ExpenseCategory> Categories { get; private set; } = [];
    public string?               DbError    { get; private set; }

    public IndexModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync()
    {
        Categories = await _db.GetCategoriesAsync(activeOnly: false);
        DbError    = _db.LastError;
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        await _db.DeleteCategoryAsync(id);
        return RedirectToPage();
    }
}
