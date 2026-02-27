namespace ExpenseApp.Models;

public class Role
{
    public int    RoleId      { get; set; }
    public string RoleName    { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class User
{
    public int     UserId      { get; set; }
    public string  UserName    { get; set; } = string.Empty;
    public string  Email       { get; set; } = string.Empty;
    public int     RoleId      { get; set; }
    public string  RoleName    { get; set; } = string.Empty;
    public int?    ManagerId   { get; set; }
    public string? ManagerName { get; set; }
    public bool    IsActive    { get; set; } = true;
    public DateTime CreatedAt  { get; set; }
}

public class CreateUserRequest
{
    public string UserName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public int    RoleId    { get; set; }
    public int?   ManagerId { get; set; }
}

public class UpdateUserRequest
{
    public string UserName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public int    RoleId    { get; set; }
    public int?   ManagerId { get; set; }
    public bool   IsActive  { get; set; } = true;
}

public class ExpenseCategory
{
    public int    CategoryId   { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool   IsActive     { get; set; } = true;
}

public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
}

public class UpdateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public bool   IsActive     { get; set; } = true;
}

public class ExpenseStatus
{
    public int    StatusId   { get; set; }
    public string StatusName { get; set; } = string.Empty;
}

public class Expense
{
    public int      ExpenseId      { get; set; }
    public int      UserId         { get; set; }
    public string   UserName       { get; set; } = string.Empty;
    public int      CategoryId     { get; set; }
    public string   CategoryName   { get; set; } = string.Empty;
    public int      StatusId       { get; set; }
    public string   StatusName     { get; set; } = string.Empty;
    public int      AmountMinor    { get; set; }  // pence
    public string   Currency       { get; set; } = "GBP";
    public decimal  AmountDecimal  { get; set; }
    public DateTime ExpenseDate    { get; set; }
    public string?  Description    { get; set; }
    public string?  ReceiptFile    { get; set; }
    public DateTime? SubmittedAt   { get; set; }
    public int?     ReviewedBy     { get; set; }
    public string?  ReviewerName   { get; set; }
    public DateTime? ReviewedAt    { get; set; }
    public DateTime CreatedAt      { get; set; }

    /// <summary>Formatted amount string e.g. "£12.34"</summary>
    public string AmountFormatted => $"£{AmountDecimal:F2}";
}

public class CreateExpenseRequest
{
    public int     UserId      { get; set; }
    public int     CategoryId  { get; set; }
    public int     AmountMinor { get; set; }
    public string  Currency    { get; set; } = "GBP";
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptFile { get; set; }
}

public class UpdateExpenseRequest
{
    public int     CategoryId  { get; set; }
    public int     AmountMinor { get; set; }
    public string  Currency    { get; set; } = "GBP";
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptFile { get; set; }
}

public class ApproveRejectRequest
{
    public int ReviewedBy { get; set; }
}

public class ExpenseSummary
{
    public string  StatusName       { get; set; } = string.Empty;
    public int     TotalCount       { get; set; }
    public long    TotalAmountMinor { get; set; }  // long to avoid overflow on large datasets
    public decimal TotalAmountGBP   { get; set; }
}
