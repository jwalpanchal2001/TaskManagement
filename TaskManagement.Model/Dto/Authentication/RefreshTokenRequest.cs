﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Dto;

public class RefreshTokenRequest
{
    public int UserId { get; set; }
    //public string? RefreshToken { get; set; }
}