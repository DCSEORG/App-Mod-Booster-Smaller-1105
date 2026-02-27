using System.Data;
using ExpenseApp.Models;
using Microsoft.Data.SqlClient;

namespace ExpenseApp.Data;

/// <summary>
/// Concrete implementation of <see cref="IExpenseDatabase"/>.
/// Uses Microsoft.Data.SqlClient with Azure AD authentication (managed identity in
/// production, Active Directory Default for local development).
/// All data access goes through named stored procedures – no inline SQL.
/// </summary>
public class ExpenseDatabase : IExpenseDatabase
{
    private readonly string _connectionString;
    private readonly ILogger<ExpenseDatabase> _logger;

    public string? LastError { get; private set; }

    public ExpenseDatabase(IConfiguration configuration, ILogger<ExpenseDatabase> logger)
    {
        _logger           = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
    }

    // ── Connection helper ────────────────────────────────────────────────────

    private async Task<SqlConnection> OpenAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        LastError = null;
        return conn;
    }

    private SqlCommand CreateProc(SqlConnection conn, string procedureName)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandType    = CommandType.StoredProcedure;
        cmd.CommandText    = procedureName;
        cmd.CommandTimeout = 30;
        return cmd;
    }

    // ── Roles ────────────────────────────────────────────────────────────────

    public async Task<List<Role>> GetRolesAsync()
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetRoles");
            await using var rdr  = await cmd.ExecuteReaderAsync();
            var results = new List<Role>();
            while (await rdr.ReadAsync())
                results.Add(ReadRole(rdr));
            return results;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetRoles");
            _logger.LogError(ex, "GetRoles failed");
            return DummyData.Roles;
        }
    }

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetRoleById");
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            await using var rdr = await cmd.ExecuteReaderAsync();
            return await rdr.ReadAsync() ? ReadRole(rdr) : null;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetRoleById");
            _logger.LogError(ex, "GetRoleById failed for RoleId={RoleId}", roleId);
            return DummyData.Roles.FirstOrDefault(r => r.RoleId == roleId);
        }
    }

    private static Role ReadRole(SqlDataReader r) => new()
    {
        RoleId      = r.GetInt32(r.GetOrdinal("RoleId")),
        RoleName    = r.GetString(r.GetOrdinal("RoleName")),
        Description = r.IsDBNull(r.GetOrdinal("Description")) ? "" : r.GetString(r.GetOrdinal("Description")),
    };

    // ── Users ────────────────────────────────────────────────────────────────

    public async Task<List<User>> GetUsersAsync(bool activeOnly = true)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetUsers");
            cmd.Parameters.AddWithValue("@ActiveOnly", activeOnly ? 1 : 0);
            await using var rdr  = await cmd.ExecuteReaderAsync();
            var results = new List<User>();
            while (await rdr.ReadAsync())
                results.Add(ReadUser(rdr));
            return results;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetUsers");
            _logger.LogError(ex, "GetUsers failed");
            return DummyData.Users;
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetUserById");
            cmd.Parameters.AddWithValue("@UserId", userId);
            await using var rdr = await cmd.ExecuteReaderAsync();
            return await rdr.ReadAsync() ? ReadUser(rdr) : null;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetUserById");
            _logger.LogError(ex, "GetUserById failed for UserId={UserId}", userId);
            return DummyData.Users.FirstOrDefault(u => u.UserId == userId);
        }
    }

    public async Task<int> CreateUserAsync(CreateUserRequest req)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.CreateUser");
        cmd.Parameters.AddWithValue("@UserName",  req.UserName);
        cmd.Parameters.AddWithValue("@Email",     req.Email);
        cmd.Parameters.AddWithValue("@RoleId",    req.RoleId);
        cmd.Parameters.AddWithValue("@ManagerId", (object?)req.ManagerId ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> UpdateUserAsync(int userId, UpdateUserRequest req)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.UpdateUser");
        cmd.Parameters.AddWithValue("@UserId",    userId);
        cmd.Parameters.AddWithValue("@UserName",  req.UserName);
        cmd.Parameters.AddWithValue("@Email",     req.Email);
        cmd.Parameters.AddWithValue("@RoleId",    req.RoleId);
        cmd.Parameters.AddWithValue("@ManagerId", (object?)req.ManagerId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive",  req.IsActive ? 1 : 0);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> DeleteUserAsync(int userId)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.DeleteUser");
        cmd.Parameters.AddWithValue("@UserId", userId);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static User ReadUser(SqlDataReader r) => new()
    {
        UserId      = r.GetInt32(r.GetOrdinal("UserId")),
        UserName    = r.GetString(r.GetOrdinal("UserName")),
        Email       = r.GetString(r.GetOrdinal("Email")),
        RoleId      = r.GetInt32(r.GetOrdinal("RoleId")),
        RoleName    = r.GetString(r.GetOrdinal("RoleName")),
        ManagerId   = r.IsDBNull(r.GetOrdinal("ManagerId"))   ? null : r.GetInt32(r.GetOrdinal("ManagerId")),
        ManagerName = r.IsDBNull(r.GetOrdinal("ManagerName")) ? null : r.GetString(r.GetOrdinal("ManagerName")),
        IsActive    = r.GetBoolean(r.GetOrdinal("IsActive")),
        CreatedAt   = r.GetDateTime(r.GetOrdinal("CreatedAt")),
    };

    // ── Categories ───────────────────────────────────────────────────────────

    public async Task<List<ExpenseCategory>> GetCategoriesAsync(bool activeOnly = true)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetExpenseCategories");
            cmd.Parameters.AddWithValue("@ActiveOnly", activeOnly ? 1 : 0);
            await using var rdr  = await cmd.ExecuteReaderAsync();
            var results = new List<ExpenseCategory>();
            while (await rdr.ReadAsync())
                results.Add(ReadCategory(rdr));
            return results;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetExpenseCategories");
            _logger.LogError(ex, "GetExpenseCategories failed");
            return DummyData.Categories;
        }
    }

    public async Task<ExpenseCategory?> GetCategoryByIdAsync(int categoryId)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetExpenseCategoryById");
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            await using var rdr = await cmd.ExecuteReaderAsync();
            return await rdr.ReadAsync() ? ReadCategory(rdr) : null;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetExpenseCategoryById");
            _logger.LogError(ex, "GetCategoryById failed");
            return DummyData.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
        }
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryRequest req)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.CreateExpenseCategory");
        cmd.Parameters.AddWithValue("@CategoryName", req.CategoryName);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest req)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.UpdateExpenseCategory");
        cmd.Parameters.AddWithValue("@CategoryId",   categoryId);
        cmd.Parameters.AddWithValue("@CategoryName", req.CategoryName);
        cmd.Parameters.AddWithValue("@IsActive",     req.IsActive ? 1 : 0);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> DeleteCategoryAsync(int categoryId)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.DeleteExpenseCategory");
        cmd.Parameters.AddWithValue("@CategoryId", categoryId);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static ExpenseCategory ReadCategory(SqlDataReader r) => new()
    {
        CategoryId   = r.GetInt32(r.GetOrdinal("CategoryId")),
        CategoryName = r.GetString(r.GetOrdinal("CategoryName")),
        IsActive     = r.GetBoolean(r.GetOrdinal("IsActive")),
    };

    // ── Expense Statuses ─────────────────────────────────────────────────────

    public async Task<List<ExpenseStatus>> GetExpenseStatusesAsync()
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetExpenseStatuses");
            await using var rdr  = await cmd.ExecuteReaderAsync();
            var results = new List<ExpenseStatus>();
            while (await rdr.ReadAsync())
                results.Add(new ExpenseStatus
                {
                    StatusId   = rdr.GetInt32(rdr.GetOrdinal("StatusId")),
                    StatusName = rdr.GetString(rdr.GetOrdinal("StatusName")),
                });
            return results;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetExpenseStatuses");
            _logger.LogError(ex, "GetExpenseStatuses failed");
            return DummyData.Statuses;
        }
    }

    // ── Expenses ─────────────────────────────────────────────────────────────

    public async Task<List<Expense>> GetExpensesAsync(int? userId = null, int? statusId = null, int page = 1, int pageSize = 50)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetExpenses");
            cmd.Parameters.AddWithValue("@UserId",     (object?)userId   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId",   (object?)statusId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageNumber", page);
            cmd.Parameters.AddWithValue("@PageSize",   pageSize);
            await using var rdr  = await cmd.ExecuteReaderAsync();
            var results = new List<Expense>();
            while (await rdr.ReadAsync())
                results.Add(ReadExpense(rdr));
            return results;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetExpenses");
            _logger.LogError(ex, "GetExpenses failed");
            return DummyData.Expenses;
        }
    }

    public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetExpenseById");
            cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
            await using var rdr = await cmd.ExecuteReaderAsync();
            return await rdr.ReadAsync() ? ReadExpense(rdr) : null;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetExpenseById");
            _logger.LogError(ex, "GetExpenseById failed for ExpenseId={ExpenseId}", expenseId);
            return DummyData.Expenses.FirstOrDefault(e => e.ExpenseId == expenseId);
        }
    }

    public async Task<int> CreateExpenseAsync(CreateExpenseRequest req)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.CreateExpense");
        cmd.Parameters.AddWithValue("@UserId",      req.UserId);
        cmd.Parameters.AddWithValue("@CategoryId",  req.CategoryId);
        cmd.Parameters.AddWithValue("@AmountMinor", req.AmountMinor);
        cmd.Parameters.AddWithValue("@Currency",    req.Currency);
        cmd.Parameters.AddWithValue("@ExpenseDate", req.ExpenseDate.Date);
        cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReceiptFile", (object?)req.ReceiptFile ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> UpdateExpenseAsync(int expenseId, UpdateExpenseRequest req)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.UpdateExpense");
        cmd.Parameters.AddWithValue("@ExpenseId",   expenseId);
        cmd.Parameters.AddWithValue("@CategoryId",  req.CategoryId);
        cmd.Parameters.AddWithValue("@AmountMinor", req.AmountMinor);
        cmd.Parameters.AddWithValue("@Currency",    req.Currency);
        cmd.Parameters.AddWithValue("@ExpenseDate", req.ExpenseDate.Date);
        cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReceiptFile", (object?)req.ReceiptFile ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> DeleteExpenseAsync(int expenseId)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.DeleteExpense");
        cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> SubmitExpenseAsync(int expenseId)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.SubmitExpense");
        cmd.Parameters.AddWithValue("@ExpenseId", expenseId);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> ApproveExpenseAsync(int expenseId, int reviewedBy)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.ApproveExpense");
        cmd.Parameters.AddWithValue("@ExpenseId",  expenseId);
        cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> RejectExpenseAsync(int expenseId, int reviewedBy)
    {
        await using var conn = await OpenAsync();
        await using var cmd  = CreateProc(conn, "dbo.RejectExpense");
        cmd.Parameters.AddWithValue("@ExpenseId",  expenseId);
        cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<ExpenseSummary>> GetExpenseSummaryAsync()
    {
        try
        {
            await using var conn = await OpenAsync();
            await using var cmd  = CreateProc(conn, "dbo.GetExpenseSummary");
            await using var rdr  = await cmd.ExecuteReaderAsync();
            var results = new List<ExpenseSummary>();
            while (await rdr.ReadAsync())
                results.Add(new ExpenseSummary
                {
                    StatusName       = rdr.GetString(rdr.GetOrdinal("StatusName")),
                    TotalCount       = rdr.GetInt32(rdr.GetOrdinal("TotalCount")),
                    TotalAmountMinor = rdr.IsDBNull(rdr.GetOrdinal("TotalAmountMinor")) ? 0L : rdr.GetInt64(rdr.GetOrdinal("TotalAmountMinor")),
                    TotalAmountGBP   = rdr.IsDBNull(rdr.GetOrdinal("TotalAmountGBP"))   ? 0 : rdr.GetDecimal(rdr.GetOrdinal("TotalAmountGBP")),
                });
            return results;
        }
        catch (Exception ex)
        {
            LastError = BuildError(ex, "GetExpenseSummary");
            _logger.LogError(ex, "GetExpenseSummary failed");
            return DummyData.Summary;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Expense ReadExpense(SqlDataReader r) => new()
    {
        ExpenseId     = r.GetInt32(r.GetOrdinal("ExpenseId")),
        UserId        = r.GetInt32(r.GetOrdinal("UserId")),
        UserName      = r.GetString(r.GetOrdinal("UserName")),
        CategoryId    = r.GetInt32(r.GetOrdinal("CategoryId")),
        CategoryName  = r.GetString(r.GetOrdinal("CategoryName")),
        StatusId      = r.GetInt32(r.GetOrdinal("StatusId")),
        StatusName    = r.GetString(r.GetOrdinal("StatusName")),
        AmountMinor   = r.GetInt32(r.GetOrdinal("AmountMinor")),
        Currency      = r.GetString(r.GetOrdinal("Currency")),
        AmountDecimal = r.GetDecimal(r.GetOrdinal("AmountDecimal")),
        ExpenseDate   = r.GetDateTime(r.GetOrdinal("ExpenseDate")),
        Description   = r.IsDBNull(r.GetOrdinal("Description"))  ? null : r.GetString(r.GetOrdinal("Description")),
        ReceiptFile   = r.IsDBNull(r.GetOrdinal("ReceiptFile"))   ? null : r.GetString(r.GetOrdinal("ReceiptFile")),
        SubmittedAt   = r.IsDBNull(r.GetOrdinal("SubmittedAt"))   ? null : r.GetDateTime(r.GetOrdinal("SubmittedAt")),
        ReviewedBy    = r.IsDBNull(r.GetOrdinal("ReviewedBy"))    ? null : r.GetInt32(r.GetOrdinal("ReviewedBy")),
        ReviewerName  = r.IsDBNull(r.GetOrdinal("ReviewerName"))  ? null : r.GetString(r.GetOrdinal("ReviewerName")),
        ReviewedAt    = r.IsDBNull(r.GetOrdinal("ReviewedAt"))    ? null : r.GetDateTime(r.GetOrdinal("ReviewedAt")),
        CreatedAt     = r.GetDateTime(r.GetOrdinal("CreatedAt")),
    };

    private static string BuildError(Exception ex, string context)
    {
        var isMiError = ex.Message.Contains("login", StringComparison.OrdinalIgnoreCase)
                     || ex.Message.Contains("principal", StringComparison.OrdinalIgnoreCase)
                     || ex.Message.Contains("token", StringComparison.OrdinalIgnoreCase)
                     || ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase);

        if (isMiError)
            return $"[{context}] Database authentication failed. " +
                   "Check that the managed identity is configured: ensure AZURE_CLIENT_ID is set in App Service " +
                   "and the managed identity has been granted access via run-sql-dbrole.py.";

        return $"[{context}] {ex.GetType().Name}: {ex.Message}";
    }
}
