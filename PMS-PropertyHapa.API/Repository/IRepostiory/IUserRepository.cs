
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.Stripe;
using static PMS_PropertyHapa.Models.DTO.TenantModelDto;

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

        Task<UserRolesDto> GetUserRolesAsync(string userId);
        Task<bool> IsUserTrialAsync(string userId);
        Task<SubscriptionInvoiceData> GetSubscriptionInvoiceAsync(string subscriptionId);
        #region Tenant
        Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync();
        Task<IEnumerable<TenantModelDto>> GetAllTenantsDllAsync(Filter filter);
        Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId);
        Task<bool> CreateTenantAsync(TenantModelDto tenantDto);
        Task<bool> UpdateTenantAsync(TenantModelDto tenantDto);
        Task<APIResponse> DeleteTenantAsync(int tenantId);

        Task<TenantModelDto> GetSingleTenantByIdAsync(int tenantId);

        Task<List<TenantModelDto>> GetTenantsReportAsync(ReportFilter reportFilter);
        Task<List<InvoiceReportDto>> GetInvoicesReportAsync(ReportFilter reportFilter);
        Task<List<TenantDependentDto>> GetTenantDependentsAsync(ReportFilter reportFilter);
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
        Task<APIResponse> DeleteLeaseAsync(int leaseId);
        Task<List<LeaseDto>> GetAllLeasesAsync();
        Task<bool> UpdateLeaseAsync(LeaseDto leaseDto);
        Task<List<InvoiceDto>> GetInvoicesByAssetAsync(int assetId);
        Task<List<LeaseDto>> GetTenantHistoryByAssetAsync(int assetId);
        Task<List<InvoiceDto>> GetInvoicesByTenantAsync(int tenantId);
        Task<List<InvoiceDto>> GetInvoicesByLandLordAsync(int landlordId);
        Task<List<LeaseDto>> GetTenantHistoryByTenantAsync(int tenantId);


        //Invoices 
        Task<List<InvoiceDto>> GetInvoicesAsync(int leaseId);
        Task<List<InvoiceDto>> GetAllInvoicesAsync();
        Task<bool> AllInvoicePaidAsync(int leaseId);
        Task<bool> AllInvoiceOwnerPaidAsync(int leaseId);
        Task<bool> InvoicePaidAsync(int invoiceId);
        Task<bool> InvoiceOwnerPaidAsync(int invoiceId);
        Task<InvoiceDto> GetInvoiceByIdAsync(int invoiceId);


        #region PropertySubType

        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAllAsync(string tenantId);
        Task<List<PropertyTypeDto>> GetAllPropertyTypesDll(Filter filter);
        Task<List<PropertySubTypeDto>> GetAllPropertySubTypesAsync();
        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAsync(int propertytypeId);
        Task<bool> CreatePropertySubTypeAsync(PropertySubTypeDto tenantDto);
        Task<bool> UpdatePropertySubTypeAsync(PropertySubTypeDto tenantDto);
        Task<bool> DeletePropertySubTypeAsync(int propertysubtypeId);

        Task<PropertySubTypeDto> GetSinglePropertySubTypeByIdAsync(int propertysubtypeId);
        #endregion


        Task<TenantOrganizationInfoDto> GetTenantOrgByIdAsync(int tenantId);

        Task<bool> UpdateTenantOrgAsync(TenantOrganizationInfoDto tenantDto);


        //Landlord
        Task<List<OwnerDto>> GetAllLandlordAsync();
        Task<List<OwnerDto>> GetAllLandlordDllAsync(Filter filter);
        Task<bool> UpdateOwnerAsync(OwnerDto tenantDto);
        Task<bool> CreateOwnerAsync(OwnerDto tenantDto);
        Task<APIResponse> DeleteOwnerAsync(int ownerId);
        Task<OwnerDto> GetSingleLandlordByIdAsync(int ownerId);
        Task<List<OwnerDto>> GetLandlordOrganizationAsync(ReportFilter reportFilter);
        Task<List<AssetDTO>> GetLandlordAssetAsync(ReportFilter reportFilter);

        //Asset
        Task<bool> CreateAssetAsync(AssetDTO assetDTO);
        Task<bool> UpdateAssetAsync(AssetDTO assetDTO);
        Task<APIResponse> DeleteAssetAsync(int assetId);
        Task<List<AssetDTO>> GetAllAssetsAsync();
        Task<AssetDTO> GetAssetByIdAsync(int assetId);
        Task<List<AssetDTO>> GetAssetsDllAsync(Filter filter);
        Task<List<UnitDTO>> GetUnitsDetailAsync(int assetId);



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
        Task<List<AssetUnitDTO>> GetUnitsDllAsync(Filter filter);
        Task<List<AssetUnitDTO>> GetUnitsByUserAsync(Filter filter);




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
        Task<List<TaskRequestDto>> GetAllTaskRequestsAsync();
        Task<List<LineItemDto>> GetAllLineItemsAsync();
        Task<List<TaskRequestDto>> GetExpenseByAssetAsync(int assetId);
        Task<List<TaskRequestDto>> GetTasksByTenantAsync(int tenantId);
        Task<List<TaskRequestDto>> GetTasksByLandLordAsync(int landlordId);
        Task<List<TaskRequestDto>> GetTaskRequestsAsync();
        Task<TaskRequestDto> GetTaskByIdAsync(int id);
        Task<bool> SaveTaskAsync(TaskRequestDto taskRequestDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> SaveTaskHistoryAsync(TaskRequestHistoryDto taskRequestHistoryDto);

        //Calendar Events

        Task<List<CalendarEvent>> GetCalendarEventsAsync(CalendarFilterModel filter);
        Task<List<OccupancyOverviewEvents>> GetOccupancyOverviewEventsAsync(CalendarFilterModel filter);
        Task<LeaseDataDto> GetLeaseDataByIdAsync(int filter);


        Task<LandlordDataDto> GetLandlordDataById(int id);
        Task<TenantDataDto> GetTenantDataById(int id);
        //Vendor Category
        Task<List<VendorCategory>> GetVendorCategoriesAsync();
        Task<List<VendorCategory>> GetVendorCategoriesDllAsync(Filter filter);
        Task<VendorCategory> GetVendorCategoryByIdAsync(int id);
        Task<bool> SaveVendorCategoryAsync(VendorCategory vendorCategory);
        Task<bool> DeleteVendorCategoryAsync(int id);

        //Vendor Classification
        Task<List<VendorClassification>> GetVendorClassificationsAsync();
        Task<List<VendorClassification>> GetVendorClassificationsDllAsync(Filter filter);
        Task<VendorClassification> GetVendorClassificationByIdAsync(int id);
        Task<bool> SaveVendorClassificationAsync(VendorClassification vendorClassification);
        Task<bool> DeleteVendorClassificationAsync(int id);
        //Vendor
        Task<List<VendorDto>> GetVendorsAsync();
        Task<List<VendorDto>> GetVendorsDllAsync(Filter filter);
        Task<VendorDto> GetVendorByIdAsync(int id);
        Task<bool> SaveVendorAsync(VendorDto vendor);
        Task<APIResponse> DeleteVendorAsync(int id);


        //Applications
        Task<List<ApplicationsDto>> GetApplicationsAsync();
        Task<ApplicationsDto> GetApplicationByIdAsync(int id);
        Task<bool> SaveApplicationAsync(ApplicationsDto vendor);
        Task<bool> DeleteApplicationAsync(int id);
        Task<string> GetTermsbyId(string id);



        //Account Type
        Task<List<AccountType>> GetAccountTypesAsync();
        Task<List<AccountType>> GetAccountTypesDllAsync(Filter filter);
        Task<AccountType> GetAccountTypeByIdAsync(int id);
        Task<bool> SaveAccountTypeAsync(AccountType accountType);
        Task<APIResponse> DeleteAccountTypeAsync(int id);



        //Account Sub Type
        Task<List<AccountSubTypeDto>> GetAccountSubTypesAsync();
        Task<List<AccountSubTypeDto>> GetAccountSubTypesDllAsync(Filter filter);
        Task<AccountSubType> GetAccountSubTypeByIdAsync(int id);
        Task<bool> SaveAccountSubTypeAsync(AccountSubType accountSubType);
        Task<APIResponse> DeleteAccountSubTypeAsync(int id);


        //Account ChartAccount
        Task<List<ChartAccountDto>> GetChartAccountsAsync();
        Task<List<ChartAccountDto>> GetChartAccountsDllAsync(Filter filter);
        Task<ChartAccount> GetChartAccountByIdAsync(int id);
        Task<bool> SaveChartAccountAsync(ChartAccount chartAccount);
        Task<APIResponse> DeleteChartAccountAsync(int id);

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
        Task<List<FinanceReportDto>> GetFinanceReportsAsync(ReportFilter reportFilter);
        Task<List<UnitDTO>> GetUnitsByAssetAsync(ReportFilter reportFilter);

        //Document
        Task<List<DocumentsDto>> GetDocumentsAsync();
        Task<DocumentsDto> GetDocumentByIdAsync(int id);
        Task<bool> SaveDocumentAsync(DocumentsDto document);
        Task<bool> DeleteDocumentAsync(int id);
        Task<List<DocumentsDto>> GetDocumentByAssetAsync(int assetId);
        Task<List<DocumentsDto>> GetDocumentByTenantAsync(int tenantId);
        Task<List<DocumentsDto>> GetDocumentByLandLordAsync(int landlordId);

        //SupportCenter
        Task<List<FAQ>> GetFAQsAsync();
        Task<List<VideoTutorial>> GetVideoTutorialsAsync();

        //LateFee

        Task<LateFeeDto> GetLateFeeAsync(Filter filter);
        Task<LateFeeAssetDto> GetLateFeeByAssetAsync(int assetId);
        Task<bool> SaveLateFeeAsync(LateFeeDto lateFee);
        Task<bool> SaveLateFeeAssetAsync(LateFeeAssetDto lateFeeAsset);

        //  Stripe Subscription
        Task<bool> SavePaymentGuidAsync(PaymentGuidDto paymentGuidDto);
        Task<bool> SavePaymentInformationAsync(PaymentInformationDto paymentInformationDto);
        Task<bool> SavePaymentMethodInformationAsync(PaymentMethodInformationDto paymentMethodInformationDto);
        Task<bool> SaveStripeSubscriptionAsync(StripeSubscriptionDto stripeSubscriptionDto);
        Task<bool> SaveSaveSubscriptionInvoiceAsync(SubscriptionInvoiceDto subscriptionInvoiceDto);
        Task<StripeSubscriptionDto> CheckTrialDaysAsync(string currenUserId);
    }
}