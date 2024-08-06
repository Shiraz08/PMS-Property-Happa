using Newtonsoft.Json;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Stripe;
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

        public async Task<bool> IsPhoneNumberExists(string phoneNumber)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/UserRegisterationAuth/verify-phone/{phoneNumber}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating profile: {ex.Message}", ex);
            }
        }
        public async Task<bool> SavePhoneOTP(OTPDto model)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = model,
                    Url = $"{villaUrl}/api/v1/UserRegisterationAuth/SavePhoneOTP"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }
        public async Task<bool> IsPhoneOTPValid(OTPDto model)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = model,
                    Url = $"{villaUrl}/api/v1/UserRegisterationAuth/verify-phone-otp"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating profile: {ex.Message}", ex);
            }
        }


        public async Task<bool> IsEmailExists(string email)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{villaUrl}/api/v1/UserRegisterationAuth/verify-email/{email}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating profile: {ex.Message}", ex);
            }
        }
        public async Task<bool> SaveEmailOTP(OTPDto model)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = model,
                    Url = $"{villaUrl}/api/v1/UserRegisterationAuth/SaveEmailOTP"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating tenant: {ex.Message}", ex);
            }
        }
        public async Task<bool> IsEmailOTPValid(OTPDto model)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = model,
                    Url = $"{villaUrl}/api/v1/UserRegisterationAuth/verify-email-otp"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating profile: {ex.Message}", ex);
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



        public async Task<bool> CreateAssetAsync(AssetDTO asset)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = asset,
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

        public async Task<List<PropertyTypeDto>> GetAllPropertyTypesDllAsync(Filter filter)
        {
            try
            {
                var apiResponse = await _baseService.SendAsync<APIResponse>(new APIRequest
                {
                    ApiType = SD.ApiType.POST,
                    Data = filter,
                    Url = $"{villaUrl}/api/v1/PropertySubTypeauth/AllPropertyTypeDll"
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


        #region Subscription CRUD

        

        public async Task<List<SubscriptionDto>> GetAllSubscriptionBlocksAsync()
        {
            try
            {
                var apiResponse = await _baseService.SendAsync<APIResponse>(new APIRequest
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/SubscriptionAuth/Subscription"
                });

                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    var subscriptions = JsonConvert.DeserializeObject<List<SubscriptionDto>>(Convert.ToString(apiResponse.Result));
                    return subscriptions;
                }
                else
                {
                    throw new Exception("Failed to retrieve subscriptions data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching subscriptions data: {ex.Message}", ex);
            }
        }


        public async Task<List<SubscriptionDto>> GetSubscriptionsByIdAsync(int Id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/SubscriptionAuth/Subscription/{Id}"
                });

                if (response != null && response.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<List<SubscriptionDto>>(Convert.ToString(response.Result));
                }
                else
                {
                    throw new Exception("Failed to retrieve subscription data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when fetching subscription data: {ex.Message}", ex);
            }
        }


        

        public async Task<bool> CreateSubscriptionAsync(SubscriptionDto subscription)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = subscription,
                    Url = $"{villaUrl}/api/v1/SubscriptionAuth/Subscription"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating subscription: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateSubscriptionAsync(SubscriptionDto subscription)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.PUT,
                    Data = subscription,
                    Url = $"{villaUrl}/api/v1/SubscriptionAuth/Subscription/{subscription.Id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when updating subscription: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteSubscriptionAsync(int Id)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.DELETE,
                    Url = $"{villaUrl}/api/v1/SubscriptionAuth/Subscription/{Id}"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when deleting subscription: {ex.Message}", ex);
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

        #region Stripe Subscription
        public async Task<bool> SavePaymentGuid(PaymentGuidDto paymentGuidDto)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = paymentGuidDto,
                    Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/SavePaymentGuid"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating Payment Guid: {ex.Message}", ex);
            }
        }

        public async Task<bool> SavePaymentInformation(PaymentInformationDto paymentInformationDto)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = paymentInformationDto,
                    Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/SavePaymentInformation"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating Payment Guid: {ex.Message}", ex);
            }
        }
        public async Task<bool> SavePaymentMethodInformation(PaymentMethodInformationDto paymentMethodInformationDto)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = paymentMethodInformationDto,
                    Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/SavePaymentMethodInformation"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating Payment Guid: {ex.Message}", ex);
            }
        }
        public async Task<bool> SaveStripeSubscription(Models.Stripe.StripeSubscriptionDto stripeSubscriptionDto)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.POST,
                    Data = stripeSubscriptionDto,
                    Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/SaveStripeSubscription"
                });

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating Payment Guid: {ex.Message}", ex);
            }
        }

        public async Task<SubscriptionInvoiceDto> GetSubscriptionInvoice(string SubscriptionId)
        {
            try
            {
                var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/GetSubscriptionInvoice/{SubscriptionId}"
                });

                if (response.IsSuccess == true)
                {
                    var invoice = response.Result != null ? JsonConvert.DeserializeObject<SubscriptionInvoiceDto>(Convert.ToString(response.Result)) : new SubscriptionInvoiceDto();
                    return invoice;
                }
                else
                {
                    throw new Exception("Failed to retrieve user data");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred when creating Payment Guid: {ex.Message}", ex);
            }
        }


        //public async Task<bool> SavePaymentInformation(PaymentInformationDto paymentInformationDto)
        //{
        //    try
        //    {
        //        var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
        //        {
        //            ApiType = SD.ApiType.POST,
        //            Data = paymentInformationDto,
        //            Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/SavePaymentInformation"
        //        });

        //        return response.IsSuccess;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"An error occurred when creating Payment Information: {ex.Message}", ex);
        //    }
        //}

        //public async Task<bool> SavePaymentMethodInformation(PaymentMethodInformationDto paymentMethodInformationDto)
        //{
        //    try
        //    {
        //        var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
        //        {
        //            ApiType = SD.ApiType.POST,
        //            Data = paymentMethodInformationDto,
        //            Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/SavePaymentMethodInformation"
        //        });

        //        return response.IsSuccess;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"An error occurred when creating Payment Information: {ex.Message}", ex);
        //    }
        //}

        //public async Task<bool> SaveStripeSubscription(StripeSubscriptionDto stripeSubscriptionDto)
        //{
        //    try
        //    {
        //        var response = await _baseService.SendAsync<APIResponse>(new APIRequest()
        //        {
        //            ApiType = SD.ApiType.POST,
        //            Data = stripeSubscriptionDto,
        //            Url = $"{villaUrl}/api/v1/StripeSubscriptionAuth/SaveStripeSubscription"
        //        });

        //        return response.IsSuccess;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"An error occurred when creating Payment Information: {ex.Message}", ex);
        //    }
        //}

        #endregion
    }
}
