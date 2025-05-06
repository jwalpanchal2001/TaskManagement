using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TaskManagement.Entity.Model;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;
using TaskManagement.Model.ViewModel;
using TaskManagement.Service.BaseService;

namespace TaskManagement.Service.Login;
public class LoginService(IHttpContextAccessor httpContextAccessor)
        : BaseServices(httpContextAccessor), ILoginService
{
    public async Task<ApiResponseModel> ValidateLogin(LoginRequestModel loginRequestModel)
    {
        try
        {
            var response = await GetFlurlRequestWithoutToken("login", "login")
                .PostJsonAsync(loginRequestModel)
                .ReceiveJson<ApiResponseModel>();

            return response ?? new ApiResponseModel(false, "Empty response from server.");
        }
        catch (FlurlHttpException ex)
        {
            var errorResponse = await ex.GetResponseJsonAsync<ApiResponseModel>();
            return errorResponse ?? new ApiResponseModel(false, "Unexpected error during login.");
        }
        catch (Exception ex)
        {
            return new ApiResponseModel(false, $"Exception: {ex.Message}");
        }
    }
}