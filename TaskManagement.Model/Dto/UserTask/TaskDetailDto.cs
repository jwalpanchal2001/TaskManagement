using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto.UserTask;

public class TaskDetails
{
    public int Id { get; set; }

    [Required]
    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }
    public int TaskId { get; set; }
}
