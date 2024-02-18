using Newtonsoft.Json;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
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


        public async Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync()
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/Tenantauth/Tenant"
                });

                if (response != null && response.IsSuccess)
                {

                    return JsonConvert.DeserializeObject<IEnumerable<TenantModelDto>>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve tenants data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching tenants data: {ex.Message}", ex);
            }
        }



        public async Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/Tenantauth/Tenant/{tenantId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<List<TenantModelDto>>(Convert.ToString(response.Result));
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


        public async Task<TenantModelDto> GetSingleTenantAsync(int tenantId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/Tenantauth/GetSingleTenant/{tenantId}"
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
                    Url = $"{villaUrl}/api/v1/Tenantauth/Tenant"
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
                    Url = $"{villaUrl}/api/v1/Tenantauth/Tenant/{tenant.TenantId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating tenant: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteTenantAsync(string tenantId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/Tenantauth/Tenant/{tenantId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting tenant: {ex.Message}", ex);
            }
        }

        #endregion



        #region TenantOrganization 
        public async Task<TenantOrganizationInfoDto> GetTenantOrganizationByIdAsync(int tenantId)
        {
            try
            {
                var apiRequest = new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/Tenantauth/TenantOrg/{tenantId}"
                };

                var response = await _baseService.SendAsync<APIResponse>(apiRequest);

                if (response != null && response.IsSuccess)
                {
                    var tenantDto = JsonConvert.DeserializeObject<TenantOrganizationInfoDto>(response.Result.ToString());
                    return tenantDto;
                }
                else
                {
                    var errorMessage = response?.ErrorMessages?.FirstOrDefault() ?? "Failed to retrieve tenant data";
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching tenant data: {ex.Message}", ex);
            }
        }



        public async Task<bool> UpdateTenantOrganizationAsync(TenantOrganizationInfoDto tenant)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = tenant,
                    Url = $"{villaUrl}/api/v1/Tenantauth/TenantOrg/{tenant.Id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating tenant: {ex.Message}", ex);
            }
        }

        #endregion


        #region PropertyType Crud


        public async Task<List<PropertyTypeDto>> GetAllPropertyTypesAsync()
        {
            try
            {
                var apiResponse = await _baseService.SendAsync<APIResponse>(new APIRequest
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/AllPropertyType"
                });

                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    var propertyTypes = JsonConvert.DeserializeObject<List<PropertyTypeDto>>(Convert.ToString(apiResponse.Result));
                    return propertyTypes;
                }
                else
                {
                    throw new Exception("Failed to retrieve property types data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching property types data: {ex.Message}", ex);
            }
        }


        public async Task<List<PropertyTypeDto>> GetPropertyTypeByIdAsync(string tenantId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertyTypeauth/PropertyType/{tenantId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<List<PropertyTypeDto>>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve property type data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching property type data: {ex.Message}", ex);
            }
        }


        public async Task<PropertyTypeDto> GetSinglePropertyTypeAsync(int propertytypeId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertyTypeauth/GetSinglePropertyType/{propertytypeId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<PropertyTypeDto>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve property type data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching tenant data: {ex.Message}", ex);
            }
        }


        public async Task<bool> CreatePropertyTypeAsync(PropertyTypeDto propertyType)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = propertyType,
                    Url = $"{villaUrl}/api/v1/PropertyTypeauth/PropertyType"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePropertyTypeAsync(PropertyTypeDto propertyType)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = propertyType,
                    Url = $"{villaUrl}/api/v1/PropertyTypeauth/PropertyType/{propertyType.PropertyTypeId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating property type: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeletePropertyTypeAsync(int propertytypeId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/PropertyTypeauth/PropertyType/{propertytypeId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting tenant: {ex.Message}", ex);
            }
        }

        #endregion




        #region Property SubType Crud


        public async Task<List<PropertyTypeDto>> GetAllPropertyTypes()
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/AllPropertyType"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<List<PropertyTypeDto>>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve property types data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching property types data: {ex.Message}", ex);
            }
        }


        public async Task<List<PropertySubTypeDto>> GetAllPropertySubTypesAsync()
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertyTypeauth/PropertySubType"
                });

                if (response != null && response.IsSuccess)
                {

                    return JsonConvert.DeserializeObject<List<PropertySubTypeDto>>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve tenants data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching tenants data: {ex.Message}", ex);
            }
        }



        public async Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAsync(int propertyTypeId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/PropertySubType/{propertyTypeId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<List<PropertySubTypeDto>>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve property sub type data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching property sub type data: {ex.Message}", ex);
            }
        }


        public async Task<PropertySubTypeDto> GetSinglePropertySubTypeAsync(int propertysubtypeId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/GetSinglePropertySubType/{propertysubtypeId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<PropertySubTypeDto>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve property type data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching tenant data: {ex.Message}", ex);
            }
        }


        public async Task<bool> CreatePropertySubTypeAsync(PropertySubTypeDto propertyType)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = propertyType,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/PropertySubType"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePropertySubTypeAsync(PropertySubTypeDto propertyType)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = propertyType,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/PropertySubType/{propertyType.PropertySubTypeName}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating property sub type: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeletePropertySubTypeAsync(int propertysubtypeId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/PropertySubType/{propertysubtypeId}"
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
