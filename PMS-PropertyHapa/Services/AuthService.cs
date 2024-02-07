using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Services.IServices;
using PMS_PropertyHapa.Shared.Enum;
using System.Net.Http;

namespace PMS_PropertyHapa.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _clientFactory;
        private string villaUrl;
        private readonly IBaseService _baseService;
        public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration, IBaseService baseService) 
        {
            _baseService = baseService;
            _clientFactory = clientFactory;
            villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");

        }

        public async Task<T> LoginAsync<T>(LoginRequestDTO obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = villaUrl + $"/api/v1/UsersAuth/login"
            },withBearer:false);
        }



        public async Task<bool> UpdateProfileAsync(ProfileModel model)
        {
            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = model,
                Url = $"{villaUrl}/api/v1/UsersAuth/Update"
            }, withBearer: true);

            return response != null && response.IsSuccess;
        }


        public async Task<ProfileModel> GetProfileAsync(string userId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/UsersAuth/{userId}"
                });

                if (response != null && response.IsSuccess)
                {
         
                    return JsonConvert.DeserializeObject<ProfileModel>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve profile data");
                }
            }
            catch (Exception ex)
            {
             
                throw new Exception($"An error occurred when fetching profile data: {ex.Message}", ex);
            }
        }



        public async Task<T> RegisterAsync<T>(RegisterationRequestDTO obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = $"{villaUrl}/api/v1/UsersAuth/register"
            }, withBearer: false);
        }

        public async Task<T> LogoutAsync<T>(TokenDTO obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = villaUrl + $"/api/{SD.CurrentAPIVersion}/UsersAuth/revoke"
            });
        }


        #region Change Password, Forgot Password, Reset Password 
        public async Task<T> ChangePasswordAsync<T>(ChangePasswordRequestDto obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = $"{villaUrl}/api/v1/UsersAuth/change-password"
            });
        }

        public async Task<APIResponse> ForgotPasswordAsync(ForgetPassword obj)
        {
            var apiResponse = new APIResponse();
            try
            {
                apiResponse = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = obj,
                    Url = $"{villaUrl}/api/v1/UsersAuth/forgot-password"
                });

                return apiResponse;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"An error occurred when calling ForgotPassword API: {e.Message}");

                return new APIResponse
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Failed to communicate with the authentication service. Please try again later." }
                };
            }
        }


        public async Task<T> ResetPasswordAsync<T>(ResetPasswordDto obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = $"{villaUrl}/api/v1/UsersAuth/reset-password"
            });
        }
        #endregion
    }
}
