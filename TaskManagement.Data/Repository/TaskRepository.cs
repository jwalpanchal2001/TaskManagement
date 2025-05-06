using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Entity.Model;

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




}