
using Microsoft.AspNetCore.Identity;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;

namespace MagicVilla_VillaAPI.Repository.IRepostiory
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);
        Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO);
        Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO);

        Task RevokeRefreshToken(TokenDTO tokenDTO);

        Task<UserDTO> RegisterTenant(RegisterationRequestDTO registrationRequestDTO);

        Task<UserDTO> RegisterAdmin(RegisterationRequestDTO registrationRequestDTO);

        Task<UserDTO> RegisterUser(RegisterationRequestDTO registrationRequestDTO);

        Task<bool> ValidateCurrentPassword(string userId, string currentPassword);

        Task<bool> ChangePassword(string userId, string currentPassword, string newPassword);

        Task<ApplicationUser> FindByEmailAsync(string email);

        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

        Task SendResetPasswordEmailAsync(ApplicationUser user, string callbackUrl);


        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto user);

        Task<ApplicationUser> FindByUserId(string userId);

        Task<string> EncryptEmail(string email);
        Task<string> DecryptEmail(string encryptedEmail);


        Task<ProfileModel> GetProfileModelAsync(string userId);

        Task<IEnumerable<UserDTO>> GetAllUsersAsync();



        #region Tenant
        Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync();
        Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId);
        Task<bool> CreateTenantAsync(TenantModelDto tenantDto);
        Task<bool> UpdateTenantAsync(TenantModelDto tenantDto);
        Task<bool> DeleteTenantAsync(string tenantId);

        Task<TenantModelDto> GetSingleTenantByIdAsync(int tenantId);
        #endregion



        #region PropertyType
        Task<List<PropertyTypeDto>> GetAllPropertyTypesAsync();
        Task<List<PropertyTypeDto>> GetPropertyTypeByIdAsync(string tenantId);
        Task<bool> CreatePropertyTypeAsync(PropertyTypeDto tenantDto);
        Task<bool> UpdatePropertyTypeAsync(PropertyTypeDto tenantDto);
        Task<bool> DeletePropertyTypeAsync(int tenantId);

        Task<PropertyTypeDto> GetSinglePropertyTypeByIdAsync(int tenantId);
        #endregion



        Task<bool> CreateLeaseAsync(LeaseDto leaseDto);

        Task<LeaseDto> GetLeaseByIdAsync(int leaseId);

        Task<List<LeaseDto>> GetAllLeasesAsync();

        Task<bool> UpdateLeaseAsync(LeaseDto leaseDto);


        //Invoices 
        Task<List<InvoiceDto>> GetInvoicesAsync(int leaseId);
        Task<bool> AllInvoicePaidAsync(int leaseId);
        Task<bool> AllInvoiceOwnerPaidAsync(int leaseId);
        Task<bool> InvoicePaidAsync(int invoiceId);
        Task<bool> InvoiceOwnerPaidAsync(int invoiceId);
        Task<InvoiceDto> GetInvoiceByIdAsync(int invoiceId);


        #region PropertySubType

        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAllAsync(string tenantId);
        Task<List<PropertyTypeDto>> GetAllPropertyTypes();
        Task<List<PropertySubTypeDto>> GetAllPropertySubTypesAsync();
        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAsync(int propertytypeId);
        Task<bool> CreatePropertySubTypeAsync(PropertySubTypeDto tenantDto);
        Task<bool> UpdatePropertySubTypeAsync(PropertySubTypeDto tenantDto);
        Task<bool> DeletePropertySubTypeAsync(int propertysubtypeId);

        Task<PropertySubTypeDto> GetSinglePropertySubTypeByIdAsync(int propertysubtypeId);
        #endregion


        Task<TenantOrganizationInfoDto> GetTenantOrgByIdAsync(int tenantId);

        Task<bool> UpdateTenantOrgAsync(TenantOrganizationInfoDto tenantDto);



        Task<List<AssetDTO>> GetAllAssetsAsync();


        Task<List<OwnerDto>> GetAllLandlordAsync();



        Task<bool> UpdateOwnerAsync(OwnerDto tenantDto);

        Task<bool> CreateOwnerAsync(OwnerDto tenantDto);

        Task<bool> DeleteOwnerAsync(string ownerId);

        Task<OwnerDto> GetSingleLandlordByIdAsync(int ownerId);




        Task<bool> CreateAssetAsync(AssetDTO assetDTO);

        Task<bool> UpdateAssetAsync(AssetDTO assetDTO);

        Task<bool> DeleteAssetAsync(int assetId);



        #region CommunicationData
        Task<bool> DeleteCommunicationAsync(int communicationId);

        Task<bool> UpdateCommunicationAsync(CommunicationDto communicationDto);

        Task<bool> CreateCommunicationAsync(CommunicationDto communication);

        Task<List<CommunicationDto>> GetAllCommunicationAsync();

        Task<bool> UpdateAccountAsync(TiwiloDto userDto);
        #endregion




        Task<List<SubscriptionDto>> GetAllSubscriptionsAsync();
        Task<SubscriptionDto> GetSubscriptionByIdAsync(int Id);
        Task<bool> CreateSubscriptionAsync(SubscriptionDto subscriptionDto);
        Task<bool> UpdateSubscriptionAsync(SubscriptionDto subscriptionDto);
        Task<bool> DeleteSubscriptionAsync(int Id);



        Task<List<AssetUnitDTO>> GetAllUnitsAsync();




        Task<bool> RegisterUserData(UserRegisterationDto registrationRequestDTO);


        Task<bool> SavePhoneOTP(OTPDto oTPDto);
        Task<bool> SaveEamilOTP(OTPDto oTPDto);
        Task<bool> VerifyEmailOtpAsync(OTPDto oTPDto);

        Task<bool> VerifyPhoneOtpAsync(OTPDto oTPDto);
        Task<bool> FindByPhoneNumberAsync(string phoneNumber);

        //Task<bool> FindByEmailAddressAsync(string email);

        //Tasks Requests
        Task<List<TaskRequestHistoryDto>> GetTaskRequestHistoryAsync(int id);
        Task<List<TaskRequestDto>> GetMaintenanceTasksAsync();
        Task<List<TaskRequestDto>> GetTaskRequestsAsync();
        Task<TaskRequestDto> GetTaskByIdAsync(int id);
        Task<bool> SaveTaskAsync(TaskRequestDto taskRequestDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> SaveTaskHistoryAsync(TaskRequestHistoryDto taskRequestHistoryDto);

        //Calendar Events

        Task<List<CalendarEvent>> GetCalendarEventsAsync(CalendarFilterModel filter);
        Task<List<OccupancyOverviewEvents>> GetOccupancyOverviewEventsAsync(CalendarFilterModel filter);
        Task<LeaseDataDto> GetLeaseDataByIdAsync(int filter);


        Task<object> GetLandlordDataById(int id);
        Task<object> GetTenantDataById(int id);
        //Vendor Category
        Task<List<VendorCategory>> GetVendorCategoriesAsync();
        Task<VendorCategory> GetVendorCategoryByIdAsync(int id);
        Task<bool> SaveVendorCategoryAsync(VendorCategory vendorCategory);
        Task<bool> DeleteVendorCategoryAsync(int id);

        //Vendor Classification
        Task<List<VendorClassification>> GetVendorClassificationsAsync();
        Task<VendorClassification> GetVendorClassificationByIdAsync(int id);
        Task<bool> SaveVendorClassificationAsync(VendorClassification vendorClassification);
        Task<bool> DeleteVendorClassificationAsync(int id);
        //Vendor
        Task<List<VendorDto>> GetVendorsAsync();
        Task<VendorDto> GetVendorByIdAsync(int id);
        Task<bool> SaveVendorAsync(VendorDto vendor);
        Task<bool> DeleteVendorAsync(int id);


        //Applications
        Task<List<ApplicationsDto>> GetApplicationsAsync();
        Task<ApplicationsDto> GetApplicationByIdAsync(int id);
        Task<bool> SaveApplicationAsync(ApplicationsDto vendor);
        Task<bool> DeleteApplicationAsync(int id);
        Task<string> GetTermsbyId(string id);



        //Account Type
        Task<List<AccountType>> GetAccountTypesAsync();
        Task<AccountType> GetAccountTypeByIdAsync(int id);
        Task<bool> SaveAccountTypeAsync(AccountType accountType);
        Task<bool> DeleteAccountTypeAsync(int id);



        //Account Sub Type
        Task<List<AccountSubTypeDto>> GetAccountSubTypesAsync();
        Task<AccountSubType> GetAccountSubTypeByIdAsync(int id);
        Task<bool> SaveAccountSubTypeAsync(AccountSubType accountSubType);
        Task<bool> DeleteAccountSubTypeAsync(int id);


        //Account ChartAccount
        Task<List<ChartAccountDto>> GetChartAccountsAsync();
        Task<ChartAccount> GetChartAccountByIdAsync(int id);
        Task<bool> SaveChartAccountAsync(ChartAccount chartAccount);
        Task<bool> DeleteChartAccountAsync(int id);

        //Budget
        Task<List<BudgetDto>> GetBudgetsAsync();
        Task<Budget> GetBudgetByIdAsync(int id);
        Task<bool> SaveBudgetAsync(BudgetDto budgetDto);
        Task<bool> SaveDuplicateBudgetAsync(BudgetDto budgetDto);
        Task<bool> DeleteBudgetAsync(int id);

        //Reports
        Task<List<LeaseReportDto>> GetLeaseReportAsync(ReportFilter reportFilter);
        Task<List<InvoiceReportDto>> GetInvoiceReportAsync(ReportFilter reportFilter);
        Task<List<TaskRequestReportDto>> GetTaskRequestReportAsync(ReportFilter reportFilter);
    }
}