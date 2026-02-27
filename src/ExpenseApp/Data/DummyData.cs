using ExpenseApp.Models;

namespace ExpenseApp.Data;

/// <summary>
/// Static fallback data returned when the database is unreachable.
/// Allows the UI to render gracefully and show a helpful error banner.
/// </summary>
internal static class DummyData
{
    internal static List<Role> Roles =>
    [
        new() { RoleId = 1, RoleName = "Employee", Description = "Regular employee who can submit expenses" },
        new() { RoleId = 2, RoleName = "Manager",  Description = "Can approve/reject submitted expenses" },
    ];

    internal static List<User> Users =>
    [
        new() { UserId = 1, UserName = "Alice Example",  Email = "alice@example.co.uk",       RoleId = 1, RoleName = "Employee", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
        new() { UserId = 2, UserName = "Bob Manager",    Email = "bob.manager@example.co.uk", RoleId = 2, RoleName = "Manager",  IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-60) },
    ];

    internal static List<ExpenseCategory> Categories =>
    [
        new() { CategoryId = 1, CategoryName = "Travel",        IsActive = true },
        new() { CategoryId = 2, CategoryName = "Meals",         IsActive = true },
        new() { CategoryId = 3, CategoryName = "Supplies",      IsActive = true },
        new() { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
        new() { CategoryId = 5, CategoryName = "Other",         IsActive = true },
    ];

    internal static List<ExpenseStatus> Statuses =>
    [
        new() { StatusId = 1, StatusName = "Draft" },
        new() { StatusId = 2, StatusName = "Submitted" },
        new() { StatusId = 3, StatusName = "Approved" },
        new() { StatusId = 4, StatusName = "Rejected" },
    ];

    internal static List<Expense> Expenses =>
    [
        new()
        {
            ExpenseId    = 1,
            UserId       = 1,
            UserName     = "Alice Example",
            CategoryId   = 1,
            CategoryName = "Travel",
            StatusId     = 2,
            StatusName   = "Submitted",
            AmountMinor  = 2540,
            AmountDecimal = 25.40m,
            Currency     = "GBP",
            ExpenseDate  = DateTime.UtcNow.AddDays(-10),
            Description  = "Taxi from airport to client site",
            SubmittedAt  = DateTime.UtcNow.AddDays(-9),
            CreatedAt    = DateTime.UtcNow.AddDays(-10),
        },
        new()
        {
            ExpenseId    = 2,
            UserId       = 1,
            UserName     = "Alice Example",
            CategoryId   = 2,
            CategoryName = "Meals",
            StatusId     = 3,
            StatusName   = "Approved",
            AmountMinor  = 1425,
            AmountDecimal = 14.25m,
            Currency     = "GBP",
            ExpenseDate  = DateTime.UtcNow.AddDays(-45),
            Description  = "Client lunch meeting",
            SubmittedAt  = DateTime.UtcNow.AddDays(-44),
            ReviewedBy   = 2,
            ReviewerName = "Bob Manager",
            ReviewedAt   = DateTime.UtcNow.AddDays(-43),
            CreatedAt    = DateTime.UtcNow.AddDays(-45),
        },
        new()
        {
            ExpenseId    = 3,
            UserId       = 1,
            UserName     = "Alice Example",
            CategoryId   = 3,
            CategoryName = "Supplies",
            StatusId     = 1,
            StatusName   = "Draft",
            AmountMinor  = 799,
            AmountDecimal = 7.99m,
            Currency     = "GBP",
            ExpenseDate  = DateTime.UtcNow.AddDays(-2),
            Description  = "Office stationery",
            CreatedAt    = DateTime.UtcNow.AddDays(-2),
        },
    ];

    internal static List<ExpenseSummary> Summary =>
    [
        new() { StatusName = "Draft",     TotalCount = 1, TotalAmountMinor =   799L, TotalAmountGBP =   7.99m },
        new() { StatusName = "Submitted", TotalCount = 1, TotalAmountMinor =  2540L, TotalAmountGBP =  25.40m },
        new() { StatusName = "Approved",  TotalCount = 1, TotalAmountMinor =  1425L, TotalAmountGBP =  14.25m },
        new() { StatusName = "Rejected",  TotalCount = 0, TotalAmountMinor =     0L, TotalAmountGBP =   0.00m },
    ];
}
