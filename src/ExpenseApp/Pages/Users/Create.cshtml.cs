using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Users;

public class CreateModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public List<Role> Roles        { get; private set; } = [];
    public List<User> Managers     { get; private set; } = [];
    public string?    ErrorMessage { get; private set; }

    public CreateModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync()
    {
        Roles    = await _db.GetRolesAsync();
        Managers = (await _db.GetUsersAsync()).Where(u => u.RoleName == "Manager").ToList();
    }

    public async Task<IActionResult> OnPostAsync(
        string  UserName,
        string  Email,
        int     RoleId,
        int?    ManagerId)
    {
        try
        {
            var newId = await _db.CreateUserAsync(new CreateUserRequest
            {
                UserName  = UserName,
                Email     = Email,
                RoleId    = RoleId,
                ManagerId = ManagerId,
            });
            return RedirectToPage("/Users/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Roles    = await _db.GetRolesAsync();
            Managers = (await _db.GetUsersAsync()).Where(u => u.RoleName == "Manager").ToList();
            return Page();
        }
    }
}
