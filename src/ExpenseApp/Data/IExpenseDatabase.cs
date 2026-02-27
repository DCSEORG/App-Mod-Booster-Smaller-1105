using ExpenseApp.Models;

namespace ExpenseApp.Data;

/// <summary>
/// All database interactions for the Expense Management application.
/// Every method calls a stored procedure – no ad-hoc SQL is permitted.
/// </summary>
public interface IExpenseDatabase
{
    // Roles
    Task<List<Role>> GetRolesAsync();
    Task<Role?> GetRoleByIdAsync(int roleId);

    // Users
    Task<List<User>> GetUsersAsync(bool activeOnly = true);
    Task<User?> GetUserByIdAsync(int userId);
    Task<int> CreateUserAsync(CreateUserRequest request);
    Task<int> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task<int> DeleteUserAsync(int userId);

    // Categories
    Task<List<ExpenseCategory>> GetCategoriesAsync(bool activeOnly = true);
    Task<ExpenseCategory?> GetCategoryByIdAsync(int categoryId);
    Task<int> CreateCategoryAsync(CreateCategoryRequest request);
    Task<int> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request);
    Task<int> DeleteCategoryAsync(int categoryId);

    // Statuses
    Task<List<ExpenseStatus>> GetExpenseStatusesAsync();

    // Expenses
    Task<List<Expense>> GetExpensesAsync(int? userId = null, int? statusId = null, int page = 1, int pageSize = 50);
    Task<Expense?> GetExpenseByIdAsync(int expenseId);
    Task<int> CreateExpenseAsync(CreateExpenseRequest request);
    Task<int> UpdateExpenseAsync(int expenseId, UpdateExpenseRequest request);
    Task<int> DeleteExpenseAsync(int expenseId);
    Task<int> SubmitExpenseAsync(int expenseId);
    Task<int> ApproveExpenseAsync(int expenseId, int reviewedBy);
    Task<int> RejectExpenseAsync(int expenseId, int reviewedBy);
    Task<List<ExpenseSummary>> GetExpenseSummaryAsync();

    // Diagnostics
    string? LastError { get; }
}
