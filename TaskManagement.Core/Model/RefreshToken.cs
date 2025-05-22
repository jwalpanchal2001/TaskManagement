using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Entity.Helper;

namespace TaskManagement.Entity.Model;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    public string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = TimeZoneHelper.GetIndianTime();

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedByIp { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Foreign Key to User
    public int UserId { get; set; }

    public User User { get; set; }

    public bool IsActive => RevokedAt == null && !IsExpired;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
