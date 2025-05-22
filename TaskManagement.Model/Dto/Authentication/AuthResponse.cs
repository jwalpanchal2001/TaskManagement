using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto;

public class AuthResponse
{

    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public bool? isAdmin { get; set; }
    public DateTime ExpiresIn { get; set; }
    public int UserId { get; set; }
}
