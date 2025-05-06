using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto;

public class CreateUserDto
{
    public int? Id { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [StringLength(100)]
    [Required]
    public string FullName { get; set; }

    public bool IsAdmin { get; set; } = false;
}