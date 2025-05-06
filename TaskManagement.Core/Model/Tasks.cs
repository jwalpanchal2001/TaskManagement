using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Entity.Model;

public class Tasks
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Foreign Key to User
    public int? UserId { get; set; }

    public User? User { get; set; }
    // Foreign Key to Creator User
    public int CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User CreatedBy { get; set; }


    // Foreign Key to TaskStatus
    public int TaskStatusId { get; set; }

    public TaskState TaskStatus { get; set; }

    public ICollection<TaskDetail> TaskDetails { get; set; }
}


