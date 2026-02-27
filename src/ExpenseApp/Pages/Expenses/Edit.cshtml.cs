using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Expenses;

public class EditModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public Expense?              Expense      { get; private set; }
    public List<ExpenseCategory> Categories   { get; private set; } = [];
    public string?               ErrorMessage { get; private set; }

    public EditModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync(int id)
    {
        Expense    = await _db.GetExpenseByIdAsync(id);
        Categories = await _db.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync(
        int      id,
        int      CategoryId,
        decimal  AmountGBP,
        DateTime ExpenseDate,
        string?  Description)
    {
        if (AmountGBP <= 0)
        {
            ErrorMessage = "Amount must be greater than zero.";
            Expense    = await _db.GetExpenseByIdAsync(id);
            Categories = await _db.GetCategoriesAsync();
            return Page();
        }

        try
        {
            var request = new UpdateExpenseRequest
            {
                CategoryId  = CategoryId,
                AmountMinor = (int)Math.Round(AmountGBP * 100),
                Currency    = "GBP",
                ExpenseDate = ExpenseDate,
                Description = Description,
            };
            await _db.UpdateExpenseAsync(id, request);
            return RedirectToPage("/Expenses/Detail", new { id });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Expense    = await _db.GetExpenseByIdAsync(id);
            Categories = await _db.GetCategoriesAsync();
            return Page();
        }
    }
}
