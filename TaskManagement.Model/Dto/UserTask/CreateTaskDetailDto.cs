using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto.UserTask;

public class CreateTaskDetailDto
{
    [Required]
    public string Description { get; set; }

    public int TaskId { get; set; }
}
