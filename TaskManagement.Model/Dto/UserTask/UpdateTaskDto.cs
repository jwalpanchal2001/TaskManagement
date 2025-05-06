using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto;

public class UpdateTaskDto
{
    public int Id { get; set; }

    [StringLength(200)]
    public string Title { get; set; }

    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public int? UserId { get; set; }
    public int TaskStatusId { get; set; }
}