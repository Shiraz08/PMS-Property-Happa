
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

        Task<bool> ValidateCurrentPassword(long userId, string currentPassword);

        Task<bool> ChangePassword(long userId, string currentPassword, string newPassword);

        Task<ApplicationUser> FindByEmailAsync(string email);

        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

        Task SendResetPasswordEmailAsync(ApplicationUser user, string callbackUrl);


        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto user);

        Task<ApplicationUser> FindByUserId(string userId);

        Task<string> EncryptEmail(string email);
        Task<string> DecryptEmail(string encryptedEmail);


        Task<ProfileModel> GetProfileModelAsync(string userId);
    }
}