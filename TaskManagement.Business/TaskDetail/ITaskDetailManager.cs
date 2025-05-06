using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Business.TaskDetail;

public interface ITaskDetailManager
{
    Task<TaskDetails> GetByIdAsync(int id);
    Task<IEnumerable<TaskDetails>> GetByTaskIdAsync(int taskId);
    Task<TaskDetails> CreateAsync(CreateTaskDetailDto taskDetailDto);
    Task UpdateAsync(int id, UpdateTaskDetailDto taskDetailDto);
    Task DeleteAsync(int id);
}
