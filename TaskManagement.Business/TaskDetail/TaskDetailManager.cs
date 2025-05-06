using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using TaskManagement.Data.Repository;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Business.TaskDetail
{
    public class TaskDetailManager : ITaskDetailManager
    {
        private readonly ITaskDetailRepository _taskDetailRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskDetailManager(ITaskDetailRepository taskDetailRepository, ITaskRepository taskRepository)
        {
            _taskDetailRepository = taskDetailRepository;
            _taskRepository = taskRepository;
        }

        public async Task<TaskDetails> GetByIdAsync(int id)
        {
            var taskDetail = await _taskDetailRepository.GetByIdAsync(id);
            return taskDetail?.Adapt<TaskDetails>();
        }

        public async Task<IEnumerable<TaskDetails>> GetByTaskIdAsync(int taskId)
        {
            var taskDetails = await _taskDetailRepository.GetByTaskIdAsync(taskId);
            return taskDetails.Adapt<IEnumerable<TaskDetails>>();
        }

        public async Task<TaskDetails> CreateAsync(CreateTaskDetailDto taskDetailDto)
        {
            // Verify task exists
            if (!await _taskRepository.TaskExistsAsync(taskDetailDto.TaskId))
            {
                throw new KeyNotFoundException("Task not found");
            }

            var taskDetail = taskDetailDto.Adapt<TaskManagement.Entity.Model.TaskDetail>();
            taskDetail.CreatedAt = DateTime.UtcNow;

            var createdDetail = await _taskDetailRepository.AddAsync(taskDetail);
            return createdDetail.Adapt<TaskDetails>();
        }

        public async Task UpdateAsync(int id, UpdateTaskDetailDto taskDetailDto)
        {
            var taskDetail = await _taskDetailRepository.GetByIdAsync(id);
            if (taskDetail == null)
            {
                throw new KeyNotFoundException("Task detail not found");
            }

            taskDetailDto.Adapt(taskDetail);
            await _taskDetailRepository.UpdateAsync(taskDetail);
        }

        public async Task DeleteAsync(int id)
        {
            if (!await _taskDetailRepository.ExistsAsync(id))
            {
                throw new KeyNotFoundException("Task detail not found");
            }

            await _taskDetailRepository.DeleteAsync(id);
        }
    }
}
