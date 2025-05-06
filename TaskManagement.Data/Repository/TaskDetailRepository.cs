using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Entity.Model;

namespace TaskManagement.Data.Repository;

public class TaskDetailRepository : ITaskDetailRepository
{
    private readonly ApplicationDbContext _context;

    public TaskDetailRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskDetail> GetByIdAsync(int id)
    {
        return await _context.TaskDetails
            .FirstOrDefaultAsync(td => td.Id == id && !td.IsDeleted);
    }

    public async Task<IEnumerable<TaskDetail>> GetByTaskIdAsync(int taskId)
    {
        return await _context.TaskDetails
            .Where(td => td.TaskId == taskId && !td.IsDeleted)
            .ToListAsync();
    }

    public async Task<TaskDetail> AddAsync(TaskDetail taskDetail)
    {
        await _context.TaskDetails.AddAsync(taskDetail);
        await _context.SaveChangesAsync();
        return taskDetail;
    }

    public async Task UpdateAsync(TaskDetail taskDetail)
    {
        _context.TaskDetails.Update(taskDetail);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var taskDetail = await GetByIdAsync(id);
        if (taskDetail != null)
        {
            taskDetail.IsDeleted = true;
            await UpdateAsync(taskDetail);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.TaskDetails
            .AnyAsync(td => td.Id == id && !td.IsDeleted);
    }
}
