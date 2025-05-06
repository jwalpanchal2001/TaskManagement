using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Entity.Model;


public class TaskState
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; }

    public bool IsDeleted { get; set; } = false;

    public ICollection<Tasks> Tasks { get; set; }
}
