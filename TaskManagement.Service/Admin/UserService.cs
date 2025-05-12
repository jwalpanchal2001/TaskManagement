using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;
using TaskManagement.Service.BaseService;

namespace TaskManagement.Service.Admin;

public class UserService : BaseServices, IUserService
{
    public UserService(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

    public async Task<ApiResponseModel> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("Users", "")
                .PostJsonAsync(createUserDto)
                .ReceiveJson<ApiResponseModel>();

            return response ?? new ApiResponseModel(false, "Empty response from server.");
        }
        catch (FlurlHttpException ex)
        {
            var error = await ex.GetResponseJsonAsync<ApiResponseModel>();
            return error ?? new ApiResponseModel(false, "Unexpected error.");
        }
        catch (Exception ex)
        {
            return new ApiResponseModel(false, ex.Message);
        }
    }
    public async Task<List<UserDto>> GetAllUsersAsync(bool includeDeleted = false)
    {
        try
        {
            var request = await GetFlurlRequestWithAutoRefreshAsync("users", $"?includeDeleted={includeDeleted}");
            var response = await request.GetJsonAsync<List<UserDto>>();

            return response ?? new List<UserDto>();
        }
        catch
        {
            return new List<UserDto>();
        }
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        try
        {
            var response = await GetFlurlRequestWithToken("users", $"{id}")
                .GetJsonAsync<UserDto>();

            return response;
        }
        catch (FlurlHttpException ex)
        {
            var error = await ex.GetResponseJsonAsync<UserDto>();
            return error;
        }
      
    }


    public async Task<bool> UpdateUserAsync(UpdateUserDto model)
    {
        try
        {
            await GetFlurlRequestWithToken("users", $"{model.Id}")
                .PutJsonAsync(model);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            await GetFlurlRequestWithToken("users", $"{id}")
                .DeleteAsync();

            return true;
        }
        catch (FlurlHttpException ex)
        {
            // Handle specific status codes if needed
            if (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("User not found");
            }

            return false;
        }
    }



}