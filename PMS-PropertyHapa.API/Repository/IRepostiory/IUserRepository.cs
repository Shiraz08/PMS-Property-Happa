
using Microsoft.AspNetCore.Identity;
using PMS_PropertyHapa.API.Areas.Identity.Data;
using PMS_PropertyHapa.Models.DTO;

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


        Task<TenantOrganizationInfoDto> GetTenantOrgByIdAsync(int tenantId);

        Task<bool> UpdateTenantOrgAsync(TenantOrganizationInfoDto tenantDto);
    }
}