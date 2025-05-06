using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Entity.Model;

public class TaskDetail
{
    public int Id { get; set; }

    [Required]
    public string Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public int TaskId { get; set; }

    public Tasks Tasks { get; set; }
}
