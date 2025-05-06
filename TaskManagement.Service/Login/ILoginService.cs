using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;
using TaskManagement.Model.ViewModel;


namespace TaskManagement.Service.Login;

public interface ILoginService
{
    Task<ApiResponseModel> ValidateLogin(LoginRequestModel loginRequestModel);
}
