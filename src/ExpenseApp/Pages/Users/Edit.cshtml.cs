using ExpenseApp.Data;
using ExpenseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseApp.Pages.Users;

public class EditModel : PageModel
{
    private readonly IExpenseDatabase _db;

    public new User?  User         { get; private set; }
    public List<Role> Roles        { get; private set; } = [];
    public List<User> Managers     { get; private set; } = [];
    public string?    ErrorMessage { get; private set; }

    public EditModel(IExpenseDatabase db) => _db = db;

    public async Task OnGetAsync(int id)
    {
        User     = await _db.GetUserByIdAsync(id);
        Roles    = await _db.GetRolesAsync();
        Managers = (await _db.GetUsersAsync()).Where(u => u.UserId != id && u.RoleName == "Manager").ToList();
    }

    public async Task<IActionResult> OnPostAsync(
        int     id,
        string  UserName,
        string  Email,
        int     RoleId,
        int?    ManagerId,
        bool    IsActive)
    {
        try
        {
            await _db.UpdateUserAsync(id, new UpdateUserRequest
            {
                UserName  = UserName,
                Email     = Email,
                RoleId    = RoleId,
                ManagerId = ManagerId,
                IsActive  = IsActive,
            });
            return RedirectToPage("/Users/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            User     = await _db.GetUserByIdAsync(id);
            Roles    = await _db.GetRolesAsync();
            Managers = (await _db.GetUsersAsync()).Where(u => u.UserId != id && u.RoleName == "Manager").ToList();
            return Page();
        }
    }
}
