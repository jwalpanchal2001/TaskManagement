using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto;

public class RegisterRequestModel
{
    [Required, MaxLength(100)]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; }

    public bool IsAdmin { get; set; }
}
