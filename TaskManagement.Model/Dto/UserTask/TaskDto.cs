using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Dto.UserTask;

namespace TaskManagement.Model.Dto;


public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; }
    public int CreatedById { get; set; }  // Added
    public string CreatedByName { get; set; }  // Added
    public int TaskStatusId { get; set; }
    public string TaskStatus { get; set; }
    public IEnumerable<TaskDetails>? TaskDetails { get; set; } // Added for task details

}