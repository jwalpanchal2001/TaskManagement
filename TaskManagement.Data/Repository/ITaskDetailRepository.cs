using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Entity.Model;

namespace TaskManagement.Data.Repository
{
    public interface ITaskDetailRepository
    {
        Task<TaskDetail> GetByIdAsync(int id);
        Task<IEnumerable<TaskDetail>> GetByTaskIdAsync(int taskId);
        Task<TaskDetail> AddAsync(TaskDetail taskDetail);
        Task UpdateAsync(TaskDetail taskDetail);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
