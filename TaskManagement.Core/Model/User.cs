using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Entity.Model;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsDeleted { get; set; } = false;

    public ICollection<Tasks> AssignedTasks { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }

}
