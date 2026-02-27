using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Categories;

public class CreateModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public string? ErrorMessage { get; private set; }

    public CreateModel(IExpenseDatabase db) => _db = db;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string CategoryName)
    {
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            ErrorMessage = "Category name is required.";
            return Page();
        }

        try
        {
            await _db.CreateCategoryAsync(new CreateCategoryRequest { CategoryName = CategoryName });
            return RedirectToPage("/Categories/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
