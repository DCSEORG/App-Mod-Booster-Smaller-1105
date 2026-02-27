using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Categories;

public class EditModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public ExpenseCategory? Category     { get; private set; }
    public string?          ErrorMessage { get; private set; }

    public EditModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync(int id)
    {
        Category = await _db.GetCategoryByIdAsync(id);
    }

    public async Task<IActionResult> OnPostAsync(int id, string CategoryName, bool IsActive)
    {
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            ErrorMessage = "Category name is required.";
            Category     = await _db.GetCategoryByIdAsync(id);
            return Page();
        }

        try
        {
            await _db.UpdateCategoryAsync(id, new UpdateCategoryRequest
            {
                CategoryName = CategoryName,
                IsActive     = IsActive,
            });
            return RedirectToPage("/Categories/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Category     = await _db.GetCategoryByIdAsync(id);
            return Page();
        }
    }
}
