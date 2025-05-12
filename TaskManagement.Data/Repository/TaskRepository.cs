using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Dto;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Data.Repository;
public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tasks> GetTaskByIdAsync(int id)
        {
        return await _context.Tasks
            .Include(t => t.User)
            .Include(t => t.TaskStatus)
            .Include(t => t.CreatedBy)
            .Include(t => t.TaskDetails)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }
    public async Task<IEnumerable<Tasks>> GetAllTasksAsync(bool includeDeleted = false)
    {
        var query = _context.Tasks
            .Include(t => t.User)
            .Include(t => t.CreatedBy)
            .Include(t => t.TaskStatus)
            //.Include(t => t.TaskDetails) // Include details in the list
            .AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(t => !t.IsDeleted);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Tasks>> GetTasksByUserIdAsync(int userId)
    {
        return await _context.Tasks
            .Include(t => t.TaskStatus)
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tasks>> GetTasksCreatedByUserAsync(int userId)
    {
        return await _context.Tasks
            .Include(t => t.TaskStatus)
            .Where(t => t.CreatedById == userId && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<Tasks> AddTaskAsync(Tasks task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(Tasks task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int id)
    {
        var task = await GetTaskByIdAsync(id);
        if (task != null)
        {
            task.IsDeleted = true;
            await UpdateTaskAsync(task);
        }
    }

    public async Task<bool> TaskExistsAsync(int id)
    {
        return await _context.Tasks.AnyAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<IEnumerable<TaskState>> GetAllTaskStatesAsync()
    {
        return await _context.TaskStates.Where(ts => !ts.IsDeleted).ToListAsync();
    }

    public async Task<bool> IsTaskUnassignedAsync(int taskId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        return task != null && task.TaskStatusId == 1;
    }

    public async Task AssignTaskToUserAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task == null)
            throw new KeyNotFoundException("Task not found");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        task.UserId = userId;
        task.TaskStatusId = 2; // Change status to "Todo"

        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task UnassignTaskAsync(int taskId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task == null)
            throw new KeyNotFoundException("Task not found");

        task.UserId = null;
        task.TaskStatusId = 1;

        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }



    public async Task<List<TaskDto1>> GetFilteredTasksAsync(TaskFilterModel filter)
    {
        try
        {
     

            // Step 1: Get task IDs from stored procedure
            var results = await _context.Set<TaskDto1>()
           .FromSqlRaw("EXEC sp_FilterTasks @IncludeDeleted = {0}, @CreatedById = {1}, @AssignedToId = {2}, @StatusId = {3}, @StartDate = {4}, @EndDate = {5}, @SearchTerm = {6}",
               filter.IncludeDeleted, filter.CreatedById, filter.AssignedToId, filter.StatusId, filter.StartDate, filter.EndDate, filter.SearchTerm)
           .ToListAsync();


            return results;
        }
        catch (Exception ex)
        {
            // Log exception
            throw;
        }
    }


    public async Task<bool> UpdateTaskStatus(int taskId, int statusId)
    {
        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Tasks SET TaskStatusId = {statusId} WHERE Id = {taskId}");

        return rowsAffected > 0;
    }

    //public async Task<IEnumerable<Tasks>> GetFilteredTasksAsync(
    // bool includeDeleted = false,
    // int? createdById = null,
    // int? assignedToId = null,
    // int? statusId = null,
    // DateTime? startDate = null,
    // DateTime? endDate = null,
    // string searchTerm = null)
    //{
    //    var query = _context.Tasks
    //        .Include(t => t.User)
    //        .Include(t => t.CreatedBy)
    //        .Include(t => t.TaskStatus)
    //        .Include(t => t.TaskDetails)
    //        .AsQueryable();

    //    // Apply filters
    //    if (!includeDeleted)
    //    {
    //        query = query.Where(t => !t.IsDeleted);
    //    }

    //    if (createdById.HasValue)
    //    {
    //        query = query.Where(t => t.CreatedById == createdById.Value);
    //    }

    //    if (assignedToId.HasValue)
    //    {
    //        query = query.Where(t => t.UserId == assignedToId.Value);
    //    }

    //    if (statusId.HasValue)
    //    {
    //        query = query.Where(t => t.TaskStatusId == statusId.Value);
    //    }

    //    if (startDate.HasValue)
    //    {
    //        query = query.Where(t => t.DueDate >= startDate.Value);
    //    }

    //    if (endDate.HasValue)
    //    {
    //        query = query.Where(t => t.DueDate <= endDate.Value);
    //    }

    //    if (!string.IsNullOrEmpty(searchTerm))
    //    {
    //        query = query.Where(t =>
    //            t.Title.Contains(searchTerm) ||
    //            t.Description.Contains(searchTerm));
    //    }

    //    // Ordering
    //    query = query
    //        .OrderBy(t => t.DueDate)
    //        .ThenByDescending(t => t.CreatedAt);

    //    return await query
    //        .AsNoTracking()
    //        .ToListAsync();
    //}
}