using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto;

public class RefreshRequestModel
{
    [Required]
    public string RefreshToken { get; init; }
    [Required]
    public string AccessToken { get; set; }
}
