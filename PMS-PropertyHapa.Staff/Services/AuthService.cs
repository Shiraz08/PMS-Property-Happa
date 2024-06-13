using Newtonsoft.Json;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.Enum;
using System.Net.Http;
using PMS_PropertyHapa.Staff.Models;
using APIResponse = PMS_PropertyHapa.Staff.Models.APIResponse;
using PMS_PropertyHapa.Staff.Services.IServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Humanizer.Localisation;

namespace PMS_PropertyHapa.Staff.Services
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
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = model,
                    Url = $"{villaUrl}/api/v1/UsersAuth/Update"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating profile: {ex.Message}", ex);
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
                    var userListJson = Convert.ToString(response.Result);
                    var tenants = JsonConvert.DeserializeObject<IEnumerable<TenantModelDto>>(userListJson);
                    return tenants;
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
                    // Attempt to deserialize the result
                    var tenantDto = JsonConvert.DeserializeObject<TenantModelDto>(Convert.ToString(response.Result));
                    return tenantDto;
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



        public async Task<OwnerDto> GetSingleLandlordAsync(int ownerId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/LandlordAuth/GetSingleLandlord/{ownerId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<OwnerDto>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve owner data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching owner data: {ex.Message}", ex);
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



        public async Task<bool> CreateAssetAsync(AssetDTO asset)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = asset,
                    Url = $"{villaUrl}/api/v1/AssetsAuth/Asset"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }


        public async Task<bool> UpdateAssetAsync(AssetDTO asset)
        {
            try
            {
                int assetId = asset.AssetId;
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = asset,
                    Url = $"{villaUrl}/api/v1/AssetsAuth/Asset/{assetId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }


        public async Task<bool> DeleteAssetAsync(int assetId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/AssetsAuth/Asset/{assetId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting tenant: {ex.Message}", ex);
            }
        }


        public async Task<IEnumerable<AssetDTO>> GetAllAssetsAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/AssetsAuth/Assets"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<AssetDTO>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve communication data");
            }


        }


        #region Communication Services Method
        public async Task<bool> CreateCommunicationAsync(CommunicationDto communication)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = communication,
                    Url = $"{villaUrl}/api/v1/CommunicationAuth/Communication"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating communication: {ex.Message}", ex);
            }
        }


        public async Task<bool> UpdateCommunicationAsync(CommunicationDto communication)
        {
            try
            {
                int communicationId = communication.Communication_Id;
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = communication,
                    Url = $"{villaUrl}/api/v1/CommunicationAuth/Communication/{communicationId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating communication: {ex.Message}", ex);
            }
        }


        public async Task<bool> DeleteCommunicationAsync(int communicationId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/CommunicationAuth/Communication/{communicationId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting communication: {ex.Message}", ex);
            }
        }


        public async Task<IEnumerable<CommunicationDto>> GetAllCommunicationAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/CommunicationAuth/Communication"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var communication = JsonConvert.DeserializeObject<IEnumerable<CommunicationDto>>(userListJson);
                return communication;
            }
            else
            {
                throw new Exception("Failed to retrieve communication data");
            }


        }
        #endregion



        public async Task<IEnumerable<OwnerDto>> GetAllLandlordAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/LandlordAuth/Landlord"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<OwnerDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve asset data");
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

        public async Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAllAsync(string tenantId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/PropertySubTypeAll/{tenantId}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<List<PropertySubTypeDto>>(Convert.ToString(response.Result));
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
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/PropertySubType/{propertyType.PropertySubTypeId}"
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


        #region Landlord


        public async Task<bool> CreateLandlordAsync(OwnerDto owner)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = owner,
                    Url = $"{villaUrl}/api/v1/LandlordAuth/Landlord"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }


        public async Task<bool> UpdateLandlordAsync(OwnerDto owner)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = owner,
                    Url = $"{villaUrl}/api/v1/LandlordAuth/Landlord/{owner.OwnerId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating owner: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteLandlordAsync(string ownerId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/LandlordAuth/Landlord/{ownerId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting owner: {ex.Message}", ex);
            }
        }
        #endregion

        #region Lease
        public async Task<bool> CreateLeaseAsync(LeaseDto lease)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = lease,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/Lease"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating lease: {ex.Message}", ex);
            }
        }

        public async Task<LeaseDto> GetLeaseByIdAsync(int leaseId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/Lease/{leaseId}"
                });

                if (response.IsSuccess && response.Result != null)
                {
                    return JsonConvert.DeserializeObject<LeaseDto>(Convert.ToString(response.Result));
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching lease by ID: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<LeaseDto>> GetAllLeasesAsync()
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/Leases"
                });

                if (response.IsSuccess && response.Result != null)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<LeaseDto>>(Convert.ToString(response.Result));
                }
                return new List<LeaseDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching all leases: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateLeaseAsync(LeaseDto lease)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = lease,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/Lease"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating lease: {ex.Message}", ex);
            }
        }

        //Invoices
        public async Task<List<InvoiceDto>> GetInvoicesAsync(int leaseId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/Invoices/{leaseId}"
                });

                if (response.IsSuccess && response.Result != null)
                {
                    return JsonConvert.DeserializeObject<List<InvoiceDto>>(Convert.ToString(response.Result));
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching lease by ID: {ex.Message}", ex);
            }
        }

        public async Task<bool> AllInvoicePaidAsync(int leaseId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/AllInvoicePaid/{leaseId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when in all paid invoices: {ex.Message}", ex);
            }
        }

        public async Task<bool> AllInvoiceOwnerPaidAsync(int leaseId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/AllInvoiceOwnerPaid/{leaseId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when all owner paid invoices: {ex.Message}", ex);
            }
        }
        
        public async Task<bool> InvoicePaidAsync(int invoiceId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/InvoicePaid/{invoiceId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when in paid invoices: {ex.Message}", ex);
            }
        }

        public async Task<bool> InvoiceOwnerPaidAsync(int invoiceId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/InvoiceOwnerPaid/{invoiceId}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when owner paid invoices: {ex.Message}", ex);
            }
        }

        public async Task<InvoiceDto> GetInvoiceByIdAsync(int invoiceId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/LeaseAuth/Invoice/{invoiceId}"
                });

                if (response.IsSuccess && response.Result != null)
                {
                    return JsonConvert.DeserializeObject<InvoiceDto>(Convert.ToString(response.Result));
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching invoice by ID: {ex.Message}", ex);
            }
        }


        public async Task<bool> UpdateAccountAsync(TiwiloDto obj)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = $"{villaUrl}/api/v1/CommunicationAuth/updateAccount"
            });

            return response.IsSuccess;

        }

        #endregion

        public async Task<IEnumerable<AssetUnitDTO>> GetAllUnitsAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/AssetsAuth/Units"
            });

            if (response.IsSuccess == true)
            {
                var unitListJson = Convert.ToString(response.Result);
                var units = JsonConvert.DeserializeObject<IEnumerable<AssetUnitDTO>>(unitListJson);
                return units;
            }
            else
            {
                throw new Exception("Failed to retrieve unit data");
            }


        }

        public async Task<bool> VerifyEmailAsync(string email)
        {
            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = new { Email = email },
                Url = $"{villaUrl}/api/v1/UserRegistrationAuth/verify-email"
            });

            return response.IsSuccess;
        }

        public async Task<T> RegisterUserAsync<T>(UserRegisterationDto model)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = model,
                Url = $"{villaUrl}/api/v1/UserRegistrationAuth/register"
            });
        }

        public async Task<bool> VerifyEmailOtpAsync(string email, string otp)
        {
            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = new { Email = email, Otp = otp },
                Url = $"{villaUrl}/api/v1/UserRegistrationAuth/verify-email-otp"
            });

            return response.IsSuccess;
        }

        public async Task<bool> VerifyPhoneAsync(string phoneNumber)
        {
            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = new { PhoneNumber = phoneNumber },
                Url = $"{villaUrl}/api/v1/UserRegistrationAuth/verify-phone"
            });

            return response.IsSuccess;
        }

        public async Task<bool> VerifySmsOtpAsync(string userId, string phoneNumber, string otp)
        {
            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = new { UserId = userId, PhoneNumber = phoneNumber, Otp = otp },
                Url = $"{villaUrl}/api/v1/UserRegistrationAuth/verify-sms-otp"
            });
            return response.IsSuccess;
        }

        #region TaskRequest

        public async Task<IEnumerable<TaskRequestHistoryDto>> GetTaskRequestHistoryAsync(int taskRequsetId)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/TaskAuth/GetTaskRequestHistory/{taskRequsetId}"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<TaskRequestHistoryDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve task history data");
            }
        }
        public async Task<IEnumerable<TaskRequestDto>> GetMaintenanceTasksAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/TaskAuth/MaintenanceTasks"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<TaskRequestDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve task data");
            }
        }
        public async Task<IEnumerable<TaskRequestDto>> GetTaskRequestsAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/TaskAuth/Tasks"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<TaskRequestDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve task data");
            }
        }
        public async Task<TaskRequestDto> GetTaskRequestByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/TaskAuth/GetTaskById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<TaskRequestDto>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve task data");
            }
        }
        public async Task<bool> SaveTaskAsync(TaskRequestDto taskRequestDto)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = taskRequestDto,
                    Url = $"{villaUrl}/api/v1/TaskAuth/Task"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating task: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/TaskAuth/Task/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting task: {ex.Message}", ex);
            }
        }
        public async Task<bool> SaveTaskHistoryAsync(TaskRequestHistoryDto taskRequestHistoryDto)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = taskRequestHistoryDto,
                    Url = $"{villaUrl}/api/v1/TaskAuth/TaskHistory"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating task History: {ex.Message}", ex);
            }
        }
        #endregion

        #region Calendar

        public async Task<List<CalendarEvent>> GetCalendarEventsAsync(CalendarFilterModel filter)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = filter,
                Url = $"{villaUrl}/api/v1/CalendarAuth/CalendarEvents"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var events = JsonConvert.DeserializeObject<List<CalendarEvent>>(userListJson);
                return events;
            }
            else
            {
                throw new Exception("Failed to retrieve calendar data");
            }
        }
        public async Task<List<OccupancyOverviewEvents>> GetOccupancyOverviewEventsAsync(CalendarFilterModel filter)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = filter,
                Url = $"{villaUrl}/api/v1/CalendarAuth/OccupancyOverviewEvents"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var events = JsonConvert.DeserializeObject<List<OccupancyOverviewEvents>>(userListJson);
                return events;
            }
            else
            {
                throw new Exception("Failed to retrieve calendar data");
            }
        }
        public async Task<LeaseDataDto> GetLeaseDataByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Url = $"{villaUrl}/api/v1/CalendarAuth/LeaseData/{id}"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var lease = JsonConvert.DeserializeObject<LeaseDataDto>(userListJson);
                return lease;
            }
            else
            {
                throw new Exception("Failed to retrieve lease data");
            }
        }
        #endregion

        #region Vendor Category

        public async Task<IEnumerable<VendorCategory>> GetVendorCategoriesAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/VendorAuth/VendorCategories"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<VendorCategory>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve vendor categories data");
            }
        }
        public async Task<VendorCategory> GetVendorCategoryByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/VendorAuth/GetVendorCategoryById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<VendorCategory>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve vendor category data");
            }
        }
        public async Task<bool> SaveVendorCategoryAsync(VendorCategory vendorCategory)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = vendorCategory,
                    Url = $"{villaUrl}/api/v1/VendorAuth/VendorCategory"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating vendor category: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteVendorCategoryAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/VendorAuth/VendorCategory/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting vendor category: {ex.Message}", ex);
            }
        }

        #endregion

        #region Vendor 

        public async Task<IEnumerable<VendorDto>> GetVendorsAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/VendorAuth/Vendors"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<VendorDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve vendor data");
            }
        }
        public async Task<VendorDto> GetVendorByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/VendorAuth/GetVendorById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<VendorDto>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve vendor data");
            }
        }
        public async Task<bool> SaveVendorAsync(VendorDto vendor)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = vendor,
                    Url = $"{villaUrl}/api/v1/VendorAuth/Vendor"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating vendor: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteVendorAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/VendorAuth/Vendor/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting vendor: {ex.Message}", ex);
            }
        }

        #endregion

        
        #region Applications 

        public async Task<IEnumerable<ApplicationsDto>> GetApplicationsAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/ApplicationsAuth/Applications"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<ApplicationsDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve application data");
            }
        }
        public async Task<ApplicationsDto> GetApplicationByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/ApplicationsAuth/GetApplicationById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<ApplicationsDto>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve application data");
            }
        }
        public async Task<bool> SaveApplicationAsync(ApplicationsDto application)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = application,
                    Url = $"{villaUrl}/api/v1/ApplicationsAuth/Application"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating application: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteApplicationAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/ApplicationsAuth/Application/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting application: {ex.Message}", ex);
            }
        }
        public async Task<string> GetTermsbyId(string id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/ApplicationsAuth/GetTerms/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return response.Result?.ToString();
            }
            else
            {
                throw new Exception("Failed to retrieve terms");
            }
        }

        #endregion

        #region AccountType

        public async Task<IEnumerable<AccountType>> GetAccountTypesAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/AccountTypeAuth/AccountTypes"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<AccountType>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve account types data");
            }
        }
        public async Task<AccountType> GetAccountTypeByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/AccountTypeAuth/GetAccountTypeById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<AccountType>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve account types data");
            }
        }
        public async Task<bool> SaveAccountTypeAsync(AccountType accountType)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = accountType,
                    Url = $"{villaUrl}/api/v1/AccountTypeAuth/AccountType"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating account type: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteAccountTypeAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/AccountTypeAuth/AccountType/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting account type: {ex.Message}", ex);
            }
        }

        #endregion

        #region AccountSubType

        public async Task<IEnumerable<AccountSubTypeDto>> GetAccountSubTypesAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/AccountSubTypeAuth/AccountSubTypes"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<AccountSubTypeDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve account sub types data");
            }
        }
        public async Task<AccountSubType> GetAccountSubTypeByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/AccountSubTypeAuth/GetAccountSubTypeById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<AccountSubType>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve account sub types data");
            }
        }
        public async Task<bool> SaveAccountSubTypeAsync(AccountSubType accountSubType)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = accountSubType,
                    Url = $"{villaUrl}/api/v1/AccountSubTypeAuth/AccountSubType"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating account sub type: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteAccountSubTypeAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/AccountSubTypeAuth/AccountSubType/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting account sub type: {ex.Message}", ex);
            }
        }

        #endregion

        #region ChartAccount

        public async Task<IEnumerable<ChartAccountDto>> GetChartAccountsAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/ChartAccountsAuth/ChartAccounts"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<ChartAccountDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve account sub types data");
            }
        }
        public async Task<ChartAccount> GetChartAccountByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/ChartAccountsAuth/GetChartAccountById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<ChartAccount>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve chart account data");
            }
        }
        public async Task<bool> SaveChartAccountAsync(ChartAccount chartAccount)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = chartAccount,
                    Url = $"{villaUrl}/api/v1/ChartAccountsAuth/ChartAccount"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating chart account: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteChartAccountAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/ChartAccountsAuth/ChartAccount/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting chart account: {ex.Message}", ex);
            }
        }

        #endregion

        #region Budget

        public async Task<IEnumerable<BudgetDto>> GetBudgetsAsync()
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/BudgetAuth/Budgets"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<BudgetDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve budget data");
            }
        }
        public async Task<Budget> GetBudgetByIdAsync(int id)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{villaUrl}/api/v1/BudgetAuth/GetBudgetById/{id}"
            });
            if (response.IsSuccess && response.Result != null)
            {
                return JsonConvert.DeserializeObject<Budget>(Convert.ToString(response.Result));
            }
            else
            {
                throw new Exception("Failed to retrieve chart account data");
            }
        }
        public async Task<bool> SaveBudgetAsync(BudgetDto budget)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = budget,
                    Url = $"{villaUrl}/api/v1/BudgetAuth/Budget"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating budget: {ex.Message}", ex);
            }
        }
        public async Task<bool> SaveDuplicateBudgetAsync(BudgetDto budget)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = budget,
                    Url = $"{villaUrl}/api/v1/BudgetAuth/DuplicateBudget"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating budget: {ex.Message}", ex);
            }
        }
        public async Task<bool> DeleteBudgetAsync(int id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/BudgetAuth/Budget/{id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting budget: {ex.Message}", ex);
            }
        }
        #endregion

        #region Report

        

        public async Task<IEnumerable<LeaseReportDto>> GetLeaseReports(ReportFilter reportFilter)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Data = reportFilter,
                Url = $"{villaUrl}/api/v1/ReportsAuth/LeaseReport"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<LeaseReportDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve Lease data");
            }
        }

        public async Task<IEnumerable<InvoiceReportDto>> GetInvoiceReports(ReportFilter reportFilter)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Data = reportFilter,
                Url = $"{villaUrl}/api/v1/ReportsAuth/InvoiceReport"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<InvoiceReportDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve Invoice data");
            }
        }

        public async Task<IEnumerable<TaskRequestReportDto>> GetTaskRequestReports(ReportFilter reportFilter)
        {

            var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,
                Data = reportFilter,
                Url = $"{villaUrl}/api/v1/ReportsAuth/TaskReport"
            });

            if (response.IsSuccess == true)
            {
                var userListJson = Convert.ToString(response.Result);
                var asset = JsonConvert.DeserializeObject<IEnumerable<TaskRequestReportDto>>(userListJson);
                return asset;
            }
            else
            {
                throw new Exception("Failed to retrieve Task Request data");
            }
        }
        #endregion
    }
}
