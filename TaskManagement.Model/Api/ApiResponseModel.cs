using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Model.Api;
public class ApiResponseModel
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Result { get; set; }

    public ApiResponseModel() { }

    public ApiResponseModel(object? result)
    {
        IsSuccess = true;
        Result = result;
    }

    public ApiResponseModel(string message)
    {
        IsSuccess = false;
        Message = message;
    }

    public ApiResponseModel(bool isSuccess, string message, object? result = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Result = result;
    }
}
