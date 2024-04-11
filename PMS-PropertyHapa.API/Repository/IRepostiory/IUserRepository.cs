
using Microsoft.AspNetCore.Identity;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
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

    }
}