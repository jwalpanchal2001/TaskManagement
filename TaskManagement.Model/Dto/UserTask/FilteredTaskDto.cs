using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto.UserTask;

public class FilteredTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsDeleted { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public int? TaskStatusId { get; set; }
    public string? TaskStatus { get; set; }
}
