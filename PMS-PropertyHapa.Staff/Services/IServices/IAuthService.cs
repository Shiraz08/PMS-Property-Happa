
using PMS_PropertyHapa.Staff.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Threading.Tasks;
using PMS_PropertyHapa.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using static PMS_PropertyHapa.Models.DTO.TenantModelDto;
using PMS_PropertyHapa.Models.Stripe;
using Microsoft.AspNetCore.Identity;

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
        Task<UserRolesDto> GetUserRolesAsync(string userId);
        Task<bool> IsUserTrialAsync(string userId);

        Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync();
        Task<IEnumerable<TenantModelDto>> GetAllTenantsDllAsync(Filter filter);
        Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId);
        Task<bool> CreateTenantAsync(TenantModelDto tenant);
        Task<bool> UpdateTenantAsync(TenantModelDto tenant);
        Task<APIResponse> DeleteTenantAsync(int tenantId);
        Task<IEnumerable<TenantModelDto>> GetTenantsReport(ReportFilter reportFilter);
        Task<IEnumerable<InvoiceReportDto>> GetInvoicesReport(ReportFilter reportFilter);
        Task<IEnumerable<TenantDependentDto>> GetTenantDependents(ReportFilter reportFilter);




        Task<bool> CreateLandlordAsync(OwnerDto owner);
        Task<bool> UpdateLandlordAsync(OwnerDto owner);
        Task<APIResponse> DeleteLandlordAsync(int ownerId);
        Task<TenantModelDto> GetSingleTenantAsync(int tenantId);
        Task<IEnumerable<OwnerDto>> GetLandlordOrganization(ReportFilter reportFilter);
        Task<IEnumerable<AssetDTO>> GetLandlordAsset(ReportFilter reportFilter);





        Task<List<PropertyTypeDto>> GetAllPropertyTypesAsync();
        Task<List<PropertyTypeDto>> GetPropertyTypeByIdAsync(string tenantId);
        Task<bool> CreatePropertyTypeAsync(PropertyTypeDto propertyType);
        Task<bool> UpdatePropertyTypeAsync(PropertyTypeDto propertyType);
        Task<bool> DeletePropertyTypeAsync(int propertytypeId);

        Task<PropertyTypeDto> GetSinglePropertyTypeAsync(int propertytypeId);

        Task<bool> CreateAssetAsync(AssetDTO asset);

        Task<APIResponse> DeleteAssetAsync(int propertyId);
        Task<bool> UpdateAssetAsync(AssetDTO asset);

        Task<IEnumerable<AssetDTO>> GetAllAssetsAsync();
        Task<AssetDTO> GetAssetByIdAsync(int assetId);
        Task<IEnumerable<AssetDTO>> GetAssetsDllAsync(Filter filter);
        Task<IEnumerable<UnitDTO>> GetUnitsDetailAsync(int assetId);


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
        Task<IEnumerable<OwnerDto>> GetAllLandlordDllAsync(Filter filter);

        Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAllAsync(string tenantId);


        Task<bool> UpdateTenantOrganizationAsync(TenantOrganizationInfoDto tenant);

        Task<TenantOrganizationInfoDto> GetTenantOrganizationByIdAsync(int tenantId);



        Task<OwnerDto> GetSingleLandlordAsync(int ownerId);


        Task<bool> CreateLeaseAsync(LeaseDto lease);

        Task<LeaseDto> GetLeaseByIdAsync(int leaseId);

        Task<APIResponse> DeleteLeaseAsync(int leaseId);

        Task<IEnumerable<LeaseDto>> GetAllLeasesAsync();

        Task<bool> UpdateLeaseAsync(LeaseDto lease);

        Task<bool> UpdateAccountAsync(TiwiloDto obj);


        // Invoices 
        Task<List<InvoiceDto>> GetInvoicesAsync(int leaseId);
        Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
        Task<bool> AllInvoicePaidAsync(int leaseId);
        Task<bool> AllInvoiceOwnerPaidAsync(int leaseId);
        Task<bool> InvoicePaidAsync(int invoiceId);
        Task<bool> InvoiceOwnerPaidAsync(int invoiceId);
        Task<InvoiceDto> GetInvoiceByIdAsync(int invoiceId);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByAsset(int assetId);
        Task<IEnumerable<LeaseDto>> GetTenantHistoryByAsset(int assetId);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByTenant(int tenantId);
        Task<IEnumerable<InvoiceDto>> GetInvoicesByLandLord(int landlordId);
        Task<IEnumerable<LeaseDto>> GetTenantHistoryByTenant(int tenantId);



        Task<IEnumerable<AssetUnitDTO>> GetAllUnitsAsync();
        Task<IEnumerable<AssetUnitDTO>> GetUnitsDllAsync(Filter filter);
        Task<IEnumerable<AssetUnitDTO>> GetUnitsByUserAsync(Filter filter);




        Task<T> RegisterUserAsync<T>(UserRegisterationDto model);
        Task<bool> VerifyEmailAsync(string email);
        Task<bool> VerifyEmailOtpAsync(string email, string otp);
        Task<bool> VerifyPhoneAsync(string phoneNumber);
        Task<bool> VerifySmsOtpAsync(string userId, string phoneNumber, string otp);


        #region Task && Task History && Maintenance
        Task<IEnumerable<TaskRequestHistoryDto>> GetTaskRequestHistoryAsync(int taskRequsetId);
        Task<IEnumerable<TaskRequestDto>> GetMaintenanceTasksAsync();
        Task<IEnumerable<TaskRequestDto>> GetAllTaskRequestsAsync();
        Task<IEnumerable<LineItemDto>> GetAllLineItemsAsync();
        Task<IEnumerable<TaskRequestDto>> GetExpenseByAssetAsync(int assetId);
        Task<IEnumerable<TaskRequestDto>> GetTasksByTenantAsync(int tenantId);
        Task<IEnumerable<TaskRequestDto>> GetTasksByLandLordAsync(int landlordId);
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
        Task<IEnumerable<VendorCategory>> GetVendorCategoriesDllAsync(Filter filter);
        Task<VendorCategory> GetVendorCategoryByIdAsync(int id);
        Task<bool> SaveVendorCategoryAsync(VendorCategory vendorCategory);
        Task<bool> DeleteVendorCategoryAsync(int id);

        #endregion

        #region Vendor Classification
        Task<IEnumerable<VendorClassification>> GetVendorClassificationsAsync();
        Task<IEnumerable<VendorClassification>> GetVendorClassificationsDllAsync(Filter filter);
        Task<VendorClassification> GetVendorClassificationByIdAsync(int id);
        Task<bool> SaveVendorClassificationAsync(VendorClassification vendorClassification);
        Task<bool> DeleteVendorClassificationAsync(int id);

        #endregion


        #region Vendor
        Task<IEnumerable<VendorDto>> GetVendorsAsync();
        Task<IEnumerable<VendorDto>> GetVendorsDllAsync(Filter filter);
        Task<VendorDto> GetVendorByIdAsync(int id);
        Task<bool> SaveVendorAsync(VendorDto vendor);
        Task<APIResponse> DeleteVendorAsync(int id);

        #endregion

        #region Applications
        Task<IEnumerable<ApplicationsDto>> GetApplicationsAsync();
        Task<ApplicationsDto> GetApplicationByIdAsync(int id);
        Task<bool> SaveApplicationAsync(ApplicationsDto applicationsDto);
        Task<bool> DeleteApplicationAsync(int id);
        Task<string> GetTermsbyId(string id);
        #endregion

        #region AccountType

        Task<IEnumerable<AccountType>> GetAccountTypesAsync();
        Task<IEnumerable<AccountType>> GetAccountTypesDllAsync(Filter filter);
        Task<AccountType> GetAccountTypeByIdAsync(int id);
        Task<bool> SaveAccountTypeAsync(AccountType accountType);
        Task<APIResponse> DeleteAccountTypeAsync(int id);

        #endregion

        #region AccountSubType

        Task<IEnumerable<AccountSubTypeDto>> GetAccountSubTypesAsync();
        Task<IEnumerable<AccountSubTypeDto>> GetAccountSubTypesDllAsync(Filter filter);
        Task<AccountSubType> GetAccountSubTypeByIdAsync(int id);
        Task<bool> SaveAccountSubTypeAsync(AccountSubType accountSubType);
        Task<APIResponse> DeleteAccountSubTypeAsync(int id);

        #endregion

        #region ChartAccount

        Task<IEnumerable<ChartAccountDto>> GetChartAccountsAsync();
        Task<IEnumerable<ChartAccountDto>> GetChartAccountsDllAsync(Filter filter);
        Task<ChartAccount> GetChartAccountByIdAsync(int id);
        Task<bool> SaveChartAccountAsync(ChartAccount chartAccount);
        Task<APIResponse> DeleteChartAccountAsync(int id);

        #endregion

        #region Budget

        Task<IEnumerable<BudgetDto>> GetBudgetsAsync();
        Task<Budget> GetBudgetByIdAsync(int id);
        Task<bool> SaveBudgetAsync(BudgetDto budget);
        Task<bool> SaveDuplicateBudgetAsync(BudgetDto budget);
        Task<bool> DeleteBudgetAsync(int id);
        #endregion

        #region Reports

        Task<IEnumerable<LeaseReportDto>> GetLeaseReports(ReportFilter reportFilter);
        Task<IEnumerable<InvoiceReportDto>> GetInvoiceReports(ReportFilter reportFilter);
        Task<IEnumerable<TaskRequestReportDto>> GetTaskRequestReports(ReportFilter reportFilter);
        Task<IEnumerable<FinanceReportDto>> GetFinanceReports(ReportFilter reportFilter);
        Task<IEnumerable<UnitDTO>> GetUnitsByAsset(ReportFilter reportFilter);
        #endregion

        #region Documents

        Task<IEnumerable<DocumentsDto>> GetDocumentsAsync();
        Task<DocumentsDto> GetDocumentByIdAsync(int id);
        Task<bool> SaveDocumentAsync(DocumentsDto document);
        Task<bool> DeleteDocumentAsync(int id);
        Task<IEnumerable<DocumentsDto>> GetDocumentByAssetAsync(int assetId);
        Task<IEnumerable<DocumentsDto>> GetDocumentByTenantAsync(int tenantId);
        Task<IEnumerable<DocumentsDto>> GetDocumentByLandLordAsync(int landlordId);

        #endregion

        #region SupportCenter

        Task<IEnumerable<FAQ>> GetFAQsAsync();
        Task<IEnumerable<VideoTutorial>> GetVideoTutorialsAsync();

        #endregion

        #region GetDataById
        Task<LandlordDataDto> GetLandlordDataById(int id);
        Task<TenantDataDto> GetTenantDataById(int id);
        #endregion

        #region LateFee

        Task<LateFeeDto> GetLateFee(Filter filter);
        Task<LateFeeAssetDto> GetLateFeeByAsset(int assetId);
        Task<bool> SaveLateFeeAsync(LateFeeDto lateFee);
        Task<bool> SaveLateFeeAssetAsync(LateFeeAssetDto lateFeeAsset);

        #endregion

        #region Subscription

        Task<List<SubscriptionDto>> GetAllSubscriptionsAsync();
        Task<StripeSubscriptionDto> CheckTrialDaysAsync(string currenUserId);
        Task<bool> SavePaymentGuid(PaymentGuidDto paymentGuidDto);

        Task<bool> SavePaymentInformation(PaymentInformationDto paymentInformationDto);
        Task<bool> SavePaymentMethodInformation(PaymentMethodInformationDto paymentMethodInformationDto);
        Task<bool> SaveStripeSubscription(StripeSubscriptionDto stripeSubscriptionDto);
        #endregion
    }
}
