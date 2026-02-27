using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class CreateModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public List<User>            Users      { get; private set; } = [];
    public List<ExpenseCategory> Categories { get; private set; } = [];
    public string?               ErrorMessage { get; private set; }

    public CreateModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync()
    {
        Users      = await _db.GetUsersAsync();
        Categories = await _db.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync(
        int      UserId,
        int      CategoryId,
        decimal  AmountGBP,
        DateTime ExpenseDate,
        string?  Description)
    {
        if (AmountGBP <= 0)
        {
            ErrorMessage = "Amount must be greater than zero.";
            Users      = await _db.GetUsersAsync();
            Categories = await _db.GetCategoriesAsync();
            return Page();
        }

        try
        {
            var request = new CreateExpenseRequest
            {
                UserId      = UserId,
                CategoryId  = CategoryId,
                AmountMinor = (int)Math.Round(AmountGBP * 100),
                Currency    = "GBP",
                ExpenseDate = ExpenseDate,
                Description = Description,
            };
            var newId = await _db.CreateExpenseAsync(request);
            return RedirectToPage("/Expenses/Detail", new { id = newId });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Users      = await _db.GetUsersAsync();
            Categories = await _db.GetCategoriesAsync();
            return Page();
        }
    }
}
