using AutoMapper;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PMS_PropertyHapa.API.Areas.Identity.Data;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Shared.Email;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public UserRepository(ApiDbContext db, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.ApplicationUsers
                .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);


            if (user == null || isValid == false)
            {
                return new TokenDTO()
                {
                    AccessToken = ""
                };
            }
            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GetAccessToken(user,jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);
            TokenDTO tokenDto = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return tokenDto;
        }

        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email=registerationRequestDTO.UserName,
                NormalizedEmail=registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult()){
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                    var userToReturn = _db.ApplicationUsers
                        .FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);

                }
            }
            catch(Exception e)
            {

            }

            return new UserDTO();
        }

        private async Task<string> GetAccessToken(ApplicationUser user, string jwtTokenId)
        {
            //if user was found generate JWT Token
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Aud, "dotnetmastery.com")
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Issuer="https://magicvilla-api.com",
                Audience="https://test-magic-api.com",
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenStr = tokenHandler.WriteToken(token);
            return tokenStr;
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            // Find an existing refresh token
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null) {
                return new TokenDTO();
            }

            // Compare data from existing refresh and access token provided and if there is any missmatch then consider it as a fraud
            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // When someone tries to use not valid refresh token, fraud possible
            if (!existingRefreshToken.IsValid)
            {
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId,existingRefreshToken.JwtTokenId);
            }
            // If just expired then mark as invalid and return empty
            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // replace old refresh with a new one with updated expire date
            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);


            // revoke existing refresh token
            await MarkTokenAsInvalid(existingRefreshToken);

            // generate new access token
            var applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
            if (applicationUser == null)
                return new TokenDTO();

            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };

        }

        public async Task RevokeRefreshToken(TokenDTO tokenDTO)
        {
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(_ => _.Refresh_Token == tokenDTO.RefreshToken);

            if (existingRefreshToken == null)
                return;

            // Compare data from existing refresh and access token provided and
            // if there is any missmatch then we should do nothing with refresh token

            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {

                return;
            }

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId,existingRefreshToken.JwtTokenId);

        }

        private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
        {
            RefreshToken refreshToken = new()
            {
                IsValid = true,
                UserId = userId,
                JwtTokenId = tokenId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(2),
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
            };

            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return userId==expectedUserId && jwtTokenId== expectedTokenId;

            }
            catch
            {
                return false;
            }
        }


        private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
        {
            var refreshToken = await _db.RefreshTokens
            .Where(u => u.UserId == userId && u.JwtTokenId == tokenId)
            .FirstOrDefaultAsync();

            if (refreshToken != null)
            {
                refreshToken.IsValid = false;
                await _db.SaveChangesAsync();
            }
        }


        private Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
           return _db.SaveChangesAsync();
        }




        #region Registeration Section
        public async Task<UserDTO> RegisterTenant(RegisterationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationRequestDTO.UserName,
                PasswordHash = registrationRequestDTO.Password,
                Name = registrationRequestDTO.Name
            };

            var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Tenant"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Tenant"));
                }

                await _userManager.AddToRoleAsync(user, "Tenant");

                return _mapper.Map<UserDTO>(user);
            }

            return new UserDTO();
        }


        public async Task<UserDTO> RegisterAdmin(RegisterationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationRequestDTO.UserName,
                PasswordHash = registrationRequestDTO.Password,
                Name = registrationRequestDTO.Name
            };

            var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await _userManager.AddToRoleAsync(user, "Admin");

                return _mapper.Map<UserDTO>(user);
            }

            return new UserDTO();
        }



        public async Task<UserDTO> RegisterUser(RegisterationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationRequestDTO.UserName,
                PasswordHash = registrationRequestDTO.Password,
                Name = registrationRequestDTO.Name
            };

            var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                await _userManager.AddToRoleAsync(user, "User");

                return _mapper.Map<UserDTO>(user);
            }
            return new UserDTO();
        }




        public async Task<bool> ValidateCurrentPassword(long userId, string currentPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                return await _userManager.CheckPasswordAsync(user, currentPassword);
            }
            return false;
        }

        public async Task<bool> ChangePassword(long userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }
            if (await _userManager.CheckPasswordAsync(user, currentPassword))
            {
                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                return result.Succeeded;
            }

            return false;
        }




        public async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _userManager.Users
                                 .FirstOrDefaultAsync(u => u.Email == email);
        }



        public async Task<ApplicationUser> FindByUserId(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            return await _userManager.Users
                                 .FirstOrDefaultAsync(u => u.Id == userId);
        }



        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }


        public async Task SendResetPasswordEmailAsync(ApplicationUser user, string callbackUrl)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(callbackUrl))
                throw new ArgumentException("Callback URL is required.", nameof(callbackUrl));

            string emailContent = $"To reset your password, follow this link: <a href='{callbackUrl}'>link</a>";
            string Subject = "Reset Password Request";
            await _emailSender.SendEmailAsync(user.Email, Subject, emailContent);
        }



        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await FindByEmailAsync(model.Email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            return result;
        }


        #endregion




    }
}
