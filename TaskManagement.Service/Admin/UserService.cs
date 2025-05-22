using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using TaskManagement.Model.Api;
using TaskManagement.Model.Dto;
using TaskManagement.Service.BaseService;

namespace TaskManagement.Service.Admin
{
    public class UserService : BaseServices, IUserService
    {
        public UserService(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        public async Task<ApiResponseModel> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                return await SendAuthorizedRequestAsync("users", "",
                    req => req.PostJsonAsync(createUserDto).ReceiveJson<ApiResponseModel>());
            }
            catch (UnauthorizedAccessException)
            {
                throw;
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
                return await SendAuthorizedRequestAsync("users", $"?includeDeleted={includeDeleted}",
                    req => req.GetJsonAsync<List<UserDto>>());
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            try
            {
                return await SendAuthorizedRequestAsync("users", $"{id}",
                    req => req.GetJsonAsync<UserDto>());
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto model)
        {
            try
            {
                await SendAuthorizedRequestAsync<object>("users", $"{model.Id}",
                    req => req.PutJsonAsync(model).ContinueWith(_ => (object)null));

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
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
                await SendAuthorizedRequestAsync<object>("users", $"{id}",
                    req => req.DeleteAsync().ContinueWith(_ => (object)null));

                return true;
            }
            catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("User not found");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

    }
}
