/*
  stored-procedures.sql
  CRUD stored procedures for the Expense Management System.
  All application data access MUST go through these procedures – no direct
  table access or ad-hoc SQL is permitted in the application layer.

  Tables covered:
    dbo.Roles
    dbo.Users
    dbo.ExpenseCategories
    dbo.ExpenseStatus
    dbo.Expenses
*/

SET NOCOUNT ON;
GO

-- ═══════════════════════════════════════════════════════════════════════════════
--  ROLES
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE OR ALTER PROCEDURE dbo.GetRoles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description
    FROM   dbo.Roles
    ORDER  BY RoleName;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetRoleById
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName, Description
    FROM   dbo.Roles
    WHERE  RoleId = @RoleId;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
--  USERS
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE OR ALTER PROCEDURE dbo.GetUsers
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId,
           u.UserName,
           u.Email,
           u.RoleId,
           r.RoleName,
           u.ManagerId,
           m.UserName  AS ManagerName,
           u.IsActive,
           u.CreatedAt
    FROM   dbo.Users  u
    JOIN   dbo.Roles  r ON u.RoleId    = r.RoleId
    LEFT   JOIN dbo.Users m ON u.ManagerId = m.UserId
    WHERE  (@ActiveOnly = 0 OR u.IsActive = 1)
    ORDER  BY u.UserName;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId,
           u.UserName,
           u.Email,
           u.RoleId,
           r.RoleName,
           u.ManagerId,
           m.UserName  AS ManagerName,
           u.IsActive,
           u.CreatedAt
    FROM   dbo.Users  u
    JOIN   dbo.Roles  r ON u.RoleId    = r.RoleId
    LEFT   JOIN dbo.Users m ON u.ManagerId = m.UserId
    WHERE  u.UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateUser
    @UserName  NVARCHAR(100),
    @Email     NVARCHAR(255),
    @RoleId    INT,
    @ManagerId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
    BEGIN
        RAISERROR('A user with this email address already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Users (UserName, Email, RoleId, ManagerId)
    VALUES (@UserName, @Email, @RoleId, @ManagerId);

    SELECT SCOPE_IDENTITY() AS UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateUser
    @UserId    INT,
    @UserName  NVARCHAR(100),
    @Email     NVARCHAR(255),
    @RoleId    INT,
    @ManagerId INT    = NULL,
    @IsActive  BIT    = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @UserId)
    BEGIN
        RAISERROR('User not found.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND UserId <> @UserId)
    BEGIN
        RAISERROR('Another user with this email address already exists.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Users
    SET    UserName  = @UserName,
           Email     = @Email,
           RoleId    = @RoleId,
           ManagerId = @ManagerId,
           IsActive  = @IsActive
    WHERE  UserId = @UserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Soft delete: mark inactive rather than physically removing the row so
    -- historical expense records remain intact.
    UPDATE dbo.Users
    SET    IsActive = 0
    WHERE  UserId = @UserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
--  EXPENSE CATEGORIES
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE OR ALTER PROCEDURE dbo.GetExpenseCategories
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, CategoryName, IsActive
    FROM   dbo.ExpenseCategories
    WHERE  (@ActiveOnly = 0 OR IsActive = 1)
    ORDER  BY CategoryName;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseCategoryById
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId, CategoryName, IsActive
    FROM   dbo.ExpenseCategories
    WHERE  CategoryId = @CategoryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateExpenseCategory
    @CategoryName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ExpenseCategories WHERE CategoryName = @CategoryName)
    BEGIN
        RAISERROR('A category with this name already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.ExpenseCategories (CategoryName)
    VALUES (@CategoryName);

    SELECT SCOPE_IDENTITY() AS CategoryId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateExpenseCategory
    @CategoryId   INT,
    @CategoryName NVARCHAR(100),
    @IsActive     BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.ExpenseCategories WHERE CategoryId = @CategoryId)
    BEGIN
        RAISERROR('Expense category not found.', 16, 1);
        RETURN;
    END

    UPDATE dbo.ExpenseCategories
    SET    CategoryName = @CategoryName,
           IsActive     = @IsActive
    WHERE  CategoryId = @CategoryId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteExpenseCategory
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Soft delete to preserve historical data
    UPDATE dbo.ExpenseCategories
    SET    IsActive = 0
    WHERE  CategoryId = @CategoryId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
--  EXPENSE STATUS (lookup – read-only from app layer)
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE OR ALTER PROCEDURE dbo.GetExpenseStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusId, StatusName
    FROM   dbo.ExpenseStatus
    ORDER  BY StatusId;
END
GO

-- ═══════════════════════════════════════════════════════════════════════════════
--  EXPENSES
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE OR ALTER PROCEDURE dbo.GetExpenses
    @UserId     INT  = NULL,   -- filter by submitting user (NULL = all)
    @StatusId   INT  = NULL,   -- filter by status (NULL = all)
    @PageNumber INT  = 1,
    @PageSize   INT  = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT e.ExpenseId,
           e.UserId,
           u.UserName,
           e.CategoryId,
           c.CategoryName,
           e.StatusId,
           s.StatusName,
           e.AmountMinor,
           e.Currency,
           CAST(e.AmountMinor / 100.0 AS DECIMAL(12,2)) AS AmountDecimal,
           e.ExpenseDate,
           e.Description,
           e.ReceiptFile,
           e.SubmittedAt,
           e.ReviewedBy,
           rv.UserName  AS ReviewerName,
           e.ReviewedAt,
           e.CreatedAt
    FROM   dbo.Expenses          e
    JOIN   dbo.Users             u  ON e.UserId     = u.UserId
    JOIN   dbo.ExpenseCategories c  ON e.CategoryId = c.CategoryId
    JOIN   dbo.ExpenseStatus     s  ON e.StatusId   = s.StatusId
    LEFT   JOIN dbo.Users        rv ON e.ReviewedBy = rv.UserId
    WHERE  (@UserId   IS NULL OR e.UserId   = @UserId)
    AND    (@StatusId IS NULL OR e.StatusId = @StatusId)
    ORDER  BY e.CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH  NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseById
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT e.ExpenseId,
           e.UserId,
           u.UserName,
           e.CategoryId,
           c.CategoryName,
           e.StatusId,
           s.StatusName,
           e.AmountMinor,
           e.Currency,
           CAST(e.AmountMinor / 100.0 AS DECIMAL(12,2)) AS AmountDecimal,
           e.ExpenseDate,
           e.Description,
           e.ReceiptFile,
           e.SubmittedAt,
           e.ReviewedBy,
           rv.UserName  AS ReviewerName,
           e.ReviewedAt,
           e.CreatedAt
    FROM   dbo.Expenses          e
    JOIN   dbo.Users             u  ON e.UserId     = u.UserId
    JOIN   dbo.ExpenseCategories c  ON e.CategoryId = c.CategoryId
    JOIN   dbo.ExpenseStatus     s  ON e.StatusId   = s.StatusId
    LEFT   JOIN dbo.Users        rv ON e.ReviewedBy = rv.UserId
    WHERE  e.ExpenseId = @ExpenseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.CreateExpense
    @UserId      INT,
    @CategoryId  INT,
    @AmountMinor INT,           -- amount in minor currency units (pence)
    @Currency    NVARCHAR(3) = 'GBP',
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @ReceiptFile NVARCHAR(500)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- New expenses always start in 'Draft' status
    DECLARE @DraftStatusId INT = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');

    IF @DraftStatusId IS NULL
    BEGIN
        RAISERROR('ExpenseStatus lookup data is missing. Please run the schema import.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Expenses (UserId, CategoryId, StatusId, AmountMinor, Currency, ExpenseDate, Description, ReceiptFile)
    VALUES (@UserId, @CategoryId, @DraftStatusId, @AmountMinor, @Currency, @ExpenseDate, @Description, @ReceiptFile);

    SELECT SCOPE_IDENTITY() AS ExpenseId;
END
GO

CREATE OR ALTER PROCEDURE dbo.UpdateExpense
    @ExpenseId   INT,
    @CategoryId  INT,
    @AmountMinor INT,
    @Currency    NVARCHAR(3)    = 'GBP',
    @ExpenseDate DATE,
    @Description NVARCHAR(1000) = NULL,
    @ReceiptFile NVARCHAR(500)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Only Draft expenses may be edited
    DECLARE @CurrentStatusName NVARCHAR(50);
    SELECT @CurrentStatusName = s.StatusName
    FROM   dbo.Expenses e
    JOIN   dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    WHERE  e.ExpenseId = @ExpenseId;

    IF @CurrentStatusName IS NULL
    BEGIN
        RAISERROR('Expense not found.', 16, 1);
        RETURN;
    END

    IF @CurrentStatusName <> 'Draft'
    BEGIN
        RAISERROR('Only expenses in Draft status can be edited.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Expenses
    SET    CategoryId  = @CategoryId,
           AmountMinor = @AmountMinor,
           Currency    = @Currency,
           ExpenseDate = @ExpenseDate,
           Description = @Description,
           ReceiptFile = @ReceiptFile
    WHERE  ExpenseId = @ExpenseId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.DeleteExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Only Draft expenses may be deleted
    DECLARE @CurrentStatusName NVARCHAR(50);
    SELECT @CurrentStatusName = s.StatusName
    FROM   dbo.Expenses e
    JOIN   dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    WHERE  e.ExpenseId = @ExpenseId;

    IF @CurrentStatusName IS NULL
    BEGIN
        RAISERROR('Expense not found.', 16, 1);
        RETURN;
    END

    IF @CurrentStatusName <> 'Draft'
    BEGIN
        RAISERROR('Only Draft expenses can be deleted.', 16, 1);
        RETURN;
    END

    DELETE FROM dbo.Expenses WHERE ExpenseId = @ExpenseId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.SubmitExpense
    @ExpenseId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DraftStatusId     INT = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Draft');
    DECLARE @SubmittedStatusId INT = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');

    IF NOT EXISTS (SELECT 1 FROM dbo.Expenses WHERE ExpenseId = @ExpenseId AND StatusId = @DraftStatusId)
    BEGIN
        RAISERROR('Expense not found or is not in Draft status.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Expenses
    SET    StatusId    = @SubmittedStatusId,
           SubmittedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.ApproveExpense
    @ExpenseId   INT,
    @ReviewedBy  INT    -- UserId of the manager approving the expense
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SubmittedStatusId INT = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');
    DECLARE @ApprovedStatusId  INT = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Approved');

    IF NOT EXISTS (SELECT 1 FROM dbo.Expenses WHERE ExpenseId = @ExpenseId AND StatusId = @SubmittedStatusId)
    BEGIN
        RAISERROR('Expense not found or is not in Submitted status.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Expenses
    SET    StatusId   = @ApprovedStatusId,
           ReviewedBy = @ReviewedBy,
           ReviewedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.RejectExpense
    @ExpenseId  INT,
    @ReviewedBy INT   -- UserId of the manager rejecting the expense
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SubmittedStatusId INT = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Submitted');
    DECLARE @RejectedStatusId  INT = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Rejected');

    IF NOT EXISTS (SELECT 1 FROM dbo.Expenses WHERE ExpenseId = @ExpenseId AND StatusId = @SubmittedStatusId)
    BEGIN
        RAISERROR('Expense not found or is not in Submitted status.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Expenses
    SET    StatusId   = @RejectedStatusId,
           ReviewedBy = @ReviewedBy,
           ReviewedAt = SYSUTCDATETIME()
    WHERE  ExpenseId = @ExpenseId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE dbo.GetExpenseSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.StatusName,
           COUNT(*)                                          AS TotalCount,
           CAST(SUM(CAST(e.AmountMinor AS BIGINT)) AS BIGINT) AS TotalAmountMinor,
           CAST(SUM(CAST(e.AmountMinor AS BIGINT)) / 100.0 AS DECIMAL(18,2)) AS TotalAmountGBP
    FROM   dbo.Expenses     e
    JOIN   dbo.ExpenseStatus s ON e.StatusId = s.StatusId
    GROUP  BY s.StatusId, s.StatusName
    ORDER  BY s.StatusId;
END
GO
