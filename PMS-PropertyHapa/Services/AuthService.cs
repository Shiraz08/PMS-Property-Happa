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

        public async Task<T> RegisterAsync<T>(RegisterationRequestDTO obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = $"{villaUrl}/api/v1/UsersAuth/register"
            }, withBearer: false);
        }

        public async Task<T> LoginAsync<T>(LoginRequestDTO obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = villaUrl + $"/api/v1/UsersAuth/login"
            }, withBearer: false);
        }


        public async Task<bool> UpdateProfileAsync(ProfileModel model)
        {
            using (var multipartContent = new MultipartFormDataContent())
            {
                // Adding string properties to the multipart/form-data
                multipartContent.Add(new StringContent(model.UserId), nameof(model.UserId));
                multipartContent.Add(new StringContent(model.Name ?? string.Empty), nameof(model.Name));
                multipartContent.Add(new StringContent(model.UserName ?? string.Empty), nameof(model.UserName));
                multipartContent.Add(new StringContent(model.Email ?? string.Empty), nameof(model.Email));
                multipartContent.Add(new StringContent(model.PhoneNumber ?? string.Empty), nameof(model.PhoneNumber));
                multipartContent.Add(new StringContent(model.Address ?? string.Empty), nameof(model.Address));
                multipartContent.Add(new StringContent(model.Address2 ?? string.Empty), nameof(model.Address2));
                multipartContent.Add(new StringContent(model.Locality ?? string.Empty), nameof(model.Locality));
                multipartContent.Add(new StringContent(model.District ?? string.Empty), nameof(model.District));
                multipartContent.Add(new StringContent(model.Region ?? string.Empty), nameof(model.Region));
                multipartContent.Add(new StringContent(model.PostalCode ?? string.Empty), nameof(model.PostalCode));
                multipartContent.Add(new StringContent(model.Country ?? string.Empty), nameof(model.Country));
                multipartContent.Add(new StringContent(model.Status.ToString()), nameof(model.Status));
           
                if (!string.IsNullOrEmpty(model.ExistingPictureUrl))
                {
                    multipartContent.Add(new StringContent(model.ExistingPictureUrl), nameof(model.ExistingPictureUrl));
                }

 
                if (model.NewPicture != null)
                {
                    multipartContent.Add(new StreamContent(model.NewPicture.OpenReadStream()), nameof(model.NewPicture), model.NewPicture.FileName);
                }

         
                var apiResponse = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = multipartContent,
                    Url = $"{villaUrl}/api/v1/UsersAuth/Update"
                });

           
                return apiResponse.IsSuccess;
            }
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


        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/UsersAuth/Users"
                });

                if (response != null && response.IsSuccess)
                {
                    var userListJson = Convert.ToString(response.Result);
                    var usersDto = JsonConvert.DeserializeObject<IEnumerable<UserDTO>>(userListJson);
                    return usersDto;
                }
                else
                {
                    throw new Exception("Failed to retrieve user data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching user data: {ex.Message}", ex);
            }
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





        #region Tenant Crud
        public async Task<TenantModelDto> GetTenantByIdAsync(int tenantId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/Tenants/{tenantId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<TenantModelDto>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve tenant data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching tenant data: {ex.Message}", ex);
            }
        }

        public async Task<bool> CreateTenantAsync(TenantModelDto tenant)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = tenant,
                    Url = $"{villaUrl}/api/v1/Tenants"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateTenantAsync(TenantModelDto tenant)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = tenant,
                    Url = $"{villaUrl}/api/v1/Tenants/{tenant.TenantId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating tenant: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteTenantAsync(int tenantId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/Tenants/{tenantId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting tenant: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
