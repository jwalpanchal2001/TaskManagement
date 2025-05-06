using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Entity.Model;

public class UserTask
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int TaskId { get; set; }
    public Tasks Task { get; set; }

    public int TaskStatusId { get; set; }
    public TaskState TaskStatus { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public TaskDetail TaskDetail { get; set; }  // One-to-one
}
