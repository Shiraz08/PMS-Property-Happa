﻿
using PMS_PropertyHapa.Staff.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Threading.Tasks;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.Staff.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
        Task<T> RegisterAsync<T>(RegisterationRequestDTO obj);
        Task<T> LogoutAsync<T>(TokenDTO obj);

        Task<T> ChangePasswordAsync<T>(ChangePasswordRequestDto obj);

        Task<APIResponse> ForgotPasswordAsync(ForgetPassword obj);

        Task<T> ResetPasswordAsync<T>(ResetPasswordDto obj);

        Task<bool> UpdateProfileAsync(ProfileModel model);

        Task<ProfileModel> GetProfileAsync(string userId);

        Task<IEnumerable<UserDTO>> GetAllUsersAsync();


        Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync();
        Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId);
        Task<bool> CreateTenantAsync(TenantModelDto tenant);
        Task<bool> UpdateTenantAsync(TenantModelDto tenant);
        Task<bool> DeleteTenantAsync(string tenantId);




        Task<bool> CreateLandlordAsync(OwnerDto owner);


        Task<bool> UpdateLandlordAsync(OwnerDto owner);

        Task<bool> DeleteLandlordAsync(string ownerId);

        Task<TenantModelDto> GetSingleTenantAsync(int tenantId);


        



        Task<List<PropertyTypeDto>> GetAllPropertyTypesAsync();
        Task<List<PropertyTypeDto>> GetPropertyTypeByIdAsync(string tenantId);
        Task<bool> CreatePropertyTypeAsync(PropertyTypeDto propertyType);
        Task<bool> UpdatePropertyTypeAsync(PropertyTypeDto propertyType);
        Task<bool> DeletePropertyTypeAsync(int propertytypeId);

        Task<PropertyTypeDto> GetSinglePropertyTypeAsync(int propertytypeId);

        Task<bool> CreateAssetAsync(AssetDTO asset);

        Task<bool> DeleteAssetAsync(int propertyId);
        Task<bool> UpdateAssetAsync(AssetDTO asset);

        Task<IEnumerable<AssetDTO>> GetAllAssetsAsync();


        #region Communication Interface Services 
        Task<bool> CreateCommunicationAsync(CommunicationDto communication);

        Task<bool> DeleteCommunicationAsync(int communication_id);
        Task<bool> UpdateCommunicationAsync(CommunicationDto communication);

        Task<IEnumerable<CommunicationDto>> GetAllCommunicationAsync();
        #endregion


        Task<List<PropertySubTypeDto>> GetAllPropertySubTypesAsync();
        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAsync(int propertytypeId);
        Task<bool> CreatePropertySubTypeAsync(PropertySubTypeDto propertyType);
        Task<bool> UpdatePropertySubTypeAsync(PropertySubTypeDto propertyType);
        Task<bool> DeletePropertySubTypeAsync(int propertysubtypeId);

        Task<PropertySubTypeDto> GetSinglePropertySubTypeAsync(int propertysubtypeId);

        Task<List<PropertyTypeDto>> GetAllPropertyTypes();

        Task<IEnumerable<OwnerDto>> GetAllLandlordAsync();

        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAllAsync(string tenantId);


        Task<bool>UpdateTenantOrganizationAsync(TenantOrganizationInfoDto tenant);

        Task<TenantOrganizationInfoDto> GetTenantOrganizationByIdAsync(int tenantId);



        Task<OwnerDto> GetSingleLandlordAsync(int ownerId);


        Task<bool> CreateLeaseAsync(LeaseDto lease);

        Task<LeaseDto> GetLeaseByIdAsync(int leaseId);

        Task<IEnumerable<LeaseDto>> GetAllLeasesAsync();

        Task<bool> UpdateLeaseAsync(LeaseDto lease);

        Task<bool> UpdateAccountAsync(TiwiloDto obj);



        Task<IEnumerable<AssetUnitDTO>> GetAllUnitsAsync();




        Task<T> RegisterUserAsync<T>(UserRegisterationDto model);
        Task<bool> VerifyEmailAsync(string email);
        Task<bool> VerifyEmailOtpAsync(string email, string otp);
        Task<bool> VerifyPhoneAsync(string phoneNumber);
        Task<bool> VerifySmsOtpAsync(string userId, string phoneNumber, string otp);


        #region Task && Task History && Maintenance
        Task<IEnumerable<TaskRequestHistoryDto>> GetTaskRequestHistoryAsync(int taskRequsetId);
        Task<IEnumerable<TaskRequestDto>> GetMaintenanceTasksAsync();
        Task<IEnumerable<TaskRequestDto>> GetTaskRequestsAsync();
        Task<TaskRequestDto> GetTaskRequestByIdAsync(int id);
        Task<bool> SaveTaskAsync(TaskRequestDto taskRequestDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> SaveTaskHistoryAsync(TaskRequestHistoryDto taskRequestHistoryDto);


        #endregion

        #region Calendar
        Task<List<CalendarEvent>> GetCalendarEventsAsync(CalendarFilterModel filter);
        Task<List<OccupancyOverviewEvents>> GetOccupancyOverviewEventsAsync(CalendarFilterModel filter);
        Task<LeaseDataDto> GetLeaseDataByIdAsync(int filter);

        #endregion

        #region Vendor Category
        Task<IEnumerable<VendorCategory>> GetVendorCategoriesAsync();
        Task<VendorCategory> GetVendorCategoryByIdAsync(int id);
        Task<bool> SaveVendorCategoryAsync(VendorCategory vendorCategory);
        Task<bool> DeleteVendorCategoryAsync(int id);

        #endregion
        

        #region Vendor
        Task<IEnumerable<VendorDto>> GetVendorsAsync();
        Task<VendorDto> GetVendorByIdAsync(int id);
        Task<bool> SaveVendorAsync(VendorDto vendor);
        Task<bool> DeleteVendorAsync(int id);

        #endregion
        
        #region Vendor
        Task<IEnumerable<ApplicationsDto>> GetApplicationsAsync();
        Task<ApplicationsDto> GetApplicationByIdAsync(int id);
        Task<bool> SaveApplicationAsync(ApplicationsDto applicationsDto);
        Task<bool> DeleteApplicationAsync(int id);

        #endregion

    }
}
