# Task Management System (ASP.NET Core + JWT + SQL Server)

A robust Task Management System with **Admin** and **User** roles. The project includes full authentication with **Access** and **Refresh tokens**, token-based authorization, task assignment, comments, and filtering support using a stored procedure.

## 🔧 Tech Stack

- ASP.NET Core Web API  
- Entity Framework Core  
- SQL Server  
- JWT Authentication  
- Swagger for API Testing  
- Flurl (for frontend API calls)  
- Stored Procedures (for complex filtering)

---

## 🚀 Features

### 🔐 Authentication & Authorization
- JWT-based authentication with access and refresh tokens.
- Login and Register APIs.
- Role-based access: **Admin** and **User**.

### 👤 Roles

#### Admin
- Create and manage users.
- Full CRUD on all tasks.
- Assign tasks to users.
- View and change task status.
- Add/view comments on all tasks.

#### User
- Create, read, update, delete **own tasks**.
- View tasks assigned to them by the admin.
- Change task status for assigned tasks.
- Add/view comments on own or assigned tasks.

---

## ⚙️ Setup Instructions


1. Unzip File 


2. Configure Database Connection
Go to appsettings.json inside the API project.

Update the connection string under ConnectionStrings:DefaultConnection.

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=TM;Trusted_Connection=True;"
}


📜 Add Stored Procedure After Migration
After running the migration and the database is ready, open SQL Server Management Studio (SSMS) and run the following stored procedure script:

sql
Copy
Edit
USE [TM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_FilterTasks]
    @IncludeDeleted BIT = 0,
    @CreatedById INT = NULL,
    @AssignedToId INT = NULL,
    @StatusId INT = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @SearchTerm NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.Id,
        t.Title,
        t.Description,
        t.CreatedAt,
        t.DueDate,
        t.IsDeleted,
        t.UserId,
        u.UserName,
        t.CreatedById,
        cb.UserName AS CreatedByName,
        t.TaskStatusId,
        ts.Name AS TaskStatus
    FROM 
        Tasks t
    LEFT JOIN 
        Users u ON t.UserId = u.Id
    LEFT JOIN 
        Users cb ON t.CreatedById = cb.Id
    LEFT JOIN 
        TaskStates ts ON t.TaskStatusId = ts.Id
    WHERE 
        (@IncludeDeleted = 1 OR t.IsDeleted = 0)
        AND (@CreatedById IS NULL OR t.CreatedById = @CreatedById)
        AND (@AssignedToId IS NULL OR t.UserId = @AssignedToId)
        AND (@StatusId IS NULL OR t.TaskStatusId = @StatusId)
        AND (@StartDate IS NULL OR t.DueDate >= @StartDate)
        AND (@EndDate IS NULL OR t.DueDate <= @EndDate)
        AND (@SearchTerm IS NULL OR 
             t.Title LIKE '%' + @SearchTerm + '%' OR 
             t.Description LIKE '%' + @SearchTerm + '%')
    ORDER BY 
        t.DueDate ASC, t.CreatedAt DESC
END




