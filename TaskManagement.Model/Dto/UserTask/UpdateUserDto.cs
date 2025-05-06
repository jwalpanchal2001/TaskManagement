using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto;

public class UpdateUserDto
{
    public int Id { get; set; }
    [Required]
    [EmailAddress]
    public string Username { get; set; }
    [StringLength(100)]
    public string FullName { get; set; }

    //public bool IsAdmin { get; set; }
}
