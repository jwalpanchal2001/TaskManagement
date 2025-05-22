using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto.UserTask;

public class TaskFilterModel
{
    public bool IncludeDeleted { get; set; } = false;
    public int? CreatedById { get; set; }
    public int? AssignedToId { get; set; }
    public int? StatusId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    // sorting 
    public string? SortBy { get; set; } = "DueDate";
    public string? SortOrder { get; set; } = "desc";


}