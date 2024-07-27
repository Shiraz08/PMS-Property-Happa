using AutoMapper;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.ContentModel;
using PMS_PropertyHapa.API.Services;
using PMS_PropertyHapa.API.ViewModels;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static PMS_PropertyHapa.Models.DTO.TenantModelDto;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private string secretKey;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly GoogleCloudStorageService _googleCloudStorageService;
        private readonly GoogleCloudStorageOptions _googleCloudStorageOptions;


        public UserRepository(ApiDbContext db, IConfiguration configuration,
                              UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager,
                               GoogleCloudStorageService googleCloudStorageService, IOptions<GoogleCloudStorageOptions> googleCloudStorageOptions)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _googleCloudStorageService = googleCloudStorageService;
            _googleCloudStorageOptions = googleCloudStorageOptions.Value;
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
            var user = await _db.ApplicationUsers
                            .FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());

            if (user == null)
            {
                return new TokenDTO() { AccessToken = "" };
            }

            // Attempt to sign in the user with the provided password
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, loginRequestDTO.Password, loginRequestDTO.Remember, false);

            // Check if the sign-in was not successful
            if (!result.Succeeded)
            {
                return new TokenDTO() { AccessToken = "" };
            }

            // Ensure the user ID can be parsed as a GUID
            bool isValidGuid = Guid.TryParse(user.Id, out Guid userIdGuid);
            if (!isValidGuid)
            {
                return new TokenDTO() { AccessToken = "" };
            }

            // Fetch tenant organization details
            var tenantOrganization = await _db.TenantOrganizationInfo
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(to => to.TenantUserId == userIdGuid);

            // Generate JWT token and refresh token
            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GetAccessToken(user, jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);

            //we need to handle subscription which is not added in database but the amount is charged from stripe Dashboard
            var hasActiveSubscription = CheckUserSubscription(user.Id);
            if (hasActiveSubscription)
            {
                var hasSubscribedRole = await _userManager.IsInRoleAsync(user, "SubscribedUser");
                if (!hasSubscribedRole)
                {
                    await _userManager.AddToRoleAsync(user, "SubscribedUser");
                }
            }
            else
            {
                var hasSubscribedRole = await _userManager.IsInRoleAsync(user, "SubscribedUser");
                if (hasSubscribedRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, "SubscribedUser");
                }
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Return TokenDTO including tenant organization details, if available
            return new TokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserName = user.UserName,
                UserId = user.Id,
                OrganizationName = tenantOrganization?.OrganizationName,
                OrganizationDescription = tenantOrganization?.OrganizationDescription,
                PrimaryColor = tenantOrganization?.OrganizatioPrimaryColor,
                SecondaryColor = tenantOrganization?.OrganizationSecondColor,
                OrganizationLogo = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"TenantOrganizationInfo_OrganizationLogo_" + tenantOrganization?.Id + Path.GetExtension(tenantOrganization?.OrganizationLogo)}",
                OrganizationIcon = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"TenantOrganizationInfo_OrganizationIcon_" + tenantOrganization?.Id + Path.GetExtension(tenantOrganization?.OrganizationIcon)}",
                Tid = tenantOrganization?.Id,
                TenantId = user.TenantId,
                Roles = roles.ToList()
            };
        }

        private bool CheckUserSubscription(string userId)
        {
            //var subscription = _db.StripeSubscriptions.FirstOrDefault(x => (x.UserId == userId) && !x.IsCanceled);
            var subscription = _db.StripeSubscriptions.Where(x => x.UserId == userId && !x.IsCanceled).OrderByDescending(x => x.Id).FirstOrDefault();
            if (subscription != null && subscription.EndDate >= DateTime.UtcNow && subscription.IsCanceled != true)
            {
                if (subscription.IsTrial == true && subscription.HasAdminPermission != true)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email = registerationRequestDTO.UserName,
                NormalizedEmail = registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name,
                TenantId = registerationRequestDTO?.TenantId ?? 0,
                EmailConfirmed = registerationRequestDTO?.EmailConfirmed ?? false,
                SubscriptionName = registerationRequestDTO?.SubscriptionName
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                    var userToReturn = _db.ApplicationUsers
                        .FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);

                }
            }
            catch (Exception e)
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
                    new Claim(JwtRegisteredClaimNames.Aud, "propertyhapa.com")
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Issuer = "https://localhost:7178",
                Audience = "https://localhost:7178",
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
            if (existingRefreshToken == null)
            {
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
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
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

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

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
                return userId == expectedUserId && jwtTokenId == expectedTokenId;

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

        public async Task<bool> UpdateAccountAsync(TiwiloDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.UserID);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            user.AccountSid = userDto.AccountSid;
            user.AuthToken = userDto.AuthToken;
            user.TiwiloPhone = userDto.TiwiloPhone;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update user account: {errors}");
            }

            return true;
        }

        public async Task<UserDTO> RegisterAdmin(RegisterationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationRequestDTO.UserName,
                PasswordHash = registrationRequestDTO.Password,
                Name = registrationRequestDTO.Name,
                Email = registrationRequestDTO.Email,
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

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            try
            {
                var userDTOs = await _userManager.Users.Where(x => x.IsDeleted != true)
                    .Select(u => new UserDTO
                    {
                        userName = u.UserName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        createdOn = u.AddedDate,
                        AppTId = u.Id,
                        AccountSid = u.AccountSid,
                        AuthToken = u.AuthToken,
                        TiwiloPhone = u.TiwiloPhone,
                    })
                    .ToListAsync();

                return userDTOs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<UserDTO> RegisterUser(RegisterationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registrationRequestDTO.UserName,
                PasswordHash = registrationRequestDTO.Password,
                Name = registrationRequestDTO.Name,
                Email = registrationRequestDTO.Email,
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

        public async Task<bool> RegisterUserData(UserRegisterationDto registrationRequestDTO)
        {
            ApplicationUser user = new ApplicationUser
            {
                FirstName = registrationRequestDTO.FirstName,
                LastName = registrationRequestDTO.LastName,
                UserName = registrationRequestDTO.EmailAddress,
                Email = registrationRequestDTO.EmailAddress,
                PhoneNumber = registrationRequestDTO.PhoneNumber,
                Country = registrationRequestDTO.Country,
            };


            var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("PropertyManager"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("PropertyManager"));
                }
                await _userManager.AddToRoleAsync(user, "PropertyManager");

                AdditionalUserData additionalData = new AdditionalUserData
                {
                    AppTenantId = user.Id,
                    OrganizationName = registrationRequestDTO.OrganizationName,
                    PropertyType = registrationRequestDTO.PropertyType,
                    Units = registrationRequestDTO.Units,
                    SEODropdown = registrationRequestDTO.SEODropdown
                };

                await _db.AdditionalUserData.AddAsync(additionalData);
                await _db.SaveChangesAsync();
            }

            return true;

        }

        public async Task<bool> ValidateCurrentPassword(string userId, string currentPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                return await _userManager.CheckPasswordAsync(user, currentPassword);
            }
            return false;
        }

        public async Task<bool> ChangePassword(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
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

            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);

        }

        public async Task<bool> FindByPhoneNumberAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && u.IsDeleted != true);
            if (user != null)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> SavePhoneOTP(OTPDto oTPEmailDto)
        {
            var otpRecord = new OTP
            {
                PhoneNumber = oTPEmailDto.PhoneNumber,
                Type = "PhoneNumber",
                Code = oTPEmailDto.Code,
                Expiry = DateTime.UtcNow.AddMinutes(20)
            };

            _db.OTP.Add(otpRecord);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyPhoneOtpAsync(OTPDto oTPDto)
        {
            if (string.IsNullOrWhiteSpace(oTPDto.PhoneNumber) || string.IsNullOrWhiteSpace(oTPDto.Code))
            {
                return false;
            }

            var otpRecord = await _db.OTP.FirstOrDefaultAsync(o => o.PhoneNumber == oTPDto.PhoneNumber && o.Code == oTPDto.Code /*&& o.Expiry > DateTime.UtcNow*/);
            if (otpRecord == null)
            {
                return false;
            }
            _db.OTP.Remove(otpRecord);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SaveEamilOTP(OTPDto oTPEmailDto)
        {
            var otpRecord = new OTP
            {
                Email = oTPEmailDto.Email,
                Type = "Email",
                Code = oTPEmailDto.Code,
                Expiry = DateTime.UtcNow.AddMinutes(20)
            };

            _db.OTP.Add(otpRecord);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyEmailOtpAsync(OTPDto oTPDto)
        {
            if (string.IsNullOrWhiteSpace(oTPDto.Email) || string.IsNullOrWhiteSpace(oTPDto.Code))
            {
                return false;
            }

            var otpRecord = await _db.OTP.FirstOrDefaultAsync(o => o.Email == oTPDto.Email && o.Code == oTPDto.Code /*&& o.Expiry > DateTime.UtcNow.AddMinutes(20)*/);
            if (otpRecord == null)
            {
                return false;
            }
            _db.OTP.Remove(otpRecord);
            await _db.SaveChangesAsync();

            return true;
        }

        public string GenerateOTP()
        {
            const string validChars = "0123456789";
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            for (int i = 0; i < 6; i++)
            {
                int index = rnd.Next(validChars.Length);
                sb.Append(validChars[index]);
            }
            return sb.ToString();
        }

        public async Task DeleteExpiredOtpRecordsAsync()
        {
            var currentTime = DateTime.UtcNow;

            var expiredOtpRecords = _db.OTP.Where(o => o.Expiry < currentTime).ToList();

            _db.OTP.RemoveRange(expiredOtpRecords);
            await _db.SaveChangesAsync();
        }

        public async Task<ApplicationUser> FindByUserId(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            return await _userManager.Users
                                 .FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted != true);
        }

        public async Task<ProfileModel> GetProfileModelAsync(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted != true);

            if (user == null)
            {
                return null;
            }

            var profile = new ProfileModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Address2 = user.Address2,
                Locality = user.Locality,
                District = user.District,
                NewPictureBase64 = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"AspNetUsers_Picture_" + user.Id + Path.GetExtension(user.Picture)}",
                Picture = user.Picture,
                Region = user.Region,
                PostalCode = user.PostalCode,
                Country = user.Country,
                TermsAndConditons = user.TermsAndConditons,
                Currency = user.Currency,
                Status = true
            };

            return profile;
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

            string emailContent = $"To reset your password, follow this link: {callbackUrl}";
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

        #region Email Encryption for Password Reset

        private readonly string EncryptionKey = "bXlTZWN1cmVLZXlIZXJlMTIzNDU2Nzg5";

        public async Task<string> EncryptEmail(string email)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(email);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    email = Convert.ToBase64String(ms.ToArray());
                }
            }
            return email;
        }

        public async Task<string> DecryptEmail(string encryptedEmail)
        {
            encryptedEmail = encryptedEmail.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(encryptedEmail);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    encryptedEmail = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return encryptedEmail;
        }

        #endregion


        #region PropertyType

        public async Task<List<PropertyTypeDto>> GetAllPropertyTypesDll(Filter filter)
        {
            try
            {
                var propertyTypeDtos = await (from pt in _db.PropertyType
                                              where pt.IsDeleted != true
                                              select new PropertyTypeDto
                                              {
                                                  PropertyTypeId = pt.PropertyTypeId,
                                                  PropertyTypeName = pt.PropertyTypeName,
                                                  AddedBy = pt.AddedBy
                                              })
                                              .AsNoTracking()
                                              .ToListAsync();
                //if (string.IsNullOrEmpty(filter.AddedBy))
                //{
                //    propertyTypeDtos = propertyTypeDtos.Where(x => x.AddedBy == filter.AddedBy).ToList();
                //}

                return propertyTypeDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PropertyTypeDto>> GetAllPropertyTypesAsync()
        {
            try
            {
                var propertyTypes = await _db.PropertyType.Where(x => x.IsDeleted != true)
                                             .AsNoTracking()
                                             .ToListAsync();

                var propertyTypeDtos = propertyTypes.Select(tenant => new PropertyTypeDto
                {
                    PropertyTypeId = tenant.PropertyTypeId,
                    PropertyTypeName = tenant.PropertyTypeName,
                    Icon_String = tenant.Icon_String,
                    Icon_SVG = tenant.Icon_SVG,
                    AppTenantId = tenant.AppTenantId,
                    Status = tenant.Status,
                    IsDeleted = tenant.IsDeleted,
                    AddedDate = tenant.AddedDate,
                    AddedBy = tenant.AddedBy,
                    ModifiedDate = tenant.ModifiedDate,
                    ModifiedBy = tenant.ModifiedBy
                }).ToList();


                return propertyTypeDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PropertyTypeDto>> GetPropertyTypeByIdAsync(string tenantId)
        {
            var tenants = await _db.PropertyType
                                   .AsNoTracking()
                                   .Where(t => t.AppTenantId == Guid.Parse(tenantId) && t.IsDeleted != true)
                                   .ToListAsync();

            if (tenants == null || !tenants.Any()) return new List<PropertyTypeDto>();

            var tenantDtos = tenants.Select(tenant => new PropertyTypeDto
            {
                PropertyTypeId = tenant.PropertyTypeId,
                PropertyTypeName = tenant.PropertyTypeName,
                Icon_String = tenant.Icon_String,
                Icon_SVG = tenant.Icon_SVG,
                AppTenantId = tenant.AppTenantId,
                Status = tenant.Status,
                IsDeleted = tenant.IsDeleted,
                AddedDate = tenant.AddedDate,
                AddedBy = tenant.AddedBy,
                ModifiedDate = tenant.ModifiedDate,
                ModifiedBy = tenant.ModifiedBy
            }).ToList();

            return tenantDtos;
        }

        public async Task<PropertyTypeDto> GetSinglePropertyTypeByIdAsync(int propertytypeId)
        {
            var propertyType = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == propertytypeId && t.IsDeleted != true);

            if (propertyType == null)
                return new PropertyTypeDto();

            var propertyTypeDto = new PropertyTypeDto
            {
                PropertyTypeId = propertyType.PropertyTypeId,
                PropertyTypeName = propertyType.PropertyTypeName,
                Icon_String = propertyType.Icon_String,
                Icon_SVG = propertyType.Icon_SVG,
                AppTenantId = propertyType.AppTenantId,
                Status = propertyType.Status,
                IsDeleted = propertyType.IsDeleted,
                AddedDate = propertyType.AddedDate,
                AddedBy = propertyType.AddedBy,
                ModifiedDate = propertyType.ModifiedDate,
                ModifiedBy = propertyType.ModifiedBy
            };

            return propertyTypeDto;
        }

        public async Task<bool> CreatePropertyTypeAsync(PropertyTypeDto tenant)
        {
            var newTenant = new PropertyType
            {
                PropertyTypeId = tenant.PropertyTypeId,
                PropertyTypeName = tenant.PropertyTypeName,
                Icon_String = tenant.Icon_String,
                Icon_SVG = tenant.Icon_SVG,
                AppTenantId = tenant.AppTenantId,
                Status = tenant.Status,
                IsDeleted = tenant.IsDeleted,
                AddedDate = DateTime.Now,
                AddedBy = tenant.AddedBy,
                ModifiedDate = tenant.ModifiedDate,
                ModifiedBy = tenant.ModifiedBy
            };

            await _db.PropertyType.AddAsync(newTenant);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdatePropertyTypeAsync(PropertyTypeDto tenant)
        {
            var propertyType = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == tenant.PropertyTypeId && t.IsDeleted != true);

            if (propertyType == null)
            {
                return false;
            }

            propertyType.PropertyTypeName = tenant.PropertyTypeName;
            propertyType.Icon_String = tenant.Icon_String;
            propertyType.Icon_SVG = tenant.Icon_SVG;
            propertyType.AppTenantId = tenant.AppTenantId;
            propertyType.Status = tenant.Status;
            propertyType.IsDeleted = tenant.IsDeleted;
            propertyType.ModifiedDate = DateTime.Now;
            propertyType.ModifiedBy = tenant.ModifiedBy;

            _db.PropertyType.Update(propertyType);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeletePropertyTypeAsync(int tenantId)
        {
            var tenant = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == tenantId && t.IsDeleted != true);
            if (tenant == null) return false;

            _db.PropertyType.Remove(tenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        #endregion



        #region PropertySubType

        public async Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAllAsync(string tenantId)
        {
            try
            {
                var propertyTypes = await _db.PropertySubType
                                             .AsNoTracking()
                                             .Where(t => t.AppTenantId == Guid.Parse(tenantId) && t.IsDeleted != true)
                                             .ToListAsync();

                var propertyTypeDtos = propertyTypes.Select(tenant => new PropertySubTypeDto
                {
                    PropertySubTypeId = tenant.PropertySubTypeId,
                    PropertyTypeId = tenant.PropertyTypeId,
                    PropertySubTypeName = tenant.PropertySubTypeName,
                    Icon_String = tenant.Icon_String,
                    Icon_SVG = tenant.Icon_SVG,
                    AppTenantId = tenant.AppTenantId,
                    Status = tenant.Status,
                    IsDeleted = tenant.IsDeleted,
                    AddedDate = tenant.AddedDate,
                    AddedBy = tenant.AddedBy,
                    ModifiedDate = tenant.ModifiedDate,
                    ModifiedBy = tenant.ModifiedBy
                }).ToList();


                return propertyTypeDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PropertySubTypeDto>> GetAllPropertySubTypesAsync()
        {
            try
            {
                var propertyTypes = await _db.PropertySubType
                                             .AsNoTracking()
                                             .ToListAsync();

                var propertyTypeDtos = propertyTypes.Where(x => x.IsDeleted != true).Select(tenant => new PropertySubTypeDto
                {
                    PropertySubTypeId = tenant.PropertySubTypeId,
                    PropertyTypeId = tenant.PropertyTypeId,
                    PropertySubTypeName = tenant.PropertySubTypeName,
                    Icon_String = tenant.Icon_String,
                    Icon_SVG = tenant.Icon_SVG,
                    AppTenantId = tenant.AppTenantId,
                    Status = tenant.Status,
                    IsDeleted = tenant.IsDeleted,
                    AddedDate = tenant.AddedDate,
                    AddedBy = tenant.AddedBy,
                    ModifiedDate = tenant.ModifiedDate,
                    ModifiedBy = tenant.ModifiedBy
                }).ToList();


                return propertyTypeDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAsync(int propertytypeId)
        {
            var tenants = await _db.PropertySubType
                                   .AsNoTracking()
                                   .Where(t => t.PropertyTypeId == propertytypeId && t.IsDeleted != true)
                                   .ToListAsync();

            if (tenants == null || !tenants.Any()) return new List<PropertySubTypeDto>();

            var tenantDtos = tenants.Select(tenant => new PropertySubTypeDto
            {
                PropertySubTypeId = tenant.PropertySubTypeId,
                PropertyTypeId = tenant.PropertyTypeId,
                PropertySubTypeName = tenant.PropertySubTypeName,
                Icon_String = tenant.Icon_String,
                Icon_SVG = tenant.Icon_SVG,
                AppTenantId = tenant.AppTenantId,
                Status = tenant.Status,
                IsDeleted = tenant.IsDeleted,
                AddedDate = tenant.AddedDate,
                AddedBy = tenant.AddedBy,
                ModifiedDate = tenant.ModifiedDate,
                ModifiedBy = tenant.ModifiedBy
            }).ToList();

            return tenantDtos;
        }

        public async Task<PropertySubTypeDto> GetSinglePropertySubTypeByIdAsync(int propertysubtypeId)
        {
            var tenant = await _db.PropertySubType.FirstOrDefaultAsync(t => t.PropertySubTypeId == propertysubtypeId && t.IsDeleted != true);

            if (tenant == null)
                return new PropertySubTypeDto();

            var tenantDto = new PropertySubTypeDto
            {
                PropertySubTypeId = tenant.PropertySubTypeId,
                PropertyTypeId = tenant.PropertyTypeId,
                PropertySubTypeName = tenant.PropertySubTypeName,
                Icon_String = tenant.Icon_String,
                Icon_SVG = tenant.Icon_SVG,
                AppTenantId = tenant.AppTenantId,
                Status = tenant.Status,
                IsDeleted = tenant.IsDeleted,
                AddedDate = tenant.AddedDate,
                AddedBy = tenant.AddedBy,
                ModifiedDate = tenant.ModifiedDate,
                ModifiedBy = tenant.ModifiedBy
            };

            return tenantDto;
        }

        public async Task<bool> CreatePropertySubTypeAsync(PropertySubTypeDto tenant)
        {
            var newTenant = new PropertySubType
            {
                PropertySubTypeId = tenant.PropertySubTypeId,
                PropertyTypeId = tenant.PropertyTypeId,
                PropertySubTypeName = tenant.PropertySubTypeName,
                Icon_String = tenant.Icon_String,
                Icon_SVG = tenant.Icon_SVG,
                AppTenantId = tenant.AppTenantId,
                Status = tenant.Status,
                IsDeleted = tenant.IsDeleted,
                AddedDate = tenant.AddedDate,
                AddedBy = tenant.AddedBy,
                ModifiedDate = tenant.ModifiedDate,
                ModifiedBy = tenant.ModifiedBy
            };

            await _db.PropertySubType.AddAsync(newTenant);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdatePropertySubTypeAsync(PropertySubTypeDto tenant)
        {
            var propertyType = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == tenant.PropertyTypeId && t.IsDeleted != true);
            if (tenant == null) return false;

            var newTenant = new PropertySubType
            {
                PropertySubTypeId = tenant.PropertySubTypeId,
                PropertyTypeId = tenant.PropertyTypeId,
                PropertySubTypeName = propertyType.PropertyTypeName,
                Icon_String = tenant.Icon_String,
                Icon_SVG = tenant.Icon_SVG,
                AppTenantId = tenant.AppTenantId,
                Status = tenant.Status,
                IsDeleted = tenant.IsDeleted,
                AddedDate = tenant.AddedDate,
                AddedBy = tenant.AddedBy,
                ModifiedDate = tenant.ModifiedDate,
                ModifiedBy = tenant.ModifiedBy
            };

            _db.PropertySubType.Update(newTenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeletePropertySubTypeAsync(int propertysubtypeId)
        {
            var tenant = await _db.PropertySubType.FirstOrDefaultAsync(t => t.PropertySubTypeId == propertysubtypeId && t.IsDeleted != true);
            if (tenant == null) return false;

            _db.PropertySubType.Remove(tenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }



        #endregion


        #region Tenant

        public async Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync()
        {
            var tenants = await _db.Tenant.Where(x => x.IsDeleted != true)
                                   .AsNoTracking()
                                   .ToListAsync();

            var tenantDtos = tenants.Select(tenant => new TenantModelDto
            {
                TenantId = tenant.TenantId,
                FirstName = tenant.FirstName,
                LastName = tenant.LastName,
                EmailAddress = tenant.EmailAddress,
                PhoneNumber = tenant.PhoneNumber,
                EmergencyContactInfo = tenant.EmergencyContactInfo,
                LeaseAgreementId = tenant.LeaseAgreementId,
                TenantNationality = tenant.TenantNationality,
                Gender = tenant.Gender,
                DOB = tenant.DOB,
                VAT = tenant.VAT,
                LegalName = tenant.LegalName,
                Account_Name = tenant.Account_Name,
                Account_Holder = tenant.Account_Holder,
                Account_IBAN = tenant.Account_IBAN,
                Account_Swift = tenant.Account_Swift,
                Account_Bank = tenant.Account_Bank,
                Account_Currency = tenant.Account_Currency,
                Address = tenant.Address,
                Address2 = tenant.Address2,
                Locality = tenant.Locality,
                Region = tenant.Region,
                PostalCode = tenant.PostalCode,
                Country = tenant.Country,
                CountryCode = tenant.CountryCode,
                PictureName = tenant.Picture,
                Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Picture_" + tenant.TenantId + Path.GetExtension(tenant.Picture)}",
                DocumentName = tenant.Document,
                Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Document_" + tenant.TenantId + Path.GetExtension(tenant.Document)}",
                AddedBy = tenant.AddedBy,
                AddedDate = tenant.AddedDate,
                ModifiedDate = tenant.ModifiedDate,
                AppTid = tenant.AppTenantId.ToString()
            }).ToList();
            return tenantDtos;
        }

        public async Task<IEnumerable<TenantModelDto>> GetAllTenantsDllAsync(Filter filter)
        {

            try
            {
                var tenants = await (from tenant in _db.Tenant
                                     where tenant.IsDeleted != true && tenant.AddedBy == filter.AddedBy
                                     select new TenantModelDto
                                     {
                                         TenantId = tenant.TenantId,
                                         FirstName = tenant.FirstName,
                                         LastName = tenant.LastName,
                                         AddedBy = tenant.AddedBy,
                                     })
                     .AsNoTracking()
                     .ToListAsync();


                return tenants;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Tenants: {ex.Message}");
                throw;
            }

        }
        
        public async Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId)
        {
            var tenants = await _db.Tenant
                                   .AsNoTracking()
                                   .Where(t => t.AppTenantId == Guid.Parse(tenantId) && t.IsDeleted != true)
                                   .ToListAsync();

            if (tenants == null || !tenants.Any()) return new List<TenantModelDto>();

            // Manual mapping from Tenant to TenantModelDto
            var tenantDtos = tenants.Select(tenant => new TenantModelDto
            {
                TenantId = tenant.TenantId,
                FirstName = tenant.FirstName,
                MiddleName = tenant.MiddleName,
                LastName = tenant.LastName,
                EmailAddress = tenant.EmailAddress,
                PhoneNumber = tenant.PhoneNumber,
                Unit = tenant.Unit,
                District = tenant.District,
                EmergencyContactInfo = tenant.EmergencyContactInfo,
                LeaseAgreementId = tenant.LeaseAgreementId,
                TenantNationality = tenant.TenantNationality,
                Gender = tenant.Gender,
                DOB = tenant.DOB,
                VAT = tenant.VAT,
                LegalName = tenant.LegalName,
                Account_Name = tenant.Account_Name,
                Account_Holder = tenant.Account_Holder,
                Account_IBAN = tenant.Account_IBAN,
                Account_Swift = tenant.Account_Swift,
                Account_Bank = tenant.Account_Bank,
                Account_Currency = tenant.Account_Currency,
                Address = tenant.Address,
                Address2 = tenant.Address2,
                Locality = tenant.Locality,
                Region = tenant.Region,
                PostalCode = tenant.PostalCode,
                Country = tenant.Country,
                CountryCode = tenant.CountryCode,
                PictureName = tenant.Picture,
                Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Picture_" + tenant.TenantId + Path.GetExtension(tenant.Picture)}",
                DocumentName = tenant.Document,
                Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Document_" + tenant.TenantId + Path.GetExtension(tenant.Document)}"
            }).ToList();

            return tenantDtos;
        }

        public async Task<TenantModelDto> GetSingleTenantByIdAsync(int tenantId)
        {
            var tenant = await _db.Tenant
                  .Include(t => t.Pets)
                  .Include(t => t.Vehicle)
                  .Include(t => t.TenantDependent)
                  .Include(t => t.CoTenant)
                  .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsDeleted != true);

            if (tenant == null)
                return new TenantModelDto();

            var tenantDto = new TenantModelDto
            {
                TenantId = tenant.TenantId,
                PictureName = tenant.Picture,
                Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Picture_" + tenant.TenantId + Path.GetExtension(tenant.Picture)}",
                DocumentName = tenant.Document,
                Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Document_" + tenant.TenantId + Path.GetExtension(tenant.Document)}",
                FirstName = tenant.FirstName,
                LastName = tenant.LastName,
                EmailAddress = tenant.EmailAddress,
                PhoneNumber = tenant.PhoneNumber,
                EmergencyContactInfo = tenant.EmergencyContactInfo,
                LeaseAgreementId = tenant.LeaseAgreementId,
                TenantNationality = tenant.TenantNationality,
                Gender = tenant.Gender,
                DOB = tenant.DOB,
                VAT = tenant.VAT,
                LegalName = tenant.LegalName,
                Account_Name = tenant.Account_Name,
                Account_Holder = tenant.Account_Holder,
                Account_IBAN = tenant.Account_IBAN,
                Account_Swift = tenant.Account_Swift,
                Account_Bank = tenant.Account_Bank,
                Account_Currency = tenant.Account_Currency,
                Address = tenant.Address,
                Address2 = tenant.Address2,
                Locality = tenant.Locality,
                Region = tenant.Region,
                PostalCode = tenant.PostalCode,
                Country = tenant.Country,
                CountryCode = tenant.CountryCode,
                Unit = tenant.Unit,
                AddedBy = tenant.AddedBy,
                AddedDate = tenant.AddedDate,
                Pets = tenant.Pets.Where(x => x.IsDeleted != true).Select(p => new PetDto
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Breed = p.Breed,
                    Type = p.Type,
                    Quantity = p.Quantity,
                    PictureName = p.Picture,
                    Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Pets_Picture_" + p.PetId + Path.GetExtension(p.Picture)}",

                }).ToList(),
                Vehicles = tenant.Vehicle.Where(x => x.IsDeleted != true).Select(v => new VehicleDto
                {
                    VehicleId = v.VehicleId,
                    Manufacturer = v.Manufacturer,
                    ModelName = v.ModelName,
                    ModelVariant = v.ModelVariant,
                    Year = v.Year
                }).ToList(),
                Dependent = tenant.TenantDependent.Where(x => x.IsDeleted != true).Select(d => new TenantDependentDto
                {
                    TenantDependentId = d.TenantDependentId,
                    TenantId = d.TenantId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    EmailAddress = d.EmailAddress,
                    PhoneNumber = d.PhoneNumber,
                    DOB = d.DOB,
                    Relation = d.Relation
                }).ToList(),
                CoTenant = tenant.CoTenant.Where(x => x.IsDeleted != true).Select(c => new CoTenantDto
                {
                    CoTenantId = c.CoTenantId,
                    TenantId = c.TenantId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    EmailAddress = c.EmailAddress,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    Unit = c.Unit,
                    District = c.District,
                    Region = c.Region,
                    PostalCode = c.PostalCode,
                    Country = c.Country
                }).ToList()
            };

            return tenantDto;
        }

        public async Task<bool> CreateTenantAsync(TenantModelDto tenantDto)
        {

            var newTenant = new Tenant
            {
                FirstName = tenantDto.FirstName,
                LastName = tenantDto.LastName,
                MiddleName = tenantDto.MiddleName,
                EmailAddress = tenantDto.EmailAddress,
                EmailAddress2 = tenantDto.EmailAddress2,
                PhoneNumber = tenantDto.PhoneNumber,
                PhoneNumber2 = tenantDto.PhoneNumber2,
                EmergencyContactInfo = tenantDto.EmergencyContactInfo,
                EmergencyName = tenantDto.EmergencyName,
                EmergencyEmailAddress = tenantDto.EmergencyEmailAddress,
                EmergencyRelation = tenantDto.EmergencyRelation,
                EmergencyDetails = tenantDto.EmergencyDetails,
                TenantNationality = tenantDto.TenantNationality,
                Gender = tenantDto.Gender,
                DOB = tenantDto.DOB,
                VAT = tenantDto.VAT,
                LegalName = tenantDto.LegalName,
                Account_Name = tenantDto.Account_Name,
                Account_Holder = tenantDto.Account_Holder,
                Account_IBAN = tenantDto.Account_IBAN,
                Account_Swift = tenantDto.Account_Swift,
                Account_Bank = tenantDto.Account_Bank,
                Account_Currency = tenantDto.Account_Currency,
                Address = tenantDto.Address,
                Address2 = tenantDto.Address2,
                Locality = tenantDto.Locality,
                District = tenantDto.District,
                Region = tenantDto.Region,
                PostalCode = tenantDto.PostalCode,
                Country = tenantDto.Country,
                CountryCode = tenantDto.CountryCode,
                AppTenantId = tenantDto.AppTenantId,
                Picture = tenantDto.PictureName,
                Document = tenantDto.DocumentName,
                AddedBy = tenantDto.AddedBy,
                AddedDate = DateTime.Now
            };

            _db.Tenant.Add(newTenant);
            await _db.SaveChangesAsync();


            if (tenantDto.Picture != null)
            {
                var ext = Path.GetExtension(tenantDto.PictureName);
                await _googleCloudStorageService.UploadImagebyBase64Async(tenantDto.Picture, "Tenant_Picture_" + newTenant.TenantId + ext);
            }

            if (tenantDto.Document != null)
            {
                var ext = Path.GetExtension(tenantDto.DocumentName);
                await _googleCloudStorageService.UploadImagebyBase64Async(tenantDto.Document, "Tenant_Document_" + newTenant.TenantId + ext);
            }


            if (tenantDto.Pets != null)
            {
                foreach (var petDto in tenantDto.Pets)
                {
                    var pet = new Pets
                    {
                        TenantId = newTenant.TenantId,
                        Name = petDto.Name,
                        Breed = petDto.Breed,
                        Type = petDto.Type,
                        Quantity = petDto.Quantity,
                        Picture = petDto.PictureName,
                        AppTenantId = tenantDto.AppTenantId,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };

                    _db.Pets.Add(pet);

                    await _db.SaveChangesAsync();
                    if (petDto.Picture != null)
                    {
                        var ext = Path.GetExtension(petDto.PictureName);
                        await _googleCloudStorageService.UploadImagebyBase64Async(petDto.Picture, "Pets_Picture_" + pet.PetId + ext);
                    }
                }
            }

            if (tenantDto.Vehicles != null)
            {
                foreach (var vehicleDto in tenantDto.Vehicles)
                {
                    var vehicle = new Vehicle
                    {
                        TenantId = newTenant.TenantId,
                        Manufacturer = vehicleDto.Manufacturer,
                        ModelName = vehicleDto.ModelName,
                        ModelVariant = vehicleDto.ModelVariant,
                        Year = vehicleDto.Year,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };

                    _db.Vehicle.Add(vehicle);
                }

                await _db.SaveChangesAsync();
            }
            if (tenantDto.Dependent != null)
            {
                foreach (var dependentDto in tenantDto.Dependent)
                {
                    var dependent = new TenantDependent
                    {
                        TenantId = newTenant.TenantId,
                        FirstName = dependentDto.FirstName,
                        LastName = dependentDto.LastName,
                        EmailAddress = dependentDto.EmailAddress,
                        PhoneNumber = dependentDto.PhoneNumber,
                        DOB = dependentDto.DOB,
                        Relation = dependentDto.Relation,
                        AppTenantId = tenantDto.AppTenantId,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };

                    _db.TenantDependent.Add(dependent);
                }

                await _db.SaveChangesAsync();
            }
            if (tenantDto.CoTenant != null)
            {
                foreach (var coTenantDto in tenantDto.CoTenant)
                {
                    var coTenant = new CoTenant
                    {
                        TenantId = newTenant.TenantId,
                        FirstName = coTenantDto.FirstName,
                        LastName = coTenantDto.LastName,
                        EmailAddress = coTenantDto.EmailAddress,
                        PhoneNumber = coTenantDto.PhoneNumber,
                        Address = coTenantDto.Address,
                        Unit = coTenantDto.Unit,
                        District = coTenantDto.District,
                        Region = coTenantDto.Region,
                        PostalCode = coTenantDto.PostalCode,
                        Country = coTenantDto.Country,
                        AppTenantId = tenantDto.AppTenantId,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };

                    _db.CoTenant.Add(coTenant);
                }
                await _db.SaveChangesAsync();
            }

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateTenantAsync(TenantModelDto tenantDto)
        {
            var tenant = await _db.Tenant
                .Include(t => t.Pets)
                .Include(t => t.Vehicle)
                .Include(t => t.TenantDependent)
                .Include(t => t.CoTenant).FirstOrDefaultAsync(t => t.TenantId == tenantDto.TenantId && t.IsDeleted != true);
            if (tenant == null)
                return false;

            tenant.FirstName = tenantDto.FirstName;
            tenant.LastName = tenantDto.LastName;
            tenant.EmailAddress = tenantDto.EmailAddress;
            tenant.PhoneNumber = tenantDto.PhoneNumber;
            tenant.EmergencyContactInfo = tenantDto.EmergencyContactInfo;
            tenant.LeaseAgreementId = tenantDto.LeaseAgreementId;
            tenant.TenantNationality = tenantDto.TenantNationality;
            tenant.Gender = tenantDto.Gender;
            tenant.DOB = tenantDto.DOB;
            tenant.VAT = tenantDto.VAT;
            tenant.Status = true;
            tenant.LegalName = tenantDto.LegalName;
            tenant.Account_Name = tenantDto.Account_Name;
            tenant.Account_Holder = tenantDto.Account_Holder;
            tenant.Account_IBAN = tenantDto.Account_IBAN;
            tenant.Account_Swift = tenantDto.Account_Swift;
            tenant.Account_Bank = tenantDto.Account_Bank;
            tenant.Account_Currency = tenantDto.Account_Currency;
            tenant.AppTenantId = tenantDto.AppTenantId;
            tenant.Address = tenantDto.Address;
            tenant.Address2 = tenantDto.Address2;
            tenant.Locality = tenantDto.Locality;
            tenant.Region = tenantDto.Region;
            tenant.PostalCode = tenantDto.PostalCode;
            tenant.Country = tenantDto.Country;
            tenant.CountryCode = tenantDto.CountryCode;
            tenant.Document = tenantDto.DocumentName;
            tenant.Picture = tenantDto.PictureName;
            tenant.ModifiedBy = tenantDto.AddedBy;
            tenant.ModifiedDate = DateTime.Now;

            foreach (var petDto in tenantDto.Pets)
            {
                var existingPet = tenant.Pets.FirstOrDefault(p => p.PetId == petDto.PetId && p.IsDeleted != true);
                if (existingPet != null)
                {
                    existingPet.Name = petDto.Name;
                    existingPet.Breed = petDto.Breed;
                    existingPet.Type = petDto.Type;
                    existingPet.Quantity = petDto.Quantity;
                    existingPet.Picture = petDto.PictureName;
                    existingPet.AppTenantId = petDto.AppTenantId;
                    existingPet.ModifiedBy = tenantDto.AddedBy;
                    existingPet.ModifiedDate = DateTime.Now;

                    if (petDto.Picture != null)
                    {
                        var ext = Path.GetExtension(petDto.PictureName);
                        await _googleCloudStorageService.UploadImagebyBase64Async(petDto.Picture, "Pets_Picture_" + existingPet.PetId + ext);
                    }
                }
                else
                {
                    var newPet = new Pets
                    {
                        TenantId = tenant.TenantId,
                        Name = petDto.Name,
                        Breed = petDto.Breed,
                        Type = petDto.Type,
                        Quantity = petDto.Quantity,
                        Picture = petDto.PictureName,
                        AppTenantId = tenantDto.AppTenantId,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };
                    tenant.Pets.Add(newPet);
                    await _db.SaveChangesAsync();
                    if (petDto.Picture != null)
                    {
                        var ext = Path.GetExtension(petDto.PictureName);
                        await _googleCloudStorageService.UploadImagebyBase64Async(petDto.Picture, "Pets_Picture_" + newPet.PetId + ext);
                    }
                }
            }

            // Update or add vehicles
            foreach (var vehicleDto in tenantDto.Vehicles)
            {
                var existingVehicle = tenant.Vehicle.FirstOrDefault(v => v.VehicleId == vehicleDto.VehicleId && v.IsDeleted != true);
                if (existingVehicle != null)
                {
                    existingVehicle.Manufacturer = vehicleDto.Manufacturer;
                    existingVehicle.ModelName = vehicleDto.ModelName;
                    existingVehicle.ModelVariant = vehicleDto.ModelVariant;
                    existingVehicle.Year = vehicleDto.Year;
                    existingVehicle.ModifiedBy = tenantDto.AddedBy;
                    existingVehicle.ModifiedDate = DateTime.Now;
                }
                else
                {
                    var newVehicle = new Vehicle
                    {
                        TenantId = tenant.TenantId,
                        Manufacturer = vehicleDto.Manufacturer,
                        ModelName = vehicleDto.ModelName,
                        Year = vehicleDto.Year,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };
                    tenant.Vehicle.Add(newVehicle);
                }
            }

            foreach (var dependentDto in tenantDto.Dependent)
            {
                var existingDependent = tenant.TenantDependent.FirstOrDefault(d => d.TenantDependentId == dependentDto.TenantDependentId && d.IsDeleted != true);
                if (existingDependent != null)
                {
                    existingDependent.FirstName = dependentDto.FirstName;
                    existingDependent.LastName = dependentDto.LastName;
                    existingDependent.EmailAddress = dependentDto.EmailAddress;
                    existingDependent.PhoneNumber = dependentDto.PhoneNumber;
                    existingDependent.DOB = dependentDto.DOB;
                    existingDependent.Relation = dependentDto.Relation;
                    existingDependent.AppTenantId = tenantDto.AppTenantId;
                    existingDependent.ModifiedDate = DateTime.Now;
                    existingDependent.ModifiedBy = tenantDto.AddedBy;
                }
                else
                {
                    var newDependent = new TenantDependent
                    {
                        TenantId = tenant.TenantId,
                        FirstName = dependentDto.FirstName,
                        LastName = dependentDto.LastName,
                        EmailAddress = dependentDto.EmailAddress,
                        PhoneNumber = dependentDto.PhoneNumber,
                        DOB = dependentDto.DOB,
                        Relation = dependentDto.Relation,
                        AppTenantId = tenantDto.AppTenantId,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };
                    tenant.TenantDependent.Add(newDependent);
                }
            }

            foreach (var coTenantDto in tenantDto.CoTenant)
            {
                var existingCoTenant = tenant.CoTenant.FirstOrDefault(ct => ct.CoTenantId == coTenantDto.CoTenantId && ct.IsDeleted != true);
                if (existingCoTenant != null)
                {

                    existingCoTenant.FirstName = coTenantDto.FirstName;
                    existingCoTenant.LastName = coTenantDto.LastName;
                    existingCoTenant.EmailAddress = coTenantDto.EmailAddress;
                    existingCoTenant.PhoneNumber = coTenantDto.PhoneNumber;
                    existingCoTenant.Address = coTenantDto.Address;
                    existingCoTenant.Unit = coTenantDto.Unit;
                    existingCoTenant.District = coTenantDto.District;
                    existingCoTenant.Region = coTenantDto.Region;
                    existingCoTenant.PostalCode = coTenantDto.PostalCode;
                    existingCoTenant.Country = coTenantDto.Country;
                    existingCoTenant.IsDeleted = false;
                    existingCoTenant.Status = true;
                    existingCoTenant.AppTenantId = tenantDto.AppTenantId;
                    existingCoTenant.ModifiedBy = tenantDto.AddedBy;
                    existingCoTenant.ModifiedDate = DateTime.Now;
                }
                else
                {

                    var newCoTenant = new CoTenant
                    {
                        TenantId = tenant.TenantId,
                        FirstName = coTenantDto.FirstName,
                        LastName = coTenantDto.LastName,
                        EmailAddress = coTenantDto.EmailAddress,
                        PhoneNumber = coTenantDto.PhoneNumber,
                        Address = coTenantDto.Address,
                        Unit = coTenantDto.Unit,
                        District = coTenantDto.District,
                        Region = coTenantDto.Region,
                        PostalCode = coTenantDto.PostalCode,
                        Country = coTenantDto.Country,
                        IsDeleted = false,
                        Status = true,
                        AppTenantId = tenantDto.AppTenantId,
                        AddedBy = tenantDto.AddedBy,
                        AddedDate = DateTime.Now
                    };
                    tenant.CoTenant.Add(newCoTenant);
                }
            }
            var result = await _db.SaveChangesAsync();

            if (tenantDto.Picture != null)
            {
                var ext = Path.GetExtension(tenantDto.PictureName);
                await _googleCloudStorageService.UploadImagebyBase64Async(tenantDto.Picture, "Tenant_Picture_" + tenant.TenantId + ext);
            }

            if (tenantDto.Document != null)
            {
                var ext = Path.GetExtension(tenantDto.DocumentName);
                await _googleCloudStorageService.UploadImagebyBase64Async(tenantDto.Document, "Tenant_Document_" + tenant.TenantId + ext);
            }

            return result > 0;
        }

        public async Task<APIResponse> DeleteTenantAsync(int tenantId)
        {
            var response = new APIResponse();

            try
            {
                var isLeaseRefrence = await _db.Lease.AnyAsync(x => x.TenantsTenantId == tenantId && x.IsDeleted != true);
                if (isLeaseRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This tenant has refrence in lease.");
                    return response;
                }

                var isTaskRefrence = await _db.TaskRequest.AnyAsync(x => x.TenantId == tenantId && x.IsDeleted != true);
                if (isTaskRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This tenant has refrence in task.");
                    return response;
                }

                var isDocumentsRefrence = await _db.Documents.AnyAsync(x => x.TenantId == tenantId && x.IsDeleted != true);
                if (isDocumentsRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This tenant has refrence in Documents.");
                    return response;
                }

                //var isCommunicationRefrence = await _db.Communication.AnyAsync(x => x.TenantIds == tenantId && x.IsDeleted != true);
                //if (isCommunicationRefrence)
                //{
                //    response.StatusCode = HttpStatusCode.InternalServerError;
                //    response.IsSuccess = false;
                //    response.ErrorMessages.Add("This owner has refrence in task.");
                //    return response;
                //}


                var tenant = await _db.Tenant.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsDeleted != true);
                if (tenant == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Tenant not found.");
                    return response;
                }

                tenant.IsDeleted = true;
                _db.Tenant.Update(tenant);
               
                await _db.SaveChangesAsync();

                var pets = await _db.Pets.Where(x => x.TenantId == tenantId && x.IsDeleted != true).ToListAsync();
                pets.ForEach(x => x.IsDeleted = true);
                _db.Pets.UpdateRange(pets);

                var vehicles = await _db.Vehicle.Where(x => x.TenantId == tenantId && x.IsDeleted != true).ToListAsync();
                vehicles.ForEach(x => x.IsDeleted = true);
                _db.Vehicle.UpdateRange(vehicles);

                var dependents = await _db.TenantDependent.Where(x => x.TenantId == tenantId && x.IsDeleted != true).ToListAsync();
                dependents.ForEach(x => x.IsDeleted = true);
                _db.TenantDependent.UpdateRange(dependents);

                var coTenant = await _db.CoTenant.Where(x => x.TenantId == tenantId && x.IsDeleted != true).ToListAsync();
                coTenant.ForEach(x => x.IsDeleted = true);
                _db.CoTenant.UpdateRange(coTenant);

                await _db.SaveChangesAsync();
                response.StatusCode = HttpStatusCode.OK;
                response.Result = "Tenant and related entities deleted successfully.";
      
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
                throw new Exception(string.Join(", ", response.ErrorMessages));
            }

            return response;
        }


        public async Task<List<TenantModelDto>> GetTenantsReportAsync(ReportFilter reportFilter)
        {

            try
            {
                var tenants = await (from tenant in _db.Tenant
                                  where tenant.IsDeleted != true && tenant.AddedBy == reportFilter.AddedBy
                                  select new TenantModelDto
                                  {
                                      TenantId = tenant.TenantId,
                                      FirstName = tenant.FirstName,
                                      LastName = tenant.LastName,
                                      EmailAddress = tenant.EmailAddress,
                                      PhoneNumber = tenant.PhoneNumber,
                                      EmergencyContactInfo = tenant.EmergencyContactInfo,
                                      LeaseAgreementId = tenant.LeaseAgreementId,
                                      TenantNationality = tenant.TenantNationality,
                                      Gender = tenant.Gender,
                                      DOB = tenant.DOB,
                                      VAT = tenant.VAT,
                                      LegalName = tenant.LegalName,
                                      Account_Name = tenant.Account_Name,
                                      Account_Holder = tenant.Account_Holder,
                                      Account_IBAN = tenant.Account_IBAN,
                                      Account_Swift = tenant.Account_Swift,
                                      Account_Bank = tenant.Account_Bank,
                                      Account_Currency = tenant.Account_Currency,
                                      Address = tenant.Address,
                                      Address2 = tenant.Address2,
                                      Locality = tenant.Locality,
                                      Region = tenant.Region,
                                      PostalCode = tenant.PostalCode,
                                      Country = tenant.Country,
                                      CountryCode = tenant.CountryCode,
                                      PictureName = tenant.Picture,
                                      Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Picture_" + tenant.TenantId + Path.GetExtension(tenant.Picture)}",
                                      DocumentName = tenant.Document,
                                      Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Document_" + tenant.TenantId + Path.GetExtension(tenant.Document)}",
                                      AddedBy = tenant.AddedBy,
                                      AddedDate = tenant.AddedDate,
                                      IsTenanted = _db.Lease.Any(x => x.TenantsTenantId == tenant.TenantId && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now) ? (int)TenantTypes.Tenanted : (int)TenantTypes.NonTenanted,
                                  })
                     .AsNoTracking()
                     .ToListAsync();

                if (reportFilter.TenantsIds.Count() > 0 && reportFilter.TenantsIds.Any())
                {
                    tenants = tenants
                        .Where(x => reportFilter.TenantsIds.Contains(x.TenantId))
                        .ToList();
                }

                if (reportFilter.TenantTypes.Count() > 0 && reportFilter.TenantTypes.Any())
                {

                    tenants = tenants
                        .Where(x => reportFilter.TenantTypes.Contains(x.IsTenanted))
                        .ToList();
                }

                if (reportFilter.TenantAddedStartDateFilter != null && reportFilter.TenantAddedEndDateFilter != null)
                {
                    tenants = tenants
                        .Where(x => x.AddedDate >= reportFilter.TenantAddedStartDateFilter && x.AddedDate <= reportFilter.TenantAddedEndDateFilter)
                        .ToList();
                }

                return tenants;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Tenants: {ex.Message}");
                throw;
            }

        }

        public async Task<List<InvoiceReportDto>> GetInvoicesReportAsync(ReportFilter reportFilter)
        {

            try
            {
                var invoices = await (from i in _db.Invoices
                                  from l in _db.Lease.Where(x => x.LeaseId == i.LeaseId && !x.IsDeleted).DefaultIfEmpty()
                                  from t in _db.Tenant.Where(x => x.TenantId == l.TenantsTenantId).DefaultIfEmpty()
                                  where i.IsDeleted != true && i.AddedBy == reportFilter.AddedBy
                                  select new InvoiceReportDto
                                  {
                                      Invoice = i.InvoiceId.ToString(),
                                      StartDate = l.StartDate,
                                      EndDate = l.EndDate,
                                      Status = l.Status,
                                      PropertyId = (int?)l.AssetId,
                                      Property = l.SelectedProperty,
                                      TenantId = t.TenantId,
                                      Tenant = t.FirstName + "-" + t.LastName,
                                      InvoicePaid = i.InvoicePaid ?? false,
                                      IsPaid = i.InvoicePaid == true ? (int)InvoiceTypes.Paid : (int)InvoiceTypes.UnPaid
                                  })
                     .AsNoTracking()
                     .ToListAsync();

                if (reportFilter.TenantsIds.Count() > 0 && reportFilter.TenantsIds.Any())
                {
                    invoices = invoices
                        .Where(x => reportFilter.TenantsIds.Contains(x.TenantId))
                        .ToList();
                }

                if (reportFilter.InvoicePaid.Count() > 0 && reportFilter.InvoicePaid.Any())
                {
                    invoices = invoices
                        .Where(x => reportFilter.InvoicePaid.Contains(x.IsPaid))
                        .ToList();
                }

                if (reportFilter.PropertiesIds != null && reportFilter.PropertiesIds.Any())
                {
                    invoices = invoices
                        .Where(x => reportFilter.PropertiesIds.Contains(x.PropertyId))
                        .ToList();
                }



                return invoices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Tenants: {ex.Message}");
                throw;
            }

        }
        
        public async Task<List<TenantDependentDto>> GetTenantDependentsAsync(ReportFilter reportFilter)
        {

            try
            {
                var dependents = await (from tdependents in _db.TenantDependent
                                  from t in _db.Tenant.Where(x => x.TenantId == tdependents.TenantId).DefaultIfEmpty()
                                  where tdependents.IsDeleted != true && tdependents.AddedBy == reportFilter.AddedBy
                                  select new TenantDependentDto
                                  {
                                      TenantId = tdependents.TenantId,
                                      TenantName = t.FirstName + " " + t.LastName,
                                      FirstName = tdependents.FirstName,
                                      LastName = tdependents.LastName,
                                      EmailAddress = tdependents.EmailAddress,
                                      PhoneNumber = tdependents.PhoneNumber,
                                      DOB = tdependents.DOB,
                                      Relation = tdependents.Relation,
                                  })
                     .AsNoTracking()
                     .ToListAsync();

                if (reportFilter.TenantsIds.Count() > 0 && reportFilter.TenantsIds.Any())
                {
                    dependents = dependents
                        .Where(x => reportFilter.TenantsIds.Contains(x.TenantId))
                        .ToList();
                }

                return dependents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Tenants: {ex.Message}");
                throw;
            }

        }

        #endregion



        #region Landlord

        public async Task<OwnerDto> GetSingleLandlordByIdAsync(int ownerId)
        {
            var landlordDto = await _db.Owner.FirstOrDefaultAsync(t => t.OwnerId == ownerId && t.IsDeleted != true);

            if (landlordDto == null)
                return new OwnerDto();
            var landlord = new OwnerDto
            {
                OwnerId = landlordDto.OwnerId,
                FirstName = landlordDto.FirstName,
                MiddleName = landlordDto.MiddleName,
                LastName = landlordDto.LastName,
                EmailAddress = landlordDto.EmailAddress,
                EmailAddress2 = landlordDto.EmailAddress2,
                PhoneNumber = landlordDto.PhoneNumber,
                PhoneNumber2 = landlordDto.PhoneNumber2,
                Fax = landlordDto.Fax,
                TaxId = landlordDto.TaxId,
                DocumentName = landlordDto.Document,
                Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Document_" + landlordDto.OwnerId + Path.GetExtension(landlordDto.Document)}",
                EmergencyContactInfo = landlordDto.EmergencyContactInfo,
                LeaseAgreementId = landlordDto.LeaseAgreementId,
                OwnerNationality = landlordDto.OwnerNationality,
                Gender = landlordDto.Gender,
                DOB = landlordDto.DOB,
                VAT = landlordDto.VAT,
                LegalName = landlordDto.LegalName,
                Account_Name = landlordDto.Account_Name,
                Account_Holder = landlordDto.Account_Holder,
                Account_IBAN = landlordDto.Account_IBAN,
                Account_Swift = landlordDto.Account_Swift,
                Account_Bank = landlordDto.Account_Bank,
                Account_Currency = landlordDto.Account_Currency,
                AppTenantId = landlordDto.AppTenantId,
                Address = landlordDto.Address,
                Address2 = landlordDto.Address2,
                Locality = landlordDto.Locality,
                Region = landlordDto.Region,
                PostalCode = landlordDto.PostalCode,
                Country = landlordDto.Country,
                CountryCode = landlordDto.CountryCode,
                PictureName = landlordDto.Picture,
                Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Picture_" + landlordDto.OwnerId + Path.GetExtension(landlordDto.Picture)}",
                AddedBy = landlordDto.AddedBy,
                AddedDate = landlordDto.AddedDate,
            };

            var ownerOrganization = await _db.OwnerOrganization.FirstOrDefaultAsync(o => o.OwnerId == ownerId && o.IsDeleted != true);
            if (ownerOrganization != null)
            {
                landlord.OrganizationName = ownerOrganization.OrganizationName;
                landlord.OrganizationDescription = ownerOrganization.OrganizationDescription;
                landlord.OrganizationIcon = ownerOrganization.OrganizationIcon;
                landlord.OrganizationLogo = ownerOrganization.OrganizationLogo;
                landlord.Website = ownerOrganization.Website;
            }
            else
            {

                landlord.OrganizationName = "";
                landlord.OrganizationDescription = "";
                landlord.OrganizationIcon = "";
                landlord.OrganizationLogo = "";
                landlord.Website = "";
            }

            return landlord;
        }

        public async Task<bool> CreateOwnerAsync(OwnerDto landlordDto)
        {
            var newLandlord = new Owner
            {
                FirstName = landlordDto.FirstName,
                MiddleName = landlordDto.MiddleName,
                LastName = landlordDto.LastName,
                EmailAddress = landlordDto.EmailAddress,
                EmailAddress2 = landlordDto.EmailAddress2,
                PhoneNumber = landlordDto.PhoneNumber,
                PhoneNumber2 = landlordDto.PhoneNumber2,
                Fax = landlordDto.Fax,
                TaxId = landlordDto.TaxId,
                EmergencyContactInfo = landlordDto.EmergencyContactInfo,
                LeaseAgreementId = landlordDto.LeaseAgreementId,
                OwnerNationality = landlordDto.OwnerNationality,
                Gender = landlordDto.Gender,
                DOB = landlordDto.DOB,
                VAT = landlordDto.VAT,
                Status = true,
                LegalName = landlordDto.LegalName,
                Document = landlordDto.DocumentUrl != null ? landlordDto.DocumentUrl.FileName : "",
                Account_Name = landlordDto.Account_Name,
                Account_Holder = landlordDto.Account_Holder,
                Account_IBAN = landlordDto.Account_IBAN,
                Account_Swift = landlordDto.Account_Swift,
                Account_Bank = landlordDto.Account_Bank,
                Account_Currency = landlordDto.Account_Currency,
                AppTenantId = landlordDto.AppTenantId,
                Address = landlordDto.Address,
                Address2 = landlordDto.Address2,
                Locality = landlordDto.Locality,
                Region = landlordDto.Region,
                PostalCode = landlordDto.PostalCode,
                Country = landlordDto.Country,
                CountryCode = landlordDto.CountryCode,
                Picture = landlordDto.PictureUrl != null ? landlordDto.PictureUrl.FileName : "",
                AddedBy = landlordDto.AddedBy,
                AddedDate = DateTime.Now
            };



            await _db.Owner.AddAsync(newLandlord);
            await _db.SaveChangesAsync();

            if (landlordDto.PictureUrl != null)
            {
                var ext = Path.GetExtension(landlordDto.PictureUrl.FileName);
                await _googleCloudStorageService.UploadImageAsync(landlordDto.PictureUrl, "Owner_Picture_" + newLandlord.OwnerId + ext);
            }

            if (landlordDto.DocumentUrl != null)
            {
                var ext = Path.GetExtension(landlordDto.DocumentUrl.FileName);
                await _googleCloudStorageService.UploadImageAsync(landlordDto.DocumentUrl, "Owner_Document_" + newLandlord.OwnerId + ext);
            }


            if (landlordDto.OrganizationName != null)
            {
                var newOwnerOrganization = new OwnerOrganization
                {
                    OwnerId = newLandlord.OwnerId,
                    OrganizationName = landlordDto.OrganizationName,
                    OrganizationDescription = landlordDto.OrganizationDescription,
                    OrganizationIcon = landlordDto.OrganizationIconFile.FileName,
                    OrganizationLogo = landlordDto.OrganizationLogoFile.FileName,
                    Website = landlordDto.Website,
                    AddedBy = landlordDto.AddedBy,
                    AddedDate = DateTime.Now
                };
                await _db.OwnerOrganization.AddAsync(newOwnerOrganization);

                await _db.SaveChangesAsync();

                if (landlordDto.OrganizationIconFile != null)
                {
                    var ext = Path.GetExtension(landlordDto.OrganizationIconFile.FileName);
                    await _googleCloudStorageService.UploadImageAsync(landlordDto.OrganizationIconFile, "Owner_OrganizationIcon_" + newOwnerOrganization.Id + ext);
                }

                if (landlordDto.OrganizationLogoFile != null)
                {
                    var ext = Path.GetExtension(landlordDto.OrganizationLogoFile.FileName);
                    await _googleCloudStorageService.UploadImageAsync(landlordDto.OrganizationLogoFile, "Owner_OrganizationLogo_" + newOwnerOrganization.Id + ext);
                }
            }


            return true;
        }

        public async Task<bool> UpdateOwnerAsync(OwnerDto tenantDto)
        {
            var tenant = await _db.Owner.FirstOrDefaultAsync(t => t.OwnerId == tenantDto.OwnerId && t.IsDeleted != true);
            if (tenant == null) return false;

            var ownerOrganization = await _db.OwnerOrganization.FirstOrDefaultAsync(o => o.OwnerId == tenantDto.OwnerId && o.IsDeleted != true);


            tenant.FirstName = tenantDto.FirstName;
            tenant.MiddleName = tenantDto.MiddleName;
            tenant.LastName = tenantDto.LastName;
            tenant.EmailAddress = tenantDto.EmailAddress;
            tenant.EmailAddress2 = tenantDto.EmailAddress2;
            tenant.PhoneNumber = tenantDto.PhoneNumber;
            tenant.PhoneNumber2 = tenantDto.PhoneNumber2;
            tenant.Fax = tenantDto.Fax;
            tenant.TaxId = tenantDto.TaxId;
            if (tenantDto.DocumentUrl != null)
            {
                tenant.Document = tenantDto.DocumentUrl.FileName;
            }
            tenant.EmergencyContactInfo = tenantDto.EmergencyContactInfo;
            tenant.LeaseAgreementId = tenantDto.LeaseAgreementId;
            tenant.OwnerNationality = tenantDto.OwnerNationality;
            tenant.Gender = tenantDto.Gender;
            tenant.DOB = tenantDto.DOB;
            tenant.VAT = tenantDto.VAT;
            tenant.Status = true;
            tenant.LegalName = tenantDto.LegalName;
            tenant.Account_Name = tenantDto.Account_Name;
            tenant.Account_Holder = tenantDto.Account_Holder;
            tenant.Account_IBAN = tenantDto.Account_IBAN;
            tenant.Account_Swift = tenantDto.Account_Swift;
            tenant.Account_Bank = tenantDto.Account_Bank;
            tenant.Account_Currency = tenantDto.Account_Currency;
            tenant.AppTenantId = tenantDto.AppTenantId;
            tenant.Address = tenantDto.Address;
            tenant.Address2 = tenantDto.Address2;
            tenant.Locality = tenantDto.Locality;
            tenant.Region = tenantDto.Region;
            tenant.PostalCode = tenantDto.PostalCode;
            tenant.Country = tenantDto.Country;
            tenant.CountryCode = tenantDto.CountryCode;
            tenant.ModifiedBy = tenantDto.AddedBy;
            tenant.ModifiedDate = DateTime.Now;
            if (tenantDto.PictureUrl != null)
            {
                tenant.Picture = tenantDto.PictureUrl.FileName;
            }


            if (ownerOrganization != null)
            {
                ownerOrganization.OrganizationName = tenantDto.OrganizationName;
                ownerOrganization.OrganizationDescription = tenantDto.OrganizationDescription;
                ownerOrganization.OrganizationIcon = tenantDto.OrganizationIcon;
                ownerOrganization.OrganizationLogo = tenantDto.OrganizationLogo;
                ownerOrganization.Website = tenantDto.Website;
                ownerOrganization.ModifiedBy = tenantDto.AddedBy;
                ownerOrganization.ModifiedDate = DateTime.Now;

                _db.OwnerOrganization.Update(ownerOrganization);
            }
            else
            {
                ownerOrganization = new OwnerOrganization
                {
                    OwnerId = (int)tenantDto.OwnerId,
                    OrganizationName = tenantDto.OrganizationName,
                    OrganizationDescription = tenantDto.OrganizationDescription,
                    OrganizationIcon = tenantDto.OrganizationIcon,
                    OrganizationLogo = tenantDto.OrganizationLogo,
                    Website = tenantDto.Website,
                    AddedBy = tenantDto.AddedBy,
                    AddedDate = DateTime.Now
                };
                _db.OwnerOrganization.Add(ownerOrganization);
            }

            _db.Owner.Update(tenant);

            var result = await _db.SaveChangesAsync();

            if (tenantDto.PictureUrl != null)
            {
                var ext = Path.GetExtension(tenantDto.PictureUrl.FileName);
                await _googleCloudStorageService.UploadImageAsync(tenantDto.PictureUrl, "Owner_Picture_" + tenant.OwnerId + ext);
            }

            if (tenantDto.DocumentUrl != null)
            {
                var ext = Path.GetExtension(tenantDto.DocumentUrl.FileName);
                await _googleCloudStorageService.UploadImageAsync(tenantDto.DocumentUrl, "Owner_Document_" + tenant.OwnerId + ext);
            }


            return result > 0;
        }

        public async Task<APIResponse> DeleteOwnerAsync(int ownerId)
        {
            var response = new APIResponse();

            try
            {
                var isRefrence = await _db.Assets.AnyAsync(x => x.OwnerId == ownerId && x.IsDeleted != true);
                if (isRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This owner has refrence in asset.");
                    return response;
                }

                var isTaskRefrence = await _db.TaskRequest.AnyAsync(x => x.OwnerId == ownerId && x.IsDeleted != true);
                if (isTaskRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This owner has refrence in asset.");
                    return response;
                }

                var isDocumentsRefrence = await _db.Documents.AnyAsync(x => x.OwnerId == ownerId && x.IsDeleted != true);
                if (isDocumentsRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This owner has refrence in Documents.");
                    return response;
                }

                var owner = await _db.Owner.FirstOrDefaultAsync(t => t.OwnerId == ownerId && t.IsDeleted != true);
                if (owner == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Owner not found.");
                    return response;
                }

                owner.IsDeleted = true;
                _db.Owner.Update(owner);

                var ownerOrganization = await _db.OwnerOrganization.FirstOrDefaultAsync(t => t.OwnerId == ownerId && t.IsDeleted != true);
                if (ownerOrganization != null)
                {
                    ownerOrganization.IsDeleted = true;
                    _db.OwnerOrganization.Update(ownerOrganization);
                }

                await _db.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                response.Result = "Owner deleted successfully.";

            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
            }

            return response;
        }

        public async Task<List<OwnerDto>> GetLandlordOrganizationAsync(ReportFilter reportFilter)
        {
            try
            {
                var organizations = await (from oorg in _db.OwnerOrganization
                                  from o in _db.Owner.Where(x => x.OwnerId == oorg.OwnerId).DefaultIfEmpty()
                                  where oorg.IsDeleted != true && oorg.AddedBy == reportFilter.AddedBy
                                  select new OwnerDto
                                  {
                                      OwnerId = oorg.OwnerId,
                                      FirstName = o.FirstName,
                                      LastName = o.LastName,
                                      OrganizationName = oorg.OrganizationName,
                                      OrganizationDescription = oorg.OrganizationDescription,
                                      OrganizationIcon = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_OrganizationIcon_" + oorg.Id + Path.GetExtension(oorg.OrganizationIcon)}",
                                      OrganizationLogo = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_OrganizationLogo_" + oorg.Id + Path.GetExtension(oorg.OrganizationLogo)}",
                                      Website = oorg.Website,
                                  })
                     .AsNoTracking()
                     .ToListAsync();

                if (reportFilter.LandlordIds.Count() > 0 && reportFilter.LandlordIds.Any())
                {
                    organizations = organizations
                        .Where(x => reportFilter.LandlordIds.Contains(x.OwnerId))
                        .ToList();
                }

                return organizations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Owners: {ex.Message}");
                throw;
            }

        }

        public async Task<List<AssetDTO>> GetLandlordAssetAsync(ReportFilter reportFilter)
        {
            try
            {
                var assets = await (from oasset in _db.Assets
                                      from o in _db.Owner.Where(x => x.OwnerId == oasset.OwnerId).DefaultIfEmpty()
                                      where oasset.IsDeleted != true && oasset.AddedBy == reportFilter.AddedBy
                                      select new AssetDTO
                                      {
                                          AssetId = oasset.AssetId,
                                          OwnerId = o.OwnerId,
                                          OwnerFirstName = o.FirstName,
                                          OwnerLastName = o.LastName,
                                          BuildingNo = oasset.BuildingNo,
                                          BuildingName = oasset.BuildingName,
                                          Country = oasset.Country,
                                          State = oasset.State,
                                          City = oasset.City,
                                          Zipcode = oasset.Zipcode,
                                      })
                     .AsNoTracking()
                     .ToListAsync();

                if (reportFilter.LandlordIds.Count() > 0 && reportFilter.LandlordIds.Any())
                {
                    assets = assets
                        .Where(x => reportFilter.LandlordIds.Contains(x.OwnerId))
                        .ToList();
                }

                return assets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Assets: {ex.Message}");
                throw;
            }

        }

        #endregion

        #region Lease
        public async Task<bool> CreateLeaseAsync(LeaseDto leaseDto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {

                var newLease = new Lease
                {
                    StartDate = leaseDto.StartDate,
                    EndDate = leaseDto.EndDate,
                    IsSigned = leaseDto.IsSigned,
                    SignatureImagePath = leaseDto.SignatureImagePath,
                    IsFixedTerm = leaseDto.IsFixedTerm,
                    SelectedProperty = leaseDto.SelectedProperty,
                    AssetId = leaseDto.AssetId,
                    UnitId = leaseDto.UnitId,
                    SelectedUnit = leaseDto.SelectedUnit,
                    IsMonthToMonth = leaseDto.IsMonthToMonth,
                    HasSecurityDeposit = leaseDto.HasSecurityDeposit,
                    LateFeesPolicy = leaseDto.LateFeesPolicy,
                    TenantsTenantId = leaseDto.TenantId,
                    AppTenantId = leaseDto.AppTenantId,
                    AddedBy = leaseDto.AddedBy,
                    AddedDate = DateTime.Now
                };

                await _db.Lease.AddAsync(newLease);
                await _db.SaveChangesAsync();

                if (leaseDto.RentCharges != null)
                {

                    foreach (var rentChargeDto in leaseDto.RentCharges)
                    {
                        var rentCharge = new RentCharge
                        {
                            LeaseId = newLease.LeaseId,
                            Amount = rentChargeDto.Amount,
                            Description = rentChargeDto.Description,
                            RentDate = rentChargeDto.RentDate,
                            RentPeriod = rentChargeDto.RentPeriod,
                            AddedBy = leaseDto.AddedBy,
                            AddedDate = DateTime.Now
                        };

                        await _db.RentCharge.AddAsync(rentCharge);
                        await _db.SaveChangesAsync();
                    }
                }

                if (leaseDto.SecurityDeposits != null)
                {
                    foreach (var securityDepositDto in leaseDto.SecurityDeposits)
                    {
                        var securityDeposit = new SecurityDeposit
                        {
                            LeaseId = newLease.LeaseId,
                            Amount = securityDepositDto.Amount,
                            Description = securityDepositDto.Description,
                            AddedBy = leaseDto.AddedBy,
                            AddedDate = DateTime.Now
                        };

                        await _db.SecurityDeposit.AddAsync(securityDeposit);
                        await _db.SaveChangesAsync();
                    }
                }

                if (leaseDto.FeeCharge != null)
                {
                    var feeDto = leaseDto.FeeCharge; 
                    var feeCharge = new PMS_PropertyHapa.Models.Entities.FeeCharge
                    {
                        LeaseId = newLease.LeaseId,
                        ChargeLatefeeActive = feeDto.ChargeLatefeeActive,
                        UsePropertyDefaultStructure = feeDto.UsePropertyDefaultStructure,
                        SpecifyLateFeeStructure = feeDto.SpecifyLateFeeStructure,
                        DueDays = feeDto.DueDays,
                        Frequency = feeDto.Frequency,
                        CalculateFee = feeDto.CalculateFee,
                        Amount = feeDto.Amount,
                        ChartAccountId = feeDto.ChartAccountId,
                        Description = feeDto.Description,
                        IsSendARemainder = feeDto.IsSendARemainder,
                        IsNotifyTenants = feeDto.IsNotifyTenants,
                        IsEnableSms = feeDto.IsEnableSms,
                        IsChargeLateFee = feeDto.IsChargeLateFee,
                        IsMonthlyLimit = feeDto.IsMonthlyLimit,
                        IsDailyLimit = feeDto.IsDailyLimit,
                        IsMinimumBalance = feeDto.IsMinimumBalance,
                        IsChargeLateFeeonSpecific = feeDto.IsChargeLateFeeonSpecific,
                        FeeDate = feeDto.FeeDate,
                        AddedBy = leaseDto.AddedBy,
                        AddedDate = DateTime.Now
                    };

                    await _db.FeeCharge.AddAsync(feeCharge);
                    await _db.SaveChangesAsync();
                }


                var maxLeaseId = await _db.Lease.MaxAsync(x => x.LeaseId);
                decimal totalRentAmount = 0;

                if (leaseDto.RentCharges != null)
                {
                    totalRentAmount += leaseDto.RentCharges.Sum(rc => rc.Amount);
                }

                if (leaseDto.SecurityDeposits != null)
                {
                    totalRentAmount += leaseDto.SecurityDeposits.Sum(sd => sd.Amount);
                }

                if (leaseDto.FeeCharge != null)
                {
                    totalRentAmount += leaseDto.FeeCharge.Amount;
                }


                var assest = await _db.Assets.FirstOrDefaultAsync(x => x.AssetId == leaseDto.AssetId && x.IsDeleted != true);

                if (leaseDto.IsMonthToMonth)
                {
                    DateTime invoiceDate = leaseDto.StartDate;
                    while (invoiceDate <= leaseDto.EndDate)
                    {
                        var newInvoice = new Invoice
                        {
                            LeaseId = maxLeaseId,
                            OwnerId = assest.OwnerId,
                            InvoiceCreatedDate = DateTime.Now,
                            TenantId = leaseDto.TenantId,
                            RentAmount = totalRentAmount,
                            AddedBy = leaseDto.AddedBy,
                            AddedDate = DateTime.Now,
                            InvoiceDate = invoiceDate
                        };

                        await _db.Invoices.AddAsync(newInvoice);
                        invoiceDate = invoiceDate.AddMonths(1);
                    }
                }
                else if (leaseDto.IsFixedTerm)
                {
                    var newInvoice = new Invoice
                    {
                        LeaseId = maxLeaseId,
                        OwnerId = assest.OwnerId,
                        InvoiceCreatedDate = DateTime.Now,
                        TenantId = leaseDto.TenantId,
                        RentAmount = totalRentAmount,
                        AddedBy = leaseDto.AddedBy,
                        AddedDate = DateTime.Now,
                        InvoiceDate = leaseDto.StartDate
                    };

                    await _db.Invoices.AddAsync(newInvoice);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<LeaseDto> GetLeaseByIdAsync(int leaseId)
        {
            var lease = await _db.Lease
                .Where(l => l.LeaseId == leaseId && l.IsDeleted != true)
                .Include(l => l.RentCharges)
                .Include(l => l.SecurityDeposit)
                .Include(l => l.FeeCharge)
                 .Include(l => l.Tenants)
                .FirstOrDefaultAsync();

            if (lease == null) return null;

            var leaseDto = new LeaseDto
            {
                LeaseId = lease.LeaseId,
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                IsSigned = lease.IsSigned,
                SelectedProperty = lease.SelectedProperty,
                SelectedUnit = lease.SelectedUnit,
                SignatureImagePath = lease.SignatureImagePath,
                IsFixedTerm = lease.IsFixedTerm,
                IsMonthToMonth = lease.IsMonthToMonth,
                HasSecurityDeposit = lease.HasSecurityDeposit,
                LateFeesPolicy = lease.LateFeesPolicy,
                TenantId = lease.TenantsTenantId,
                AppTenantId = lease.AppTenantId,
                RentCharges = lease.RentCharges.Where(x => x.IsDeleted != true).Select(rc => new RentChargeDto
                {

                    RentChargeId = rc.RentChargeId,
                    Amount = rc.Amount,
                    Description = rc.Description,
                    RentDate = rc.RentDate,
                    RentPeriod = rc.RentPeriod
                }).ToList(),
                FeeCharge = lease.FeeCharge.Where(x => x.IsDeleted != true).Select(rc => new FeeChargeDto
                {
                    FeeChargeId = rc.FeeChargeId,
                    ChargeLatefeeActive = rc.ChargeLatefeeActive,
                    UsePropertyDefaultStructure = rc.UsePropertyDefaultStructure,
                    SpecifyLateFeeStructure = rc.SpecifyLateFeeStructure,
                    DueDays = rc.DueDays,
                    Frequency = rc.Frequency,
                    CalculateFee = rc.CalculateFee,
                    Amount = rc.Amount,
                    ChartAccountId = rc.ChartAccountId,
                    Description = rc.Description,
                    IsSendARemainder = rc.IsSendARemainder,
                    IsNotifyTenants = rc.IsNotifyTenants,
                    IsEnableSms = rc.IsEnableSms,
                    IsChargeLateFee = rc.IsChargeLateFee,
                    IsMonthlyLimit = rc.IsMonthlyLimit,
                    IsDailyLimit = rc.IsDailyLimit,
                    IsMinimumBalance = rc.IsMinimumBalance,
                    IsChargeLateFeeonSpecific = rc.IsChargeLateFeeonSpecific,
                    FeeDate = rc.FeeDate,
                }).FirstOrDefault(),

                SecurityDeposits = lease.SecurityDeposit.Where(x => x.IsDeleted != true).Select(sd => new SecurityDepositDto
                {

                    SecurityDepositId = sd.SecurityDepositId,
                    Amount = sd.Amount,
                    Description = sd.Description
                }).ToList(),
                Tenant = lease.Tenants != null ? new TenantModelDto
                {
                    TenantId = lease.Tenants.TenantId,
                    FirstName = lease.Tenants.FirstName,
                    LastName = lease.Tenants.LastName,
                    EmailAddress = lease.Tenants.EmailAddress,
                    PhoneNumber = lease.Tenants.PhoneNumber,
                } : null
            };

            return leaseDto;
        }

        public async Task<List<LeaseDto>> GetAllLeasesAsync()
        {
            try
            {
                var leases = await _db.Lease
                          .Include(l => l.RentCharges)
                          .Include(l => l.SecurityDeposit)
                          .Include(l => l.FeeCharge)
                          .Include(l => l.Tenants)
                          .Where(l => l.IsDeleted != true)
                          .AsNoTracking()
                          .ToListAsync();

                var leaseDtos = leases.Select(lease => new LeaseDto
                {
                    LeaseId = lease.LeaseId,
                    StartDate = lease.StartDate,
                    EndDate = lease.EndDate,
                    IsSigned = lease.IsSigned,
                    SelectedProperty = lease.SelectedProperty,
                    SelectedUnit = lease.SelectedUnit,
                    SignatureImagePath = lease.SignatureImagePath,
                    IsFixedTerm = lease.IsFixedTerm,
                    IsMonthToMonth = lease.IsMonthToMonth,
                    HasSecurityDeposit = lease.HasSecurityDeposit,
                    LateFeesPolicy = lease.LateFeesPolicy,
                    TenantId = lease.TenantsTenantId,
                    AppTenantId = lease.AppTenantId,
                    AddedBy = lease.AddedBy,
                    RentCharges = lease.RentCharges.Where(x => x.IsDeleted != true).Select(rc => new RentChargeDto
                    {

                        RentChargeId = rc.RentChargeId,
                        Amount = rc.Amount,
                        Description = rc.Description,
                        RentDate = rc.RentDate,
                        RentPeriod = rc.RentPeriod
                    }).ToList() ?? new List<RentChargeDto>(),

                    FeeCharge = lease.FeeCharge.Where(x => x.IsDeleted != true)
                    .Select(rc => new FeeChargeDto
                     {
                         FeeChargeId = rc.FeeChargeId,
                         ChargeLatefeeActive = rc.ChargeLatefeeActive,
                         UsePropertyDefaultStructure = rc.UsePropertyDefaultStructure,
                         SpecifyLateFeeStructure = rc.SpecifyLateFeeStructure,
                         DueDays = rc.DueDays,
                         Frequency = rc.Frequency,
                         CalculateFee = rc.CalculateFee,
                         Amount = rc.Amount,
                         ChartAccountId = rc.ChartAccountId,
                         Description = rc.Description,
                         IsSendARemainder = rc.IsSendARemainder,
                         IsNotifyTenants = rc.IsNotifyTenants,
                         IsEnableSms = rc.IsEnableSms,
                         IsChargeLateFee = rc.IsChargeLateFee,
                         IsMonthlyLimit = rc.IsMonthlyLimit,
                         IsDailyLimit = rc.IsDailyLimit,
                         IsMinimumBalance = rc.IsMinimumBalance,
                         IsChargeLateFeeonSpecific = rc.IsChargeLateFeeonSpecific,
                         FeeDate = rc.FeeDate,
                     }).FirstOrDefault() ?? new FeeChargeDto(),

                    SecurityDeposits = lease.SecurityDeposit.Where(x => x.IsDeleted != true).Select(sd => new SecurityDepositDto
                    {

                        SecurityDepositId = sd.SecurityDepositId,
                        Amount = sd.Amount,
                        Description = sd.Description
                    }).ToList() ?? new List<SecurityDepositDto>(),

                    Tenant = lease.Tenants != null ? new TenantModelDto
                    {
                        TenantId = lease.Tenants.TenantId,
                        AppTenantId = lease.Tenants.AppTenantId,
                        FirstName = lease.Tenants.FirstName,
                        LastName = lease.Tenants.LastName,
                        EmailAddress = lease.Tenants.EmailAddress,
                        PhoneNumber = lease.Tenants.PhoneNumber,
                    } : null
                }).ToList();

                return leaseDtos;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<InvoiceDto>> GetInvoicesByAssetAsync(int assetId)
        {
            try
            {
                var invoices = await (from i in _db.Invoices
                                      from l in _db.Lease.Where(x=>x.LeaseId == i.LeaseId).DefaultIfEmpty()
                                      from t in _db.Tenant.Where(x=>x.TenantId == l.TenantsTenantId).DefaultIfEmpty()
                                      from u in _db.AssetsUnits.Where(x=>x.UnitId == l.UnitId).DefaultIfEmpty()
                                      where l.AssetId == assetId && !i.IsDeleted
                                      select new InvoiceDto
                                      {
                                          InvoiceId = i.InvoiceId,
                                          TenantId = l.TenantsTenantId,
                                          TenantName = t != null ? t.FirstName + " " + t.LastName : null,
                                          InvoiceCreatedDate = i.InvoiceCreatedDate,
                                          InvoicePaid = i.InvoicePaid,
                                          RentAmount = i.RentAmount,
                                          LeaseId = i.LeaseId,
                                          InvoiceDate = i.InvoiceDate,
                                          InvoicePaidToOwner = i.InvoicePaidToOwner,
                                          Unit = u.UnitName,
                                          AddedBy = i.AddedBy
                                      }).AsNoTracking().ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                throw;
            }
        }
        
        public async Task<List<InvoiceDto>> GetInvoicesByTenantAsync(int tenantId)
        {
            try
            {
                var invoices = await (from i in _db.Invoices
                                      from l in _db.Lease.Where(x=>x.LeaseId == i.LeaseId).DefaultIfEmpty()
                                      from t in _db.Tenant.Where(x=>x.TenantId == l.TenantsTenantId).DefaultIfEmpty()
                                      from u in _db.AssetsUnits.Where(x=>x.UnitId == l.UnitId).DefaultIfEmpty()
                                      where l.TenantsTenantId == tenantId && !i.IsDeleted
                                      select new InvoiceDto
                                      {
                                          InvoiceId = i.InvoiceId,
                                          TenantId = l.TenantsTenantId,
                                          TenantName = t != null ? t.FirstName + " " + t.LastName : null,
                                          InvoiceCreatedDate = i.InvoiceCreatedDate,
                                          InvoicePaid = i.InvoicePaid,
                                          RentAmount = i.RentAmount,
                                          LeaseId = i.LeaseId,
                                          InvoiceDate = i.InvoiceDate,
                                          InvoicePaidToOwner = i.InvoicePaidToOwner,
                                          Unit = u.UnitName,
                                          AddedBy = i.AddedBy
                                      }).AsNoTracking().ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                throw;
            }
        }
        
        public async Task<List<InvoiceDto>> GetInvoicesByLandLordAsync(int landlordId)
        {
            try
            {
                var invoices = await (from i in _db.Invoices
                                      from l in _db.Lease.Where(x=>x.LeaseId == i.LeaseId).DefaultIfEmpty()
                                      from t in _db.Tenant.Where(x=>x.TenantId == l.TenantsTenantId).DefaultIfEmpty()
                                      from a in _db.Assets.Where(x=>x.AssetId == l.AssetId).DefaultIfEmpty()
                                      from o in _db.Owner.Where(x=>x.OwnerId == a.OwnerId).DefaultIfEmpty()
                                      from u in _db.AssetsUnits.Where(x=>x.UnitId == l.UnitId).DefaultIfEmpty()
                                      where a.OwnerId == landlordId && !i.IsDeleted
                                      select new InvoiceDto
                                      {
                                          InvoiceId = i.InvoiceId,
                                          TenantId = l.TenantsTenantId,
                                          TenantName = t != null ? t.FirstName + " " + t.LastName : null,
                                          InvoiceCreatedDate = i.InvoiceCreatedDate,
                                          InvoicePaid = i.InvoicePaid,
                                          RentAmount = i.RentAmount,
                                          LeaseId = i.LeaseId,
                                          InvoiceDate = i.InvoiceDate,
                                          InvoicePaidToOwner = i.InvoicePaidToOwner,
                                          OwnerId = o.OwnerId,
                                          OwnerName = o.FirstName + " " + o.LastName,
                                          Asset = a.BuildingNo + "-" + a.BuildingName,
                                          Unit = u.UnitName,
                                          AddedBy = i.AddedBy
                                      }).AsNoTracking().ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                throw;
            }
        }

        public async Task<List<LeaseDto>> GetTenantHistoryByTenantAsync(int tenantId)
        {
            try
            {
                var leases = await (from l in _db.Lease
                                    from a in _db.Assets.Where(x => x.AssetId == l.AssetId).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == l.UnitId).DefaultIfEmpty()
                                    from t in _db.Tenant.Where(x => x.TenantId == l.TenantsTenantId).DefaultIfEmpty()
                                    from i in _db.Invoices.Where(x => x.LeaseId == l.LeaseId).DefaultIfEmpty()
                                    from f in _db.FeeCharge.Where(x => x.LeaseId == l.LeaseId).DefaultIfEmpty()
                                    where l.TenantsTenantId == tenantId && !l.IsDeleted
                                    group new { l, t, u, i, f } by new
                                    {
                                        l.LeaseId,
                                        l.StartDate,
                                        l.EndDate,
                                        l.IsSigned,
                                        l.SignatureImagePath,
                                        l.AssetId,
                                        a.BuildingNo,
                                        a.BuildingName,
                                        l.SelectedProperty,
                                        l.UnitId,
                                        u.UnitName,
                                        l.IsFixedTerm,
                                        l.IsMonthToMonth,
                                        l.HasSecurityDeposit,
                                        l.LateFeesPolicy,
                                        l.AppTenantId,
                                        t.FirstName,
                                        t.LastName,
                                        l.AddedBy
                                    } into g
                                    select new LeaseDto
                                    {
                                        LeaseId = g.Key.LeaseId,
                                        StartDate = g.Key.StartDate,
                                        EndDate = g.Key.EndDate,
                                        IsSigned = g.Key.IsSigned,
                                        SignatureImagePath = g.Key.SignatureImagePath,
                                        AssetId = g.Key.AssetId,
                                        SelectedProperty = g.Key.BuildingNo + "-" + g.Key.BuildingName,
                                        UnitId = g.Key.UnitId,
                                        SelectedUnit = g.Key.UnitName,
                                        IsFixedTerm = g.Key.IsFixedTerm,
                                        IsMonthToMonth = g.Key.IsMonthToMonth,
                                        HasSecurityDeposit = g.Key.HasSecurityDeposit,
                                        LateFeesPolicy = g.Key.LateFeesPolicy,
                                        Tenant = new TenantModelDto
                                        {
                                            FirstName = g.Key.FirstName,
                                            LastName = g.Key.LastName
                                        },
                                        AddedBy = g.Key.AddedBy,
                                        Frequency = g.Key.IsFixedTerm ? "Fixed" : (g.Key.IsMonthToMonth ? "Monthly" : ""),
                                        RentCharges = g.Select(x => new RentChargeDto
                                        {
                                            Amount = x.i != null ? x.i.RentAmount : 0,
                                        }).ToList(),
                                        TotalRentAmount = g.Sum(x => x.i != null ? x.i.RentAmount : 0)
                                    }).AsNoTracking().ToListAsync();

                return leases;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<LeaseDto>> GetTenantHistoryByAssetAsync(int assetId)
        {
            try
            {
                var leases = await (from l in _db.Lease
                                    from a in _db.Assets.Where(x => x.AssetId == l.AssetId).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == l.UnitId).DefaultIfEmpty()
                                    from t in _db.Tenant.Where(x => x.TenantId == l.TenantsTenantId).DefaultIfEmpty()
                                    from i in _db.Invoices.Where(x => x.LeaseId == l.LeaseId).DefaultIfEmpty()
                                    from f in _db.FeeCharge.Where(x => x.LeaseId == l.LeaseId).DefaultIfEmpty()
                                    where l.AssetId == assetId && !l.IsDeleted
                                    group new { l, t, u, i, f } by new
                                    {
                                        l.LeaseId,
                                        l.StartDate,
                                        l.EndDate,
                                        l.IsSigned,
                                        l.SignatureImagePath,
                                        l.AssetId,
                                        a.BuildingNo,
                                        a.BuildingName,
                                        l.SelectedProperty,
                                        l.UnitId,
                                        u.UnitName,
                                        l.IsFixedTerm,
                                        l.IsMonthToMonth,
                                        l.HasSecurityDeposit,
                                        l.LateFeesPolicy,
                                        l.AppTenantId,
                                        t.FirstName,
                                        t.LastName,
                                        l.AddedBy
                                    } into g
                                    select new LeaseDto
                                    {
                                        LeaseId = g.Key.LeaseId,
                                        StartDate = g.Key.StartDate,
                                        EndDate = g.Key.EndDate,
                                        IsSigned = g.Key.IsSigned,
                                        SignatureImagePath = g.Key.SignatureImagePath,
                                        AssetId = g.Key.AssetId,
                                        SelectedProperty = g.Key.BuildingNo + "-" + g.Key.BuildingName,
                                        UnitId = g.Key.UnitId,
                                        SelectedUnit = g.Key.UnitName,
                                        IsFixedTerm = g.Key.IsFixedTerm,
                                        IsMonthToMonth = g.Key.IsMonthToMonth,
                                        HasSecurityDeposit = g.Key.HasSecurityDeposit,
                                        LateFeesPolicy = g.Key.LateFeesPolicy,
                                        Tenant = new TenantModelDto
                                        {
                                            FirstName = g.Key.FirstName,
                                            LastName = g.Key.LastName
                                        },
                                        AddedBy = g.Key.AddedBy,
                                        Frequency = g.Key.IsFixedTerm ? "Fixed" : (g.Key.IsMonthToMonth ? "Monthly" : ""),
                                        RentCharges = g.Select(x => new RentChargeDto
                                        {
                                            Amount = x.i != null ? x.i.RentAmount : 0,
                                        }).ToList(),
                                        TotalRentAmount = g.Sum(x => x.i != null ? x.i.RentAmount : 0)
                                    }).AsNoTracking().ToListAsync();

                return leases;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<bool> UpdateLeaseAsync(LeaseDto leaseDto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var existingLease = await _db.Lease
                    .Include(l => l.RentCharges)
                    .Include(l => l.SecurityDeposit)
                    .Include(l => l.FeeCharge)
                    .FirstOrDefaultAsync(l => l.LeaseId == leaseDto.LeaseId && l.IsDeleted != true);

                if (existingLease == null)
                {
                    return false;
                }

                existingLease.StartDate = leaseDto.StartDate;
                existingLease.EndDate = leaseDto.EndDate;
                existingLease.IsSigned = leaseDto.IsSigned;
                existingLease.SelectedProperty = leaseDto.SelectedProperty;
                //existingLease.AssetId = leaseDto.PropertyId;
                //existingLease.UnitId = leaseDto.UnitId;
                existingLease.SelectedUnit = leaseDto.SelectedUnit;
                existingLease.SignatureImagePath = leaseDto.SignatureImagePath;
                existingLease.IsFixedTerm = leaseDto.IsFixedTerm;
                existingLease.IsMonthToMonth = leaseDto.IsMonthToMonth;
                existingLease.HasSecurityDeposit = leaseDto.HasSecurityDeposit;
                existingLease.LateFeesPolicy = leaseDto.LateFeesPolicy;
                existingLease.TenantsTenantId = leaseDto.TenantId;
                existingLease.AppTenantId = leaseDto.AppTenantId;
                existingLease.ModifiedBy = leaseDto.AddedBy;
                existingLease.ModifiedDate = DateTime.Now;


                if (leaseDto.RentCharges != null)
                {
                    foreach (var rcDto in leaseDto.RentCharges)
                    {
                        var existingRc = existingLease.RentCharges.FirstOrDefault(rc => rc.RentChargeId == rcDto.RentChargeId && rc.IsDeleted != true);
                        if (existingRc != null)
                        {
                            existingRc.Amount = rcDto.Amount;
                            existingRc.Description = rcDto.Description;
                            existingRc.LeaseId = existingLease.LeaseId;
                            existingRc.RentDate = rcDto.RentDate;
                            existingRc.RentPeriod = rcDto.RentPeriod;
                            existingRc.ModifiedBy = leaseDto.AddedBy;
                            existingRc.ModifiedDate = DateTime.Now;
                        }
                        else
                        {

                            existingLease.RentCharges.Add(new RentCharge
                            {
                                Amount = rcDto.Amount,
                                Description = rcDto.Description,
                                LeaseId = existingLease.LeaseId,
                                RentDate = rcDto.RentDate,
                                RentPeriod = rcDto.RentPeriod,
                                AddedBy = leaseDto.AddedBy,
                                AddedDate = DateTime.Now
                            });
                        }
                    }
                }


                if (leaseDto.FeeCharge != null)
                {
                    var rcDto = leaseDto.FeeCharge; 
                    var existingRc = existingLease.FeeCharge.FirstOrDefault(rc => rc.FeeChargeId == rcDto.FeeChargeId && rc.IsDeleted != true);

                    if (existingRc != null)
                    {
                        existingRc.LeaseId = existingLease.LeaseId;
                        existingRc.ChargeLatefeeActive = rcDto.ChargeLatefeeActive;
                        existingRc.UsePropertyDefaultStructure = rcDto.UsePropertyDefaultStructure;
                        existingRc.SpecifyLateFeeStructure = rcDto.SpecifyLateFeeStructure;
                        existingRc.DueDays = rcDto.DueDays;
                        existingRc.Frequency = rcDto.Frequency;
                        existingRc.CalculateFee = rcDto.CalculateFee;
                        existingRc.Amount = rcDto.Amount;
                        existingRc.ChartAccountId = rcDto.ChartAccountId;
                        existingRc.Description = rcDto.Description;
                        existingRc.IsSendARemainder = rcDto.IsSendARemainder;
                        existingRc.IsNotifyTenants = rcDto.IsNotifyTenants;
                        existingRc.IsEnableSms = rcDto.IsEnableSms;
                        existingRc.IsChargeLateFee = rcDto.IsChargeLateFee;
                        existingRc.IsMonthlyLimit = rcDto.IsMonthlyLimit;
                        existingRc.IsDailyLimit = rcDto.IsDailyLimit;
                        existingRc.IsMinimumBalance = rcDto.IsMinimumBalance;
                        existingRc.IsChargeLateFeeonSpecific = rcDto.IsChargeLateFeeonSpecific;
                        existingRc.FeeDate = rcDto.FeeDate;
                        existingRc.ModifiedBy = leaseDto.AddedBy;
                        existingRc.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        existingLease.FeeCharge.Add(new PMS_PropertyHapa.Models.Entities.FeeCharge
                        {
                            LeaseId = existingLease.LeaseId,
                            ChargeLatefeeActive = rcDto.ChargeLatefeeActive,
                            UsePropertyDefaultStructure = rcDto.UsePropertyDefaultStructure,
                            SpecifyLateFeeStructure = rcDto.SpecifyLateFeeStructure,
                            DueDays = rcDto.DueDays,
                            Frequency = rcDto.Frequency,
                            CalculateFee = rcDto.CalculateFee,
                            Amount = rcDto.Amount,
                            ChartAccountId = rcDto.ChartAccountId,
                            Description = rcDto.Description,
                            IsSendARemainder = rcDto.IsSendARemainder,
                            IsNotifyTenants = rcDto.IsNotifyTenants,
                            IsEnableSms = rcDto.IsEnableSms,
                            IsChargeLateFee = rcDto.IsChargeLateFee,
                            IsMonthlyLimit = rcDto.IsMonthlyLimit,
                            IsDailyLimit = rcDto.IsDailyLimit,
                            IsMinimumBalance = rcDto.IsMinimumBalance,
                            IsChargeLateFeeonSpecific = rcDto.IsChargeLateFeeonSpecific,
                            FeeDate = rcDto.FeeDate,
                            AddedBy = leaseDto.AddedBy,
                            AddedDate = DateTime.Now
                        });
                    }
                }


                if (leaseDto.SecurityDeposits != null)
                {
                    foreach (var sdDto in leaseDto.SecurityDeposits)
                    {
                        var existingSd = existingLease.SecurityDeposit.FirstOrDefault(sd => sd.SecurityDepositId == sdDto.SecurityDepositId && sd.IsDeleted != true);
                        if (existingSd != null)
                        {

                            existingSd.Amount = sdDto.Amount;
                            existingSd.Description = sdDto.Description;
                            existingSd.LeaseId = existingLease.LeaseId;
                            existingSd.ModifiedBy = leaseDto.AddedBy;
                            existingSd.ModifiedDate = DateTime.Now;
                        }
                        else
                        {

                            existingLease.SecurityDeposit.Add(new SecurityDeposit
                            {
                                Amount = sdDto.Amount,
                                Description = sdDto.Description,
                                LeaseId = existingLease.LeaseId,
                                AddedBy = leaseDto.AddedBy,
                                AddedDate = DateTime.Now
                            });
                        }
                    }
                }

                var assest = await _db.Assets.FirstOrDefaultAsync(x => x.AssetId == existingLease.AssetId && x.IsDeleted != true);
                var oldInvoices = await _db.Invoices.Where(x => x.LeaseId == leaseDto.LeaseId && x.IsDeleted != true).ToListAsync();

                oldInvoices.ForEach(x => x.IsDeleted = true);
                _db.Invoices.UpdateRange(oldInvoices);

                decimal totalRentAmount = 0;

                if (leaseDto.RentCharges != null)
                {
                    totalRentAmount += leaseDto.RentCharges.Sum(rc => rc.Amount);
                }

                if (leaseDto.SecurityDeposits != null)
                {
                    totalRentAmount += leaseDto.SecurityDeposits.Sum(sd => sd.Amount);
                }

                if (leaseDto.FeeCharge != null)
                {
                    totalRentAmount += leaseDto.FeeCharge.Amount;
                }

                if (leaseDto.IsMonthToMonth)
                {
                    DateTime invoiceDate = leaseDto.StartDate;
                    while (invoiceDate <= leaseDto.EndDate)
                    {
                        var newInvoice = new Invoice
                        {
                            LeaseId = leaseDto.LeaseId,
                            OwnerId = assest.OwnerId,
                            InvoiceCreatedDate = DateTime.Now,
                            TenantId = leaseDto.TenantId,
                            RentAmount = totalRentAmount,
                            AddedBy = leaseDto.AddedBy,
                            AddedDate = DateTime.Now,
                            InvoiceDate = invoiceDate
                        };

                        await _db.Invoices.AddAsync(newInvoice);
                        invoiceDate = invoiceDate.AddMonths(1);
                    }
                }
                else if (leaseDto.IsFixedTerm)
                {
                    var newInvoice = new Invoice
                    {
                        LeaseId = leaseDto.LeaseId,
                        OwnerId = assest.OwnerId,
                        InvoiceCreatedDate = DateTime.Now,
                        TenantId = leaseDto.TenantId,
                        RentAmount = totalRentAmount,
                        AddedBy = leaseDto.AddedBy,
                        AddedDate = DateTime.Now,
                        InvoiceDate = leaseDto.StartDate
                    };

                    await _db.Invoices.AddAsync(newInvoice);
                }


                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<APIResponse> DeleteLeaseAsync(int leaseId)
        {
            var response = new APIResponse();
            try
            {
                var lease = await _db.Lease.FirstOrDefaultAsync(x => x.LeaseId == leaseId && x.IsDeleted != true);
                if (lease == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Lease not found.");
                    return response;
                }

                lease.IsDeleted = true;
                _db.Lease.Update(lease);

                await _db.SaveChangesAsync();

                var rentCharges = await _db.RentCharge.Where(x => x.LeaseId == leaseId && x.IsDeleted != true).ToListAsync();
                rentCharges.ForEach(x => x.IsDeleted = true);
                _db.RentCharge.UpdateRange(rentCharges);

                var feeCharges = await _db.FeeCharge.Where(x => x.LeaseId == leaseId && x.IsDeleted != true).ToListAsync();
                feeCharges.ForEach(x => x.IsDeleted = true);
                _db.FeeCharge.UpdateRange(feeCharges);

                var securityDeposits = await _db.SecurityDeposit.Where(x => x.LeaseId == leaseId && x.IsDeleted != true).ToListAsync();
                securityDeposits.ForEach(x => x.IsDeleted = true);
                _db.SecurityDeposit.UpdateRange(securityDeposits);

                await _db.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                response.Result = "Lease and associated charges deleted successfully.";

            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
            }

            return response;
        }



        //Inovices

        public async Task<List<InvoiceDto>> GetInvoicesAsync(int leaseId)
        {
            var invoices = await (from i in _db.Invoices
                                  from t in _db.Tenant.Where(x => x.TenantId == i.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                  from o in _db.Owner.Where(x => x.OwnerId == i.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                  where i.LeaseId == leaseId && i.IsDeleted != true
                                  select new InvoiceDto
                                  {
                                      InvoiceId = i.InvoiceId,
                                      OwnerId = i.OwnerId,
                                      OwnerName = o.FirstName + " " + o.LastName,
                                      TenantId = i.TenantId,
                                      TenantName = t.FirstName + " " + t.LastName,
                                      InvoiceCreatedDate = i.InvoiceCreatedDate,
                                      InvoicePaid = i.InvoicePaid,
                                      RentAmount = i.RentAmount,
                                      LeaseId = i.LeaseId,
                                      InvoiceDate = i.InvoiceDate,
                                      InvoicePaidToOwner = i.InvoicePaidToOwner,
                                      AddedBy = i.AddedBy,
                                  }).ToListAsync();

            return invoices;
        }

        public async Task<List<InvoiceDto>> GetAllInvoicesAsync()
        {
            try
            {
                var invoices = await (from i in _db.Invoices
                                      from t in _db.Tenant.Where(x => x.TenantId == i.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                      from o in _db.Owner.Where(x => x.OwnerId == i.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                      where i.IsDeleted != true // Remove leaseId filter
                                      select new InvoiceDto
                                      {
                                          InvoiceId = i.InvoiceId,
                                          OwnerId = i.OwnerId,
                                          OwnerName = o.FirstName + " " + o.LastName,
                                          TenantId = i.TenantId,
                                          TenantName = t.FirstName + " " + t.LastName,
                                          InvoiceCreatedDate = i.InvoiceCreatedDate,
                                          InvoicePaid = i.InvoicePaid,
                                          RentAmount = i.RentAmount,
                                          LeaseId = i.LeaseId,
                                          InvoiceDate = i.InvoiceDate,
                                          InvoicePaidToOwner = i.InvoicePaidToOwner,
                                          AddedBy = i.AddedBy,
                                      }).ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve invoices", ex);
            }
        }

        public async Task<InvoiceDto> GetInvoiceByIdAsync(int invoiceId)
        {
            var invoice = await (from i in _db.Invoices
                                 from t in _db.Tenant.Where(x => x.TenantId == i.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                 from o in _db.Owner.Where(x => x.OwnerId == i.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                 where i.InvoiceId == invoiceId && i.IsDeleted != true
                                 select new InvoiceDto
                                 {
                                     InvoiceId = i.InvoiceId,
                                     OwnerId = i.OwnerId,
                                     OwnerName = o.FirstName + " " + o.LastName,
                                     TenantId = i.TenantId,
                                     TenantName = t.FirstName + " " + t.LastName,
                                     InvoiceCreatedDate = i.InvoiceCreatedDate,
                                     InvoicePaid = i.InvoicePaid,
                                     RentAmount = i.RentAmount,
                                     LeaseId = i.LeaseId,
                                     InvoiceDate = i.InvoiceDate,
                                     InvoicePaidToOwner = i.InvoicePaidToOwner,
                                     AddedBy = i.AddedBy,
                                 }).FirstOrDefaultAsync();
            return invoice;
        }

        public async Task<bool> AllInvoicePaidAsync(int leaseId)
        {
            var invoices = await _db.Invoices.Where(t => t.LeaseId == leaseId && t.InvoicePaid != true && t.IsDeleted != true).ToListAsync();

            invoices.ForEach(x => x.InvoicePaid = true);
            _db.Invoices.UpdateRange(invoices);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> AllInvoiceOwnerPaidAsync(int leaseId)
        {
            var invoices = await _db.Invoices.Where(t => t.LeaseId == leaseId && t.InvoicePaidToOwner != true && t.IsDeleted != true).ToListAsync();

            invoices.ForEach(x => x.InvoicePaidToOwner = true);
            _db.Invoices.UpdateRange(invoices);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> InvoicePaidAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FirstOrDefaultAsync(t => t.InvoiceId == invoiceId && t.IsDeleted != true);

            invoice.InvoicePaid = true;
            _db.Invoices.Update(invoice);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> InvoiceOwnerPaidAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FirstOrDefaultAsync(t => t.InvoiceId == invoiceId && t.IsDeleted != true);

            invoice.InvoicePaidToOwner = true;
            _db.Invoices.Update(invoice);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        


        #endregion

        #region TenantOrg

        public async Task<TenantOrganizationInfoDto> GetTenantOrgByIdAsync(int tenantId)
        {
            var tenant = await _db.TenantOrganizationInfo.FirstOrDefaultAsync(t => t.Id == tenantId && t.IsDeleted != true);

            if (tenant == null)
                return new TenantOrganizationInfoDto();

            var tenantDto = new TenantOrganizationInfoDto
            {
                Id = tenant.Id,
                TenantUserId = tenant.TenantUserId,
                OrganizationName = tenant.OrganizationName,
                OrganizationDescription = tenant.OrganizationDescription,
                OrganizationIconName = tenant.OrganizationIcon,
                OrganizationIcon = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"TenantOrganizationInfo_OrganizationIcon_" + tenant.Id + Path.GetExtension(tenant.OrganizationIcon)}",
                OrganizationLogoName = tenant.OrganizationLogo,
                OrganizationLogo = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"TenantOrganizationInfo_OrganizationLogo_" + tenant.Id + Path.GetExtension(tenant.OrganizationLogo)}",
                OrganizatioPrimaryColor = tenant.OrganizatioPrimaryColor,
                OrganizationSecondColor = tenant.OrganizationSecondColor,
            };

            return tenantDto;
        }

        public async Task<bool> UpdateTenantOrgAsync(TenantOrganizationInfoDto tenantDto)
        {
            if (tenantDto.Id < 0) return false;

            var newTenant = _db.TenantOrganizationInfo.FirstOrDefault(x => x.TenantUserId == tenantDto.TenantUserId && x.IsDeleted != true);
            if (newTenant == null)
                newTenant = new TenantOrganizationInfo();


            newTenant.Id = tenantDto.Id;
            newTenant.TenantUserId = tenantDto.TenantUserId;
            newTenant.OrganizationName = tenantDto.OrganizationName;
            newTenant.OrganizationDescription = tenantDto.OrganizationDescription;
            if (tenantDto.OrganizationIconName != null)
            {
                newTenant.OrganizationIcon = tenantDto.OrganizationIconName;
            }
            if (tenantDto.OrganizationLogoName != null)
            {
                newTenant.OrganizationLogo = tenantDto.OrganizationLogoName;
            }
            newTenant.OrganizatioPrimaryColor = tenantDto.OrganizatioPrimaryColor;
            newTenant.OrganizationSecondColor = tenantDto.OrganizationSecondColor;

            if (tenantDto.Id > 0)
                _db.TenantOrganizationInfo.Update(newTenant);

            else
                _db.TenantOrganizationInfo.Add(newTenant);


            var result = await _db.SaveChangesAsync();

            if (tenantDto.OrganizationIcon != null)
            {
                var ext = Path.GetExtension(tenantDto.OrganizationIconName);
                await _googleCloudStorageService.UploadImagebyBase64Async(tenantDto.OrganizationIcon, "TenantOrganizationInfo_OrganizationIcon_" + newTenant.Id + ext);
            }

            if (tenantDto.OrganizationLogo != null)
            {
                var ext = Path.GetExtension(tenantDto.OrganizationLogoName);
                await _googleCloudStorageService.UploadImagebyBase64Async(tenantDto.OrganizationLogo, "TenantOrganizationInfo_OrganizationLogo_" + newTenant.Id + ext);
            }
            return result > 0;
        }

        #endregion

        #region Assets

        public async Task<List<AssetDTO>> GetAllAssetsAsync()
        {
            try
            {

                var assets = await (from asset in _db.Assets
                                    from owner in _db.Owner.Where(x => x.OwnerId == asset.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    where asset.IsDeleted != true
                                    select new AssetDTO
                                    {
                                        AssetId = asset.AssetId,
                                        SelectedPropertyType = asset.SelectedPropertyType,
                                        SelectedBankAccountOption = asset.SelectedBankAccountOption,
                                        SelectedReserveFundsOption = asset.SelectedReserveFundsOption,
                                        SelectedSubtype = asset.SelectedSubtype,
                                        SelectedOwnershipOption = asset.SelectedOwnershipOption,
                                        BuildingNo = asset.BuildingNo,
                                        BuildingName = asset.BuildingName,
                                        Street1 = asset.Street1,
                                        Street2 = asset.Street2,
                                        City = asset.City,
                                        Country = asset.Country,
                                        Zipcode = asset.Zipcode,
                                        State = asset.State,
                                        AppTid = asset.AppTenantId,
                                        PictureFileName = asset.Image,
                                        Image = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Assets_Image_" + asset.AssetId + Path.GetExtension(asset.Image)}",
                                        AddedBy = asset.AddedBy,
                                        AddedDate = asset.AddedDate,
                                        ModifiedDate = asset.ModifiedDate,
                                        Units = asset.Units.Where(x => x.IsDeleted != true).Select(u => new UnitDTO
                                        {
                                            UnitId = u.UnitId,
                                            AssetId = u.AssetId,
                                            UnitName = u.UnitName,
                                            Bath = u.Bath,
                                            Beds = u.Beds,
                                            Rent = u.Rent,
                                            Size = u.Size,
                                        }).ToList(),
                                        OwnerData = new OwnerDto
                                        {
                                            OwnerId = owner.OwnerId,
                                            FirstName = owner.FirstName,
                                            MiddleName = owner.MiddleName,
                                            LastName = owner.LastName,
                                            Fax = owner.Fax,
                                            TaxId = owner.TaxId,
                                            EmailAddress = owner.EmailAddress,
                                            EmailAddress2 = owner.EmailAddress2,
                                            PictureName = owner.Picture,
                                            Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Picture_" + owner.OwnerId + Path.GetExtension(owner.Picture)}",
                                            PhoneNumber = owner.PhoneNumber,
                                            PhoneNumber2 = owner.PhoneNumber2,
                                            EmergencyContactInfo = owner.EmergencyContactInfo,
                                            LeaseAgreementId = owner.LeaseAgreementId,
                                            OwnerNationality = owner.OwnerNationality,
                                            Gender = owner.Gender,
                                            DocumentName = owner.Document,
                                            Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Document_" + owner.OwnerId + Path.GetExtension(owner.Document)}",
                                            DOB = owner.DOB,
                                            VAT = owner.VAT,
                                            LegalName = owner.LegalName,
                                            Account_Name = owner.Account_Name,
                                            Account_Holder = owner.Account_Holder,
                                            Account_IBAN = owner.Account_IBAN,
                                            Account_Swift = owner.Account_Swift,
                                            Account_Bank = owner.Account_Bank,
                                            Account_Currency = owner.Account_Currency,
                                            AddedBy = owner.AddedBy,
                                            AddedDate = owner.AddedDate,
                                            ModifiedDate = owner.ModifiedDate
                                        }
                                    }).AsNoTracking()
                                        .ToListAsync();

                return assets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<AssetDTO> GetAssetByIdAsync(int assetId)
        {
            try
            {

                var asset = await (from a in _db.Assets
                                   from o in _db.Owner.Where(x => x.OwnerId == a.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                   where a.AssetId == assetId && a.IsDeleted != true
                                   select new AssetDTO
                                   {
                                       AssetId = a.AssetId,
                                       OwnerId = a.OwnerId,
                                       SelectedPropertyType = a.SelectedPropertyType,
                                       SelectedBankAccountOption = a.SelectedBankAccountOption,
                                       SelectedReserveFundsOption = a.SelectedReserveFundsOption,
                                       SelectedSubtype = a.SelectedSubtype,
                                       SelectedOwnershipOption = a.SelectedOwnershipOption,
                                       BuildingNo = a.BuildingNo,
                                       BuildingName = a.BuildingName,
                                       Street1 = a.Street1,
                                       Street2 = a.Street2,
                                       City = a.City,
                                       Country = a.Country,
                                       Zipcode = a.Zipcode,
                                       State = a.State,
                                       AppTid = a.AppTenantId,
                                       PictureFileName = a.Image,
                                       Image = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Assets_Image_" + a.AssetId + Path.GetExtension(a.Image)}",
                                       AddedBy = a.AddedBy,
                                       AddedDate = a.AddedDate,
                                       ModifiedDate = a.ModifiedDate,
                                       Units = a.Units.Where(x => x.IsDeleted != true).Select(u => new UnitDTO
                                       {
                                           UnitId = u.UnitId,
                                           AssetId = u.AssetId,
                                           UnitName = u.UnitName,
                                           Bath = u.Bath,
                                           Beds = u.Beds,
                                           Rent = u.Rent,
                                           Size = u.Size,
                                       }).ToList(),
                                       OwnerData = new OwnerDto
                                       {
                                           OwnerId = o.OwnerId,
                                           FirstName = o.FirstName,
                                           MiddleName = o.MiddleName,
                                           LastName = o.LastName,
                                           Fax = o.Fax,
                                           TaxId = o.TaxId,
                                           EmailAddress = o.EmailAddress,
                                           EmailAddress2 = o.EmailAddress2,
                                           PictureName = o.Picture,
                                           Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Picture_" + o.OwnerId + Path.GetExtension(o.Picture)}",
                                           PhoneNumber = o.PhoneNumber,
                                           PhoneNumber2 = o.PhoneNumber2,
                                           EmergencyContactInfo = o.EmergencyContactInfo,
                                           LeaseAgreementId = o.LeaseAgreementId,
                                           OwnerNationality = o.OwnerNationality,
                                           Gender = o.Gender,
                                           DocumentName = o.Document,
                                           Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Document_" + o.OwnerId + Path.GetExtension(o.Document)}",
                                           DOB = o.DOB,
                                           VAT = o.VAT,
                                           LegalName = o.LegalName,
                                           Account_Name = o.Account_Name,
                                           Account_Holder = o.Account_Holder,
                                           Account_IBAN = o.Account_IBAN,
                                           Account_Swift = o.Account_Swift,
                                           Account_Bank = o.Account_Bank,
                                           Account_Currency = o.Account_Currency,
                                           AddedBy = o.AddedBy,
                                           AddedDate = o.AddedDate,
                                           ModifiedDate = o.ModifiedDate
                                       }
                                   }).AsNoTracking()
                                        .FirstOrDefaultAsync();

                return asset;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AssetDTO>> GetAssetsDllAsync(Filter filter)
        {
            try
            {

                var assetsQuery = _db.Assets
                .Where(x => x.IsDeleted != true)
                .Select(a => new AssetDTO
                {
                    AssetId = a.AssetId,
                    BuildingNo = a.BuildingNo,
                    BuildingName = a.BuildingName,
                    AddedBy = a.AddedBy,
                });

                if (!string.IsNullOrEmpty(filter.AddedBy))
                {
                    assetsQuery = assetsQuery.Where(x => x.AddedBy == filter.AddedBy);
                }

                var assets = await assetsQuery.ToListAsync();

                return assets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AssetUnitDTO>> GetAllUnitsAsync()
        {
            try
            {
                var units = await _db.AssetsUnits
                                             .Where(x => x.IsDeleted != true)
                                             .AsNoTracking()
                                             .ToListAsync();



                var Unit = units.Select(tenant => new AssetUnitDTO
                {
                    AssetId = tenant.AssetId,
                    UnitId = tenant.UnitId,
                    UnitName = tenant.UnitName,
                    Bath = tenant.Bath,
                    Beds = tenant.Beds,
                    Rent = tenant.Rent,
                    Size = tenant.Size,
                }).ToList();


                return Unit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping units: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AssetUnitDTO>> GetUnitsDllAsync(Filter filter)
        {
            try
            {
                var units = await (from au in _db.AssetsUnits
                                   where au.IsDeleted != true
                                   select new AssetUnitDTO
                                   {
                                       UnitId = au.UnitId,
                                       UnitName = au.UnitName,
                                       AssetId = au.AssetId,
                                   }).AsNoTracking().ToListAsync();

                if (filter.AssetIds.Count() > 0)
                {
                    units = units.Where(x => filter.AssetIds.Contains(x.AssetId)).ToList();
                }

                return units;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping units: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AssetUnitDTO>> GetUnitsByUserAsync(Filter filter)
        {
            try
            {
                var units = await (from a in _db.Assets
                                   from au in _db.AssetsUnits.Where(x => x.AssetId == a.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                   where a.AddedBy == filter.AddedBy && a.IsDeleted != true
                                   select new AssetUnitDTO
                                   {
                                       UnitId = au.UnitId,
                                       UnitName = au.UnitName,
                                       AssetId = au.AssetId,
                                   }).AsNoTracking().ToListAsync();

                return units;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping units: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UnitDTO>> GetUnitsDetailAsync(int assetId)
        {
            try
            {
                var result = await (from au in _db.AssetsUnits
                                    where au.AssetId == assetId && au.IsDeleted != true
                                    select new UnitDTO
                                    {
                                        UnitId = au.UnitId,
                                        AssetId = au.AssetId,
                                        UnitName = au.UnitName,
                                        Beds = au.Beds,
                                        Bath = au.Bath,
                                        Size = au.Size,
                                        Rent = au.Rent,


                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping unit details: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateAssetAsync(AssetDTO assetDTO)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == assetDTO.AddedBy && x.IsDeleted != true);
            if (user != null)
            {
                var subscription = await _db.Subscriptions.FirstOrDefaultAsync(x => x.Id == user.SubscriptionId && x.IsDeleted != true);

                if (subscription != null && subscription.SubscriptionName == SubscriptionTypes.Free.ToString())
                {
                    var leaseCount = await _db.Assets
                        .Where(x => x.AddedBy == assetDTO.AddedBy && x.IsDeleted != true)
                        .AsNoTracking()
                        .CountAsync();

                    if (leaseCount >= 5)
                    {
                        return false;
                    }
                }

            }

            if (assetDTO.SelectedOwnershipOption == "owned-by-me")
            {

                var existingOwner = await _db.Owner.FirstOrDefaultAsync(o => o.EmailAddress == user.Email && o.IsDeleted != true);
                bool isValidGuid = Guid.TryParse(user.Id, out Guid userIdGuid);
                var tenantOrganization = await _db.TenantOrganizationInfo.AsNoTracking().FirstOrDefaultAsync(to => to.TenantUserId == userIdGuid && to.IsDeleted != true);

                if (existingOwner == null)
                {
                    existingOwner = new Owner
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        EmailAddress = user.Email,
                        Address = user.Address,
                        District = user.District,
                        Region = user.Region,
                        Country = user.Country,
                        PostalCode = user.PostalCode,
                        AppTenantId = Guid.Parse(assetDTO.AppTid),
                        Picture = tenantOrganization.OrganizationLogo,
                        AddedDate = DateTime.Now,
                        AddedBy = assetDTO.AddedBy

                    };

                    await _db.Owner.AddAsync(existingOwner);
                    await _db.SaveChangesAsync();

                    if (existingOwner.Picture != null)
                    {
                        var ext = Path.GetExtension(existingOwner.Picture);
                        var picture = await _googleCloudStorageService.GetGoogleImageAsync("TenantOrganizationInfo_OrganizationLogo_" + tenantOrganization.Id + ext);
                        if (picture != null)
                        {
                            await _googleCloudStorageService.UploadImageAsync(picture, "Owner_Picture_" + existingOwner.OwnerId + ext);
                        }
                    }

                }

                assetDTO.OwnerId = existingOwner.OwnerId;
            }

            var newAsset = new Assets
            {
                OwnerId = assetDTO.OwnerId,
                SelectedPropertyType = assetDTO.SelectedPropertyType,
                SelectedBankAccountOption = assetDTO.SelectedBankAccountOption,
                SelectedReserveFundsOption = assetDTO.SelectedReserveFundsOption,
                SelectedSubtype = assetDTO.SelectedSubtype,
                SelectedOwnershipOption = assetDTO.SelectedOwnershipOption,
                BuildingNo = assetDTO.BuildingNo,
                BuildingName = assetDTO.BuildingName,
                Street1 = assetDTO.Street1,
                Street2 = assetDTO.Street2,
                City = assetDTO.City,
                Country = assetDTO.Country,
                Zipcode = assetDTO.Zipcode,
                State = assetDTO.State,
                Image = assetDTO.PictureFileName,
                AppTenantId = assetDTO.AppTid,
                AddedBy = assetDTO.AddedBy,
                AddedDate = DateTime.Now,
                Units = new List<AssetsUnits>()
            };

            foreach (var u in assetDTO.Units)
            {
                var unit = new AssetsUnits
                {
                    UnitName = u.UnitName,
                    Beds = u.Beds,
                    Bath = u.Bath,
                    Size = u.Size,
                    Rent = u.Rent,
                    AddedBy = assetDTO.AddedBy,
                    AddedDate = DateTime.Now
                };
                newAsset.Units.Add(unit);
            }
            await _db.Assets.AddAsync(newAsset);
            var result = await _db.SaveChangesAsync();

            if (assetDTO.Image != null)
            {
                var ext = Path.GetExtension(assetDTO.PictureFileName);
                await _googleCloudStorageService.UploadImagebyBase64Async(assetDTO.Image, "Assets_Image_" + newAsset.AssetId + ext);
            }

            return result > 0;
        }

        public async Task<bool> UpdateAssetAsync(AssetDTO assetDTO)
        {

            if (assetDTO.SelectedOwnershipOption == "owned-by-me")
            {
                var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == assetDTO.AddedBy && x.IsDeleted != true);
                var existingOwner = await _db.Owner.FirstOrDefaultAsync(o => o.EmailAddress == user.Email && o.IsDeleted != true);
                if (existingOwner == null)
                {
                    bool isValidGuid = Guid.TryParse(user.Id, out Guid userIdGuid);
                    var tenantOrganization = await _db.TenantOrganizationInfo.AsNoTracking().FirstOrDefaultAsync(to => to.TenantUserId == userIdGuid);
                    if (existingOwner == null)
                    {
                        existingOwner = new Owner
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            EmailAddress = user.Email,
                            Address = user.Address,
                            District = user.District,
                            Region = user.Region,
                            Country = user.Country,
                            PostalCode = user.PostalCode,
                            AppTenantId = Guid.Parse(assetDTO.AppTid),
                            Picture = tenantOrganization.OrganizationLogo,
                            AddedDate = DateTime.Now,
                            AddedBy = assetDTO.AddedBy

                        };
                        await _db.Owner.AddAsync(existingOwner);
                        await _db.SaveChangesAsync();
                    }

                    if (existingOwner.Picture != null)
                    {
                        var ext = Path.GetExtension(existingOwner.Picture);
                        var picture = await _googleCloudStorageService.GetGoogleImageAsync("TenantOrganizationInfo_OrganizationLogo_" + tenantOrganization.Id + ext);
                        if (picture != null)
                        {
                            await _googleCloudStorageService.UploadImageAsync(picture, "Owner_Picture_" + existingOwner.OwnerId + ext);
                        }
                    }
                }

                assetDTO.OwnerId = existingOwner.OwnerId;
            }


            var existingAsset = await _db.Assets.FindAsync(assetDTO.AssetId);
            if (existingAsset == null)
            {
                return false;
            }

            existingAsset.OwnerId = assetDTO.OwnerId;
            existingAsset.SelectedPropertyType = assetDTO.SelectedPropertyType;
            existingAsset.SelectedBankAccountOption = assetDTO.SelectedBankAccountOption;
            existingAsset.SelectedReserveFundsOption = assetDTO.SelectedReserveFundsOption;
            existingAsset.SelectedSubtype = assetDTO.SelectedSubtype;
            existingAsset.SelectedOwnershipOption = assetDTO.SelectedOwnershipOption;
            existingAsset.BuildingNo = assetDTO.BuildingNo;
            existingAsset.BuildingName = assetDTO.BuildingName;
            existingAsset.Street1 = assetDTO.Street1;
            existingAsset.Street2 = assetDTO.Street2;
            existingAsset.City = assetDTO.City;
            existingAsset.Country = assetDTO.Country;
            existingAsset.Zipcode = assetDTO.Zipcode;
            existingAsset.State = assetDTO.State;
            existingAsset.ModifiedBy = assetDTO.AddedBy;
            existingAsset.ModifiedDate = DateTime.Now;
            if (assetDTO.PictureFileName != null)
            {
                existingAsset.Image = assetDTO.PictureFileName;
            }
            existingAsset.AppTenantId = assetDTO.AppTid;

            var existingUnits = _db.AssetsUnits.Where(u => u.AssetId == assetDTO.AssetId && u.IsDeleted != true).ToList();
            var incomingUnitIds = assetDTO.Units.Select(u => u.UnitId).ToHashSet();

            // Update existing units and add new units
            foreach (var unitDTO in assetDTO.Units)
            {
                var existingUnit = existingUnits.FirstOrDefault(u => u.UnitId == unitDTO.UnitId && u.IsDeleted != true);
                if (existingUnit != null)
                {
                    existingUnit.AssetId = assetDTO.AssetId;
                    existingUnit.UnitName = unitDTO.UnitName;
                    existingUnit.Beds = unitDTO.Beds;
                    existingUnit.Bath = unitDTO.Bath;
                    existingUnit.Size = unitDTO.Size;
                    existingUnit.Rent = unitDTO.Rent;
                    existingUnit.ModifiedBy = assetDTO.AddedBy;
                    existingUnit.ModifiedDate = DateTime.Now;

                    _db.AssetsUnits.Update(existingUnit);
                }
                else
                {
                    var newUnit = new AssetsUnits
                    {
                        AssetId = assetDTO.AssetId,
                        UnitName = unitDTO.UnitName,
                        Beds = unitDTO.Beds,
                        Bath = unitDTO.Bath,
                        Size = unitDTO.Size,
                        Rent = unitDTO.Rent,
                        AddedBy = assetDTO.AddedBy,
                        AddedDate = DateTime.Now
                    };
                    _db.AssetsUnits.Add(newUnit);
                }
            }

            // Remove units that are no longer in the DTO
            var unitsToRemove = existingUnits.Where(u => !incomingUnitIds.Contains(u.UnitId) && u.IsDeleted != true).ToList();

            if (unitsToRemove.Any())
            {
                unitsToRemove.ForEach(x => x.IsDeleted = true);
            }
            var result = await _db.SaveChangesAsync();

            if (assetDTO.Image != null)
            {
                var ext = Path.GetExtension(assetDTO.PictureFileName);
                var res = await _googleCloudStorageService.UploadImagebyBase64Async(assetDTO.Image, "Assets_Image_" + existingAsset.AssetId + ext);
            }

            return result > 0;
        }

        public async Task<APIResponse> DeleteAssetAsync(int assetId)
        {
            var response = new APIResponse();

            try
            {
                var isTaskRefrence = await _db.TaskRequest.AnyAsync(x => x.AssetId == assetId && x.IsDeleted != true);
                if (isTaskRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Asset has refrence in TaskRequest.");
                    return response;
                }

                var isLeaseRefrence = await _db.Lease.AnyAsync(x => x.AssetId == assetId && x.IsDeleted != true);
                if (isLeaseRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Asset has refrence in Lease.");
                    return response;
                }

                var isBudgetRefrence = await _db.Budgets.AnyAsync(x => x.PropertyId == assetId && x.IsDeleted != true);
                if (isBudgetRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Asset has refrence in Lease.");
                    return response;
                }

                //var isCommunicationRefrence = await _db.Communication.AnyAsync(x => x.PropertyIds == assetId && x.IsDeleted != true);
                //if (isCommunicationRefrence)
                //{
                //    response.StatusCode = HttpStatusCode.InternalServerError;
                //    response.IsSuccess = false;
                //    response.ErrorMessages.Add("This Asset has refrence in Lease.");
                //    return response;
                //}

                var isDocumentsRefrence = await _db.Documents.AnyAsync(x => x.AssetId == assetId && x.IsDeleted != true);
                if (isDocumentsRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Asset has refrence in Documents.");
                    return response;
                }

                var asset = await _db.Assets.FindAsync(assetId);
                if (asset == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Asset not found.");
                    return response;
                }

                asset.IsDeleted = true;
                _db.Assets.Update(asset);

                await _db.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Result = "Asset deleted successfully.";
                
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
                throw new Exception(string.Join(", ", response.ErrorMessages));
            }

            return response;
        }


        #endregion

        #region Communication

        public async Task<List<CommunicationDto>> GetAllCommunicationAsync()
        {
            try
            {
                var communicationTypes = await _db.Communication
                                             .Where(x => x.IsDeleted != true)
                                             .AsNoTracking()
                                             .ToListAsync();



                var propertyTypeDtos = communicationTypes.Select(communication => new CommunicationDto
                {
                    Communication_Id = communication.Communication_Id,
                    Communication_File = communication.Communication_File,
                    Subject = communication.Subject,
                    Message = communication.Message,
                    PropertyIds = communication.PropertyIds,
                    TenantIds = communication.TenantIds,
                    IsByEmail = communication.IsByEmail,
                    IsByText = communication.IsByText,
                    IsShowCommunicationInTenantPortal = communication.IsShowCommunicationInTenantPortal,
                    IsPostOnTenantScreen = communication.IsPostOnTenantScreen,
                    RemoveFeedDate = communication.RemoveFeedDate,
                    AddedBy = communication.AddedBy,
                    AddedAt = communication.AddedDate,
                    UserID = communication.AppTenantId.ToString(),
                }).ToList();


                return propertyTypeDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping communication: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateCommunicationAsync(CommunicationDto communication)
        {
            var CommunicationData = new Communication

            {
                Communication_Id = communication.Communication_Id,
                Communication_File = communication.Communication_File,
                Subject = communication.Subject,
                Message = communication.Message,
                PropertyIds = communication.PropertyIds,
                TenantIds = communication.TenantIds,
                IsByEmail = communication.IsByEmail,
                IsByText = communication.IsByText,
                AppTenantId = Guid.Parse(communication.UserID),
                IsShowCommunicationInTenantPortal = communication.IsShowCommunicationInTenantPortal,
                IsPostOnTenantScreen = communication.IsPostOnTenantScreen,
                RemoveFeedDate = communication.RemoveFeedDate,
                AddedBy = communication.AddedBy,
                AddedDate = DateTime.Now,

            };
            await _db.Communication.AddAsync(CommunicationData);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateCommunicationAsync(CommunicationDto communication)
        {

            var existingCommunication = await _db.Communication.FindAsync(communication.Communication_Id);
            if (existingCommunication == null)
            {
                return false;
            }

            existingCommunication.Communication_Id = communication.Communication_Id;
            existingCommunication.Communication_File = communication.Communication_File;
            existingCommunication.Subject = communication.Subject;
            existingCommunication.Message = communication.Message;
            existingCommunication.PropertyIds = communication.PropertyIds;
            existingCommunication.TenantIds = communication.TenantIds;
            existingCommunication.IsByEmail = communication.IsByEmail;
            existingCommunication.IsByText = communication.IsByText;
            existingCommunication.AppTenantId = Guid.Parse(communication.UserID);
            existingCommunication.IsShowCommunicationInTenantPortal = communication.IsShowCommunicationInTenantPortal;
            existingCommunication.IsPostOnTenantScreen = communication.IsPostOnTenantScreen;
            existingCommunication.RemoveFeedDate = communication.RemoveFeedDate;
            existingCommunication.AddedBy = communication.AddedBy;
            existingCommunication.AddedDate = DateTime.Now;

            _db.Communication.Update(existingCommunication);
            var result = await _db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteCommunicationAsync(int communicationId)
        {
            var communication = await _db.Communication.FindAsync(communicationId);
            if (communication == null)
            {
                return false;
            }

            _db.Communication.Remove(communication);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }

        #endregion


        #region Landlord
        public async Task<List<OwnerDto>> GetAllLandlordAsync()
        {
            try
            {
                var ownerDtos = await (from owner in _db.Owner
                                       join organization in _db.OwnerOrganization
                                       on owner.OwnerId equals organization.OwnerId into orgGroup
                                       from org in orgGroup.DefaultIfEmpty()
                                       where owner.IsDeleted != true
                                       select new OwnerDto
                                       {
                                           OwnerId = owner.OwnerId,
                                           FirstName = owner.FirstName,
                                           MiddleName = owner.MiddleName,
                                           LastName = owner.LastName,
                                           Fax = owner.Fax,
                                           TaxId = owner.TaxId,
                                           EmailAddress = owner.EmailAddress,
                                           EmailAddress2 = owner.EmailAddress2,
                                           PictureName = owner.Picture,
                                           Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Picture_" + owner.OwnerId + Path.GetExtension(owner.Picture)}",
                                           PhoneNumber = owner.PhoneNumber,
                                           PhoneNumber2 = owner.PhoneNumber2,
                                           EmergencyContactInfo = owner.EmergencyContactInfo,
                                           LeaseAgreementId = owner.LeaseAgreementId,
                                           OwnerNationality = owner.OwnerNationality,
                                           Gender = owner.Gender,
                                           DocumentName = owner.Document,
                                           Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Document_" + owner.OwnerId + Path.GetExtension(owner.Document)}",
                                           DOB = owner.DOB,
                                           VAT = owner.VAT,
                                           LegalName = owner.LegalName,
                                           Account_Name = owner.Account_Name,
                                           Account_Holder = owner.Account_Holder,
                                           Account_IBAN = owner.Account_IBAN,
                                           Account_Swift = owner.Account_Swift,
                                           Account_Bank = owner.Account_Bank,
                                           Account_Currency = owner.Account_Currency,
                                           OrganizationName = org.OrganizationName,
                                           OrganizationDescription = org.OrganizationDescription,
                                           OrganizationIcon = org.OrganizationIcon,
                                           OrganizationLogo = org.OrganizationLogo,
                                           Website = org.Website,
                                           AddedBy = owner.AddedBy,
                                           AddedDate = owner.AddedDate,
                                           ModifiedDate = owner.ModifiedDate

                                       })
                                       .AsNoTracking()
                                       .ToListAsync();

                return ownerDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping owners and organizations: {ex.Message}");
                throw;
            }
        }

        public async Task<List<OwnerDto>> GetAllLandlordDllAsync(Filter filter)
        {
            try
            {
                var ownerDtos = await (from owner in _db.Owner
                                       where owner.IsDeleted != true && owner.AddedBy == filter.AddedBy
                                       select new OwnerDto
                                       {
                                           OwnerId = owner.OwnerId,
                                           FirstName = owner.FirstName,
                                           LastName = owner.LastName,
                                           AddedBy = owner.AddedBy,
                                       })
                                       .AsNoTracking()
                                       .ToListAsync();

                return ownerDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping owners: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Subscritions

        public async Task<List<SubscriptionDto>> GetAllSubscriptionsAsync()
        {
            return await _db.Subscriptions.Where(x => x.IsDeleted != true).Select(subscription => new SubscriptionDto
            {
                Id = subscription.Id,
                SubscriptionName = subscription.SubscriptionName,
                Price = subscription.Price,
                SmallDescription = subscription.SmallDescription,
                SubscriptionType = subscription.SubscriptionType,
                Currency = subscription.Currency,
                NoOfUnits = subscription.NoOfUnits,
                Tax = subscription.Tax,
                AppTenantId = subscription.AppTenantId ?? "",
                TenantId = subscription.TenantId
            }).ToListAsync();
        }

        public async Task<SubscriptionDto> GetSubscriptionByIdAsync(int Id)
        {
            var subscription = await _db.Subscriptions.FindAsync(Id);
            if (subscription == null) return null;
            return new SubscriptionDto
            {
                Id = subscription.Id,
                SubscriptionName = subscription.SubscriptionName,
                Price = subscription.Price,
                SmallDescription = subscription.SmallDescription,
                SubscriptionType = subscription.SubscriptionType,
                Currency = subscription.Currency,
                NoOfUnits = subscription.NoOfUnits,
                Tax = subscription.Tax,
                AppTenantId = subscription.AppTenantId ?? "",
                TenantId = subscription.TenantId
            };
        }

        public async Task<bool> CreateSubscriptionAsync(SubscriptionDto subscriptionDto)
        {
            var subscription = new Subscription
            {
                Id = subscriptionDto.Id,
                SubscriptionName = subscriptionDto.SubscriptionName,
                Price = subscriptionDto.Price,
                SmallDescription = subscriptionDto.SmallDescription,
                SubscriptionType = subscriptionDto.SubscriptionType,
                Currency = subscriptionDto.Currency,
                NoOfUnits = subscriptionDto.NoOfUnits,
                Tax = subscriptionDto.Tax,
                AppTenantId = subscriptionDto.AppTenantId ?? "",
                TenantId = subscriptionDto.TenantId
            };
            await _db.Subscriptions.AddAsync(subscription);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateSubscriptionAsync(SubscriptionDto subscriptionDto)
        {
            var subscription = await _db.Subscriptions.FindAsync(subscriptionDto.Id);
            if (subscription == null) return false;

            subscription.Id = subscriptionDto.Id;
            subscription.SubscriptionName = subscriptionDto.SubscriptionName;
            subscription.Price = subscriptionDto.Price;
            subscription.SmallDescription = subscriptionDto.SmallDescription;
            subscription.SubscriptionType = subscriptionDto.SubscriptionType;
            subscription.Currency = subscriptionDto.Currency;
            subscription.NoOfUnits = subscriptionDto.NoOfUnits;
            subscription.Tax = subscriptionDto.Tax;
            subscription.AppTenantId = subscriptionDto.AppTenantId ?? "";
            subscription.TenantId = subscriptionDto.TenantId;

            _db.Subscriptions.Update(subscription);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteSubscriptionAsync(int Id)
        {
            var subscription = await _db.Subscriptions.FindAsync(Id);
            if (subscription == null) return false;

            _db.Subscriptions.Remove(subscription);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region TaskRequest

        public async Task<List<TaskRequestHistoryDto>> GetTaskRequestHistoryAsync(int id)
        {
            try
            {
                var result = await (from t in _db.TaskRequestHistory
                                    where t.TaskRequestId == id && t.IsDeleted != true
                                    select new TaskRequestHistoryDto
                                    {
                                        TaskRequestHistoryId = t.TaskRequestHistoryId,
                                        TaskRequestId = t.TaskRequestId,
                                        Status = t.Status,
                                        Date = t.Date,
                                        Remarks = t.Remarks,
                                        AddedBy = t.AddedBy,

                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping task history: {ex.Message}");
                throw;
            }
        }

        public async Task<List<TaskRequestDto>> GetMaintenanceTasksAsync()
        {
            try
            {
                var result = await (from t in _db.TaskRequest
                                    from a in _db.Assets.Where(x => x.AssetId == t.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == t.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == t.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                        //from l in _db.LineItem.Where(x=>x.TaskRequestId == t.TaskRequestId).DefaultIfEmpty()
                                    where t.Status != TaskStatusTypes.NotStarted.ToString() && t.IsDeleted != true
                                    select new TaskRequestDto
                                    {
                                        TaskRequestId = t.TaskRequestId,
                                        Type = t.Type,
                                        Subject = t.Subject,
                                        Description = t.Description,
                                        IsOneTimeTask = t.IsOneTimeTask,
                                        IsRecurringTask = t.IsRecurringTask,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        Frequency = t.Frequency,
                                        DueDays = t.DueDays,
                                        IsTaskRepeat = t.IsTaskRepeat,
                                        DueDate = t.DueDate,
                                        Status = t.Status,
                                        Priority = t.Priority,
                                        Assignees = t.Assignees,
                                        IsNotifyAssignee = t.IsNotifyAssignee,
                                        AssetId = t.AssetId,
                                        Asset = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = t.UnitId,
                                        Unit = u.UnitName,
                                        TaskRequestFileName = t.TaskRequestFile,
                                        TaskRequestFile = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Task_TaskRequestFile_" + t.TaskRequestId + Path.GetExtension(t.TaskRequestFile)}",
                                        OwnerId = t.OwnerId,
                                        Owner = o.FirstName + " " + o.LastName,
                                        IsNotifyOwner = t.IsNotifyOwner,
                                        TenantId = t.TenantId,
                                        Tenant = tnt.FirstName + " " + tnt.LastName,
                                        IsNotifyTenant = t.IsNotifyTenant,
                                        HasPermissionToEnter = t.HasPermissionToEnter,
                                        EntryNotes = t.EntryNotes,
                                        VendorId = t.VendorId,
                                        ApprovedByOwner = t.ApprovedByOwner,
                                        PartsAndLabor = t.PartsAndLabor,
                                        AddedBy = t.AddedBy,
                                        LineItems = (from item in _db.LineItem
                                                     where item.TaskRequestId == t.TaskRequestId && item.IsDeleted != true
                                                     select new LineItemDto
                                                     {
                                                         LineItemId = item.LineItemId,
                                                         TaskRequestId = item.TaskRequestId,
                                                         Quantity = item.Quantity,
                                                         Price = item.Price,
                                                         ChartAccountId = item.ChartAccountId,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping maintainance task: {ex.Message}");
                throw;
            }
        }

        public async Task<List<TaskRequestDto>> GetAllTaskRequestsAsync()
        {
            try
            {
                var result = await (from t in _db.TaskRequest
                                    from a in _db.Assets.Where(x => x.AssetId == t.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == t.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == t.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                    where t.IsDeleted != true
                                    select new TaskRequestDto
                                    {
                                        TaskRequestId = t.TaskRequestId,
                                        Type = t.Type,
                                        Subject = t.Subject,
                                        Description = t.Description,
                                        IsOneTimeTask = t.IsOneTimeTask,
                                        IsRecurringTask = t.IsRecurringTask,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        Frequency = t.Frequency,
                                        DueDays = t.DueDays,
                                        IsTaskRepeat = t.IsTaskRepeat,
                                        DueDate = t.DueDate,
                                        Status = t.Status,
                                        Priority = t.Priority,
                                        Assignees = t.Assignees,
                                        IsNotifyAssignee = t.IsNotifyAssignee,
                                        AssetId = t.AssetId,
                                        Asset = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = t.UnitId,
                                        Unit = u.UnitName,
                                        TaskRequestFileName = t.TaskRequestFile,
                                        TaskRequestFile = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Task_TaskRequestFile_" + t.TaskRequestId + Path.GetExtension(t.TaskRequestFile)}",
                                        OwnerId = t.OwnerId,
                                        Owner = o.FirstName + " " + o.LastName,
                                        IsNotifyOwner = t.IsNotifyOwner,
                                        TenantId = t.TenantId,
                                        Tenant = tnt.FirstName + " " + tnt.LastName,
                                        IsNotifyTenant = t.IsNotifyTenant,
                                        HasPermissionToEnter = t.HasPermissionToEnter,
                                        EntryNotes = t.EntryNotes,
                                        VendorId = t.VendorId,
                                        ApprovedByOwner = t.ApprovedByOwner,
                                        PartsAndLabor = t.PartsAndLabor,
                                        AddedBy = t.AddedBy,
                                        AddedDate = t.AddedDate,
                                        ModifiedDate = t.ModifiedDate,
                                        LineItems = (from item in _db.LineItem
                                                     from ca in _db.ChartAccount.Where(x => x.ChartAccountId == item.ChartAccountId && x.IsDeleted != true).DefaultIfEmpty()
                                                     where item.TaskRequestId == t.TaskRequestId && item.IsDeleted != true
                                                     select new LineItemDto
                                                     {
                                                         LineItemId = item.LineItemId,
                                                         TaskRequestId = item.TaskRequestId,
                                                         Quantity = item.Quantity,
                                                         Price = item.Price,
                                                         ChartAccountId = item.ChartAccountId,
                                                         AccountName = ca.Name,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping task requests: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LineItemDto>> GetAllLineItemsAsync()
        {
            try
            {
                var result = await (from item in _db.LineItem
                                    from ca in _db.ChartAccount.Where(x => x.ChartAccountId == item.ChartAccountId && x.IsDeleted != true).DefaultIfEmpty()
                                    where item.IsDeleted != true
                                    select new LineItemDto
                                    {
                                        LineItemId = item.LineItemId,
                                        TaskRequestId = item.TaskRequestId,
                                        Quantity = item.Quantity,
                                        Price = item.Price,
                                        ChartAccountId = item.ChartAccountId,
                                        AccountName = ca.Name,
                                        Memo = item.Memo,
                                        AddedDate = item.AddedDate,
                                        AddedBy = item.AddedBy
                                    })
                     .AsNoTracking()
                     .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping line items {ex.Message}");
                throw;
            }
        }
        public async Task<List<TaskRequestDto>> GetExpenseByAssetAsync(int assetId)
        {
            try
            {

               var result = await (from t in _db.TaskRequest
                                   from a in _db.Assets.Where(x => x.AssetId == t.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                   from u in _db.AssetsUnits.Where(x => x.UnitId == t.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                   where t.IsDeleted != true
                                   select new TaskRequestDto
                                    {
                                        TaskRequestId = t.TaskRequestId,
                                        Type = t.Type,
                                        Subject = t.Subject,
                                        Description = t.Description,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        Frequency = t.Frequency,
                                        DueDays = t.DueDays,
                                        IsTaskRepeat = t.IsTaskRepeat,
                                        DueDate = t.DueDate,
                                        Status = t.Status,
                                        Assignees = t.Assignees,
                                        IsNotifyAssignee = t.IsNotifyAssignee,
                                        AssetId = t.AssetId,
                                        Asset = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = t.UnitId,
                                        Unit = u.UnitName,
                                        HasPermissionToEnter = t.HasPermissionToEnter,
                                        EntryNotes = t.EntryNotes,
                                        VendorId = t.VendorId,
                                        PartsAndLabor = t.PartsAndLabor,
                                        LineItems = (from item in _db.LineItem
                                                     where item.TaskRequestId == t.TaskRequestId && item.IsDeleted != true
                                                     select new LineItemDto
                                                     {
                                                         LineItemId = item.LineItemId,
                                                         TaskRequestId = item.TaskRequestId,
                                                         Quantity = item.Quantity,
                                                         Price = item.Price,
                                                         ChartAccountId = item.ChartAccountId,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping line items {ex.Message}");
                throw;
            }
        }

        public async Task<List<TaskRequestDto>> GetTasksByTenantAsync(int tenantId)
        {
            try
            {

               var result = await (from t in _db.TaskRequest
                                   from tn in _db.Tenant.Where(x => x.TenantId == t.TenantId).DefaultIfEmpty()
                                   where t.TenantId == tenantId && t.IsDeleted != true
                                   select new TaskRequestDto
                                    {
                                        TaskRequestId = t.TaskRequestId,
                                        Type = t.Type,
                                        Subject = t.Subject,
                                        Description = t.Description,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        Frequency = t.Frequency,
                                        DueDays = t.DueDays,
                                        IsTaskRepeat = t.IsTaskRepeat,
                                        DueDate = t.DueDate,
                                        Priority = t.Priority,
                                        Status = t.Status,
                                        Assignees = t.Assignees,
                                        IsNotifyAssignee = t.IsNotifyAssignee,
                                        TenantId = t.TenantId,
                                        Tenant = tn.FirstName + " " + tn.LastName,
                                        HasPermissionToEnter = t.HasPermissionToEnter,
                                        EntryNotes = t.EntryNotes,
                                        VendorId = t.VendorId,
                                        PartsAndLabor = t.PartsAndLabor,
                                    })
                     .AsNoTracking()
                     .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping line items {ex.Message}");
                throw;
            }
        }
        public async Task<List<TaskRequestDto>> GetTasksByLandLordAsync(int landlordId)
        {
            try
            {

               var result = await (from t in _db.TaskRequest
                                   from a in _db.Assets.Where(x => x.AssetId == t.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                   from u in _db.AssetsUnits.Where(x => x.UnitId == t.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                   from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId).DefaultIfEmpty()
                                   where t.OwnerId == landlordId && t.IsDeleted != true
                                   select new TaskRequestDto
                                    {
                                        TaskRequestId = t.TaskRequestId,
                                        Type = t.Type,
                                        Subject = t.Subject,
                                        Description = t.Description,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        Frequency = t.Frequency,
                                        DueDays = t.DueDays,
                                        IsTaskRepeat = t.IsTaskRepeat,
                                        DueDate = t.DueDate,
                                        Priority = t.Priority,
                                        Status = t.Status,
                                        Assignees = t.Assignees,
                                        IsNotifyAssignee = t.IsNotifyAssignee,
                                        OwnerId = t.OwnerId,
                                        Owner = o.FirstName + " " + o.LastName,
                                        AssetId = t.AssetId,
                                        Asset = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = t.UnitId,
                                        Unit = u.UnitName,
                                        HasPermissionToEnter = t.HasPermissionToEnter,
                                        EntryNotes = t.EntryNotes,
                                        VendorId = t.VendorId,
                                        PartsAndLabor = t.PartsAndLabor,
                                    })
                     .AsNoTracking()
                     .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping line items {ex.Message}");
                throw;
            }
        }

        public async Task<List<TaskRequestDto>> GetTaskRequestsAsync()
        {
            try
            {
                var result = await (from t in _db.TaskRequest
                                    from a in _db.Assets.Where(x => x.AssetId == t.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == t.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == t.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                        //from l in _db.LineItem.Where(x=>x.TaskRequestId == t.TaskRequestId).DefaultIfEmpty()
                                    where t.Status == TaskStatusTypes.NotStarted.ToString() && t.IsDeleted != true
                                    select new TaskRequestDto
                                    {
                                        TaskRequestId = t.TaskRequestId,
                                        Type = t.Type,
                                        Subject = t.Subject,
                                        Description = t.Description,
                                        IsOneTimeTask = t.IsOneTimeTask,
                                        IsRecurringTask = t.IsRecurringTask,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        Frequency = t.Frequency,
                                        DueDays = t.DueDays,
                                        IsTaskRepeat = t.IsTaskRepeat,
                                        DueDate = t.DueDate,
                                        Status = t.Status,
                                        Priority = t.Priority,
                                        Assignees = t.Assignees,
                                        IsNotifyAssignee = t.IsNotifyAssignee,
                                        AssetId = t.AssetId,
                                        Asset = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = t.UnitId,
                                        Unit = u.UnitName,
                                        TaskRequestFileName = t.TaskRequestFile,
                                        TaskRequestFile = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Task_TaskRequestFile_" + t.TaskRequestId + Path.GetExtension(t.TaskRequestFile)}",
                                        OwnerId = t.OwnerId,
                                        Owner = o.FirstName + " " + o.LastName,
                                        IsNotifyOwner = t.IsNotifyOwner,
                                        TenantId = t.TenantId,
                                        Tenant = tnt.FirstName + " " + tnt.LastName,
                                        IsNotifyTenant = t.IsNotifyTenant,
                                        HasPermissionToEnter = t.HasPermissionToEnter,
                                        EntryNotes = t.EntryNotes,
                                        VendorId = t.VendorId,
                                        ApprovedByOwner = t.ApprovedByOwner,
                                        PartsAndLabor = t.PartsAndLabor,
                                        AddedBy = t.AddedBy,
                                        LineItems = (from item in _db.LineItem
                                                     from ca in _db.ChartAccount.Where(x => x.ChartAccountId == item.ChartAccountId && x.IsDeleted != true).DefaultIfEmpty()
                                                     where item.TaskRequestId == t.TaskRequestId && item.IsDeleted != true
                                                     select new LineItemDto
                                                     {
                                                         LineItemId = item.LineItemId,
                                                         TaskRequestId = item.TaskRequestId,
                                                         Quantity = item.Quantity,
                                                         Price = item.Price,
                                                         ChartAccountId = item.ChartAccountId,
                                                         AccountName = ca.Name,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping task requests: {ex.Message}");
                throw;
            }
        }

        public async Task<TaskRequestDto> GetTaskByIdAsync(int id)
        {
            try
            {
                var result = await (from t in _db.TaskRequest
                                    from a in _db.Assets.Where(x => x.AssetId == t.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == t.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == t.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                    where t.TaskRequestId == id && t.IsDeleted != true
                                    select new TaskRequestDto
                                    {
                                        TaskRequestId = t.TaskRequestId,
                                        Type = t.Type,
                                        Subject = t.Subject,
                                        Description = t.Description,
                                        IsOneTimeTask = t.IsOneTimeTask,
                                        IsRecurringTask = t.IsRecurringTask,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        Frequency = t.Frequency,
                                        DueDays = t.DueDays,
                                        IsTaskRepeat = t.IsTaskRepeat,
                                        DueDate = t.DueDate,
                                        Status = t.Status,
                                        Priority = t.Priority,
                                        Assignees = t.Assignees,
                                        IsNotifyAssignee = t.IsNotifyAssignee,
                                        AssetId = t.AssetId,
                                        Asset = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = t.UnitId,
                                        Unit = u.UnitName,
                                        TaskRequestFileName = t.TaskRequestFile,
                                        TaskRequestFile = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Task_TaskRequestFile_" + t.TaskRequestId + Path.GetExtension(t.TaskRequestFile)}",
                                        OwnerId = t.OwnerId,
                                        Owner = o.FirstName + " " + o.LastName,
                                        IsNotifyOwner = t.IsNotifyOwner,
                                        TenantId = t.TenantId,
                                        Tenant = tnt.FirstName + " " + tnt.LastName,
                                        IsNotifyTenant = t.IsNotifyTenant,
                                        HasPermissionToEnter = t.HasPermissionToEnter,
                                        EntryNotes = t.EntryNotes,
                                        VendorId = t.VendorId,
                                        ApprovedByOwner = t.ApprovedByOwner,
                                        PartsAndLabor = t.PartsAndLabor,
                                        AddedBy = t.AddedBy,
                                        LineItems = (from item in _db.LineItem
                                                     from ca in _db.ChartAccount.Where(x => x.ChartAccountId == item.ChartAccountId && x.IsDeleted != true).DefaultIfEmpty()
                                                     where item.TaskRequestId == t.TaskRequestId && item.IsDeleted != true
                                                     select new LineItemDto
                                                     {
                                                         LineItemId = item.LineItemId,
                                                         TaskRequestId = item.TaskRequestId,
                                                         Quantity = item.Quantity,
                                                         Price = item.Price,
                                                         ChartAccountId = item.ChartAccountId,
                                                         AccountName = ca.Name,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping task request: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveTaskAsync(TaskRequestDto taskRequestDto)
        {
            try
            {
                var taskRequest = _db.TaskRequest.FirstOrDefault(x => x.TaskRequestId == taskRequestDto.TaskRequestId && x.IsDeleted != true);

                if (taskRequest == null)
                    taskRequest = new TaskRequest();

                taskRequest.TaskRequestId = taskRequestDto.TaskRequestId;
                taskRequest.Type = taskRequestDto.Type;
                taskRequest.Subject = taskRequestDto.Subject;
                taskRequest.Description = taskRequestDto.Description;
                taskRequest.IsOneTimeTask = taskRequestDto.IsOneTimeTask;
                taskRequest.IsRecurringTask = taskRequestDto.IsRecurringTask;
                taskRequest.StartDate = taskRequestDto.StartDate;
                taskRequest.EndDate = taskRequestDto.EndDate;
                taskRequest.Frequency = taskRequestDto.Frequency;
                taskRequest.DueDays = taskRequestDto.DueDays;
                taskRequest.IsTaskRepeat = taskRequestDto.IsTaskRepeat;
                taskRequest.DueDate = taskRequestDto.DueDate;
                taskRequest.Status = taskRequestDto.Status;
                taskRequest.Priority = taskRequestDto.Priority;
                taskRequest.Assignees = taskRequestDto.Assignees;
                taskRequest.IsNotifyAssignee = taskRequestDto.IsNotifyAssignee;
                taskRequest.AssetId = taskRequestDto.AssetId;
                taskRequest.UnitId = taskRequestDto.UnitId;
                if (taskRequestDto.TaskRequestFileName != null)
                {
                    taskRequest.TaskRequestFile = taskRequestDto.TaskRequestFileName;
                }
                taskRequest.OwnerId = taskRequestDto.OwnerId;
                taskRequest.IsNotifyOwner = taskRequestDto.IsNotifyOwner;
                taskRequest.TenantId = taskRequestDto.TenantId;
                taskRequest.IsNotifyTenant = taskRequestDto.IsNotifyTenant;
                taskRequest.HasPermissionToEnter = taskRequestDto.HasPermissionToEnter;
                taskRequest.EntryNotes = taskRequestDto.EntryNotes;
                taskRequest.VendorId = taskRequestDto.VendorId;
                taskRequest.ApprovedByOwner = taskRequestDto.ApprovedByOwner;
                taskRequest.PartsAndLabor = taskRequestDto.PartsAndLabor;

                if (taskRequestDto.TaskRequestId > 0)
                {
                    taskRequest.ModifiedBy = taskRequestDto.AddedBy;
                    taskRequest.ModifiedDate = DateTime.Now;
                    _db.TaskRequest.Update(taskRequest);
                }
                else
                {
                    taskRequest.AddedBy = taskRequestDto.AddedBy;
                    taskRequest.AddedDate = DateTime.Now;
                    _db.TaskRequest.Add(taskRequest);
                }

                await _db.SaveChangesAsync();

                var maxId = 0;
                if (taskRequestDto.TaskRequestId > 0)
                    maxId = taskRequest.TaskRequestId;
                else
                    maxId = _db.TaskRequest.Max(x => x.TaskRequestId);


                int[] lineItemIds = taskRequestDto.LineItems.Select(x => x.LineItemId).ToArray();
                var lineItemsToBeDeleted = _db.LineItem
                    .Where(item => item.TaskRequestId == taskRequestDto.TaskRequestId && !lineItemIds.Contains(item.LineItemId) && item.IsDeleted != true)
                    .ToList();

                foreach (var lineItemToBeDeleted in lineItemsToBeDeleted)
                {
                    lineItemToBeDeleted.IsDeleted = true;
                    lineItemToBeDeleted.ModifiedBy = taskRequestDto.AddedBy;
                    lineItemToBeDeleted.ModifiedDate = DateTime.Now;
                    _db.LineItem.Update(lineItemToBeDeleted);
                }
                if (taskRequestDto.LineItems != null && taskRequestDto.LineItems.Any())
                {
                    var existingLineItems = _db.LineItem.Where(item => item.TaskRequestId == taskRequestDto.TaskRequestId && item.IsDeleted != true).ToList();
                    foreach (var lineItemDto in taskRequestDto.LineItems)
                    {
                        var existingLineItem = existingLineItems.FirstOrDefault(item => item.LineItemId == lineItemDto.LineItemId && item.IsDeleted != true);

                        if (existingLineItem != null)
                        {
                            existingLineItem.Quantity = lineItemDto.Quantity;
                            existingLineItem.Price = lineItemDto.Price;
                            existingLineItem.ChartAccountId = lineItemDto.ChartAccountId;
                            existingLineItem.Memo = lineItemDto.Memo;
                            existingLineItem.ModifiedBy = taskRequestDto.AddedBy;
                            existingLineItem.ModifiedDate = DateTime.Now;
                            _db.LineItem.Update(existingLineItem);
                        }
                        else
                        {
                            var newLineItem = new LineItem
                            {
                                TaskRequestId = maxId,
                                Quantity = lineItemDto.Quantity,
                                Price = lineItemDto.Price,
                                ChartAccountId = lineItemDto.ChartAccountId,
                                Memo = lineItemDto.Memo,
                                AddedDate = DateTime.Now,
                                AddedBy = taskRequestDto.AddedBy
                            };
                            _db.LineItem.Add(newLineItem);
                        }
                    }
                }

                await _db.SaveChangesAsync();


                var taskRequestHistory = new TaskRequestHistory();

                taskRequestHistory.TaskRequestId = maxId;
                taskRequestHistory.Date = DateTime.Now;
                taskRequestHistory.Status = taskRequestDto.Status;
                taskRequestHistory.Remarks = taskRequestDto.Description;
                taskRequestHistory.AddedBy = taskRequestDto.AddedBy;
                taskRequestHistory.AddedDate = DateTime.Now;
                _db.TaskRequestHistory.Add(taskRequestHistory);

                var result = await _db.SaveChangesAsync();

                if (taskRequestDto.TaskRequestFile != null)
                {

                    var ext = Path.GetExtension(taskRequestDto.TaskRequestFileName);
                    await _googleCloudStorageService.UploadImagebyBase64Async(taskRequestDto.TaskRequestFile, "Task_TaskRequestFile_" + taskRequest.TaskRequestId + ext);

                }

                return result > 0;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _db.TaskRequest.FindAsync(id);
            if (task == null) return false;

            task.IsDeleted = true;
            _db.TaskRequest.Update(task);
            var saveResult = await _db.SaveChangesAsync();

            var lineItems = await _db.LineItem.Where(x => x.TaskRequestId == id && x.IsDeleted != true).ToListAsync();

            lineItems.ForEach(x => x.IsDeleted = true);
            _db.LineItem.UpdateRange(lineItems);
            var lineItemSaveResult = await _db.SaveChangesAsync();

            return saveResult > 0 || lineItemSaveResult > 0;
        }

        public async Task<bool> SaveTaskHistoryAsync(TaskRequestHistoryDto taskRequestHistoryDto)
        {
            var taskRequest = await _db.TaskRequest.FirstOrDefaultAsync(x => x.TaskRequestId == taskRequestHistoryDto.TaskRequestId && x.IsDeleted != true);

            if (taskRequest != null)
            {
                taskRequest.Status = taskRequestHistoryDto.Status;
                taskRequest.ModifiedBy = taskRequestHistoryDto.AddedBy;
                taskRequest.ModifiedDate = DateTime.Now;

                if (taskRequest.Type == TaskTypes.WorkOrderRequest)
                {
                    taskRequest.AccountName = taskRequestHistoryDto.AccountName;
                    taskRequest.AccountHolder = taskRequestHistoryDto.AccountHolder;
                    taskRequest.AccountIBAN = taskRequestHistoryDto.AccountIBAN;
                    taskRequest.AccountSwift = taskRequestHistoryDto.AccountSwift;
                    taskRequest.AccountBank = taskRequestHistoryDto.AccountBank;
                    taskRequest.AccountCurrency = taskRequestHistoryDto.AccountCurrency;
                    if (taskRequestHistoryDto.DocumentFile != null)
                    {
                        taskRequest.DocumentFile = taskRequestHistoryDto.DocumentFile.FileName;
                    }

                }

                _db.TaskRequest.Update(taskRequest);

                await _db.SaveChangesAsync();
            }

            var taskRequestHistory = await _db.TaskRequestHistory.FirstOrDefaultAsync(x => x.TaskRequestHistoryId == taskRequestHistoryDto.TaskRequestHistoryId && x.IsDeleted != true);

            if (taskRequestHistory == null)
                taskRequestHistory = new TaskRequestHistory();

            taskRequestHistory.TaskRequestHistoryId = taskRequestHistoryDto.TaskRequestHistoryId;
            taskRequestHistory.TaskRequestId = taskRequestHistoryDto.TaskRequestId;
            taskRequestHistory.Date = DateTime.Now;
            taskRequestHistory.Status = taskRequestHistoryDto.Status;

            taskRequestHistory.Remarks = taskRequestHistoryDto.Remarks;

            if (taskRequestHistoryDto.TaskRequestHistoryId > 0)
            {
                taskRequestHistory.ModifiedBy = taskRequestHistoryDto.AddedBy;
                taskRequestHistory.ModifiedDate = DateTime.Now;
                _db.TaskRequestHistory.Update(taskRequestHistory);
            }
            else
            {
                taskRequestHistory.AddedBy = taskRequestHistoryDto.AddedBy;
                taskRequestHistory.AddedDate = DateTime.Now;
                _db.TaskRequestHistory.Add(taskRequestHistory);
            }

            var result = await _db.SaveChangesAsync();

            if (taskRequestHistoryDto.DocumentFile != null)
            {
                var ext = Path.GetExtension(taskRequestHistoryDto.DocumentFile.FileName);
                await _googleCloudStorageService.UploadImageAsync(taskRequestHistoryDto.DocumentFile, "TaskHistory_DocumentFile_" + taskRequestHistory.TaskRequestHistoryId + ext);
            }

            return result > 0;
        }

        #endregion

        #region LandloadGetData

        public async Task<LandlordDataDto> GetLandlordDataById(int ownerId)
        {
            try
            {
                var ownerData = await _db.Owner
                    .Where(o => o.OwnerId == ownerId && o.IsDeleted != true)
                    .Select(o => new
                    {
                        Owner = o,
                        OwnerOrganization = _db.OwnerOrganization
                            .Where(oo => oo.OwnerId == o.OwnerId && oo.IsDeleted != true)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (ownerData == null)
                {
                    return null;
                }

                var landlordData = new LandlordDataDto
                {
                    OwnerId = ownerData.Owner.OwnerId,
                    FirstName = ownerData.Owner.FirstName,
                    MiddleName = ownerData.Owner.MiddleName,
                    LastName = ownerData.Owner.LastName,
                    EmailAddress = ownerData.Owner.EmailAddress,
                    EmailAddress2 = ownerData.Owner.EmailAddress2,
                    PhoneNumber = ownerData.Owner.PhoneNumber,
                    PhoneNumber2 = ownerData.Owner.PhoneNumber2,
                    Fax = ownerData.Owner.Fax,
                    TaxId = ownerData.Owner.TaxId,
                    DocumentName = ownerData.Owner.Document,
                    Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Document_" + ownerData.Owner.OwnerId + Path.GetExtension(ownerData.Owner.Document)}",
                    EmergencyContactInfo = ownerData.Owner.EmergencyContactInfo,
                    LeaseAgreementId = ownerData.Owner.LeaseAgreementId,
                    OwnerNationality = ownerData.Owner.OwnerNationality,
                    Gender = ownerData.Owner.Gender,
                    DOB = ownerData.Owner.DOB,
                    VAT = ownerData.Owner.VAT,
                    LegalName = ownerData.Owner.LegalName,
                    Account_Name = ownerData.Owner.Account_Name,
                    Account_Holder = ownerData.Owner.Account_Holder,
                    Account_IBAN = ownerData.Owner.Account_IBAN,
                    Account_Swift = ownerData.Owner.Account_Swift,
                    Account_Bank = ownerData.Owner.Account_Bank,
                    Account_Currency = ownerData.Owner.Account_Currency,
                    AppTenantId = ownerData.Owner.AppTenantId,
                    Address = ownerData.Owner.Address,
                    Address2 = ownerData.Owner.Address2,
                    Locality = ownerData.Owner.Locality,
                    Region = ownerData.Owner.Region,
                    PostalCode = ownerData.Owner.PostalCode,
                    Country = ownerData.Owner.Country,
                    CountryCode = ownerData.Owner.CountryCode,
                    PictureName = ownerData.Owner.Picture,
                    Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Picture_" + ownerData.Owner.OwnerId + Path.GetExtension(ownerData.Owner.Picture)}",
                    OrganizationName = ownerData.OwnerOrganization?.OrganizationName,
                    OrganizationDescription = ownerData.OwnerOrganization?.OrganizationDescription,
                    OrganizationIcon = ownerData.OwnerOrganization?.OrganizationIcon,
                    OrganizationLogo = ownerData.OwnerOrganization?.OrganizationLogo,
                    Website = ownerData.OwnerOrganization?.Website,
                    AddedBy = ownerData.Owner?.AddedBy,
                    AddedDate = ownerData.Owner?.AddedDate,
                };

                var assetsTask = await GetAssetsByOwnerId(ownerId);
                var leasesTask = await GetLeasesByOwnerId(ownerId);
                var invoicesTask = await GetInvoicesByOwnerId(ownerId);
                var taskRequestsTask = await GetTaskRequestsByOwnerId(ownerId);


                landlordData.Assets = assetsTask;
                landlordData.Leases = leasesTask;
                landlordData.Invoices = invoicesTask;
                landlordData.TaskRequest = taskRequestsTask;

                return landlordData;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<AssetDTO>> GetAssetsByOwnerId(int ownerId)
        {
            return await _db.Assets
                .Where(a => a.OwnerId == ownerId && a.IsDeleted != true)
                .Select(a => new AssetDTO
                {
                    AssetId = a.AssetId,
                    SelectedPropertyType = a.SelectedPropertyType,
                    SelectedBankAccountOption = a.SelectedBankAccountOption,
                    SelectedReserveFundsOption = a.SelectedReserveFundsOption,
                    SelectedSubtype = a.SelectedSubtype,
                    SelectedOwnershipOption = a.SelectedOwnershipOption,
                    BuildingNo = a.BuildingNo,
                    BuildingName = a.BuildingName,
                    Street1 = a.Street1,
                    Street2 = a.Street2,
                    City = a.City,
                    Country = a.Country,
                    Zipcode = a.Zipcode,
                    State = a.State,
                    AppTid = a.AppTenantId,
                    PictureFileName = a.Image,
                    Image = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Assets_Image_" + a.AssetId + Path.GetExtension(a.Image)}",
                    AddedBy = a.AddedBy,
                    AddedDate = a.AddedDate,
                    ModifiedDate = a.ModifiedDate,
                    Units = a.Units.Where(x => x.IsDeleted != true).Select(u => new UnitDTO
                    {
                        UnitId = u.UnitId,
                        AssetId = u.AssetId,
                        UnitName = u.UnitName,
                        Bath = u.Bath,
                        Beds = u.Beds,
                        Rent = u.Rent,
                        Size = u.Size,
                    }).ToList()
                })
                .ToListAsync();
        }

        private async Task<List<LeaseDto>> GetLeasesByOwnerId(int ownerId)
        {
            return await (from asset in _db.Assets
                          from lease in _db.Lease.Where(l => l.AssetId == asset.AssetId && l.IsDeleted != true).DefaultIfEmpty()
                          where asset.OwnerId == ownerId && asset.IsDeleted != true
                          select new LeaseDto
                          {
                              LeaseId = lease != null ? lease.LeaseId : 0,
                              StartDate = lease != null ? (DateTime)lease.StartDate : DateTime.MinValue,
                              EndDate = lease != null ? (DateTime)lease.EndDate : DateTime.MinValue,
                              IsSigned = lease != null ? lease.IsSigned : false,
                              SelectedProperty = lease != null ? lease.SelectedProperty : string.Empty,
                              SelectedUnit = lease != null ? lease.SelectedUnit : string.Empty,
                              SignatureImagePath = lease != null ? lease.SignatureImagePath : string.Empty,
                              IsFixedTerm = lease != null ? lease.IsFixedTerm : false,
                              IsMonthToMonth = lease != null ? lease.IsMonthToMonth : false,
                              HasSecurityDeposit = lease != null ? lease.HasSecurityDeposit : false,
                              LateFeesPolicy = lease != null ? lease.LateFeesPolicy : string.Empty,
                              TenantId = lease != null ? lease.TenantsTenantId : 0,
                              AppTenantId = lease != null ? lease.AppTenantId : string.Empty,
                              RentCharges = lease != null ? lease.RentCharges.Where(x => x.IsDeleted != true).Select(rc => new RentChargeDto
                              {
                                  RentChargeId = rc.RentChargeId,
                                  Amount = rc.Amount,
                                  Description = rc.Description,
                                  RentDate = rc.RentDate,
                                  RentPeriod = rc.RentPeriod
                              }).ToList() : new List<RentChargeDto>(),
                              FeeCharge = lease != null ? lease.FeeCharge.Where(x => x.IsDeleted != true).Select(fc => new FeeChargeDto
                              {
                                  FeeChargeId = fc.FeeChargeId,
                                  ChargeLatefeeActive = fc.ChargeLatefeeActive,
                                  UsePropertyDefaultStructure = fc.UsePropertyDefaultStructure,
                                  SpecifyLateFeeStructure = fc.SpecifyLateFeeStructure,
                                  DueDays = fc.DueDays,
                                  Frequency = fc.Frequency,
                                  CalculateFee = fc.CalculateFee,
                                  Amount = fc.Amount,
                                  ChartAccountId = fc.ChartAccountId,
                                  Description = fc.Description,
                                  IsSendARemainder = fc.IsSendARemainder,
                                  IsNotifyTenants = fc.IsNotifyTenants,
                                  IsEnableSms = fc.IsEnableSms,
                                  IsChargeLateFee = fc.IsChargeLateFee,
                                  IsMonthlyLimit = fc.IsMonthlyLimit,
                                  IsDailyLimit = fc.IsDailyLimit,
                                  IsMinimumBalance = fc.IsMinimumBalance,
                                  IsChargeLateFeeonSpecific = fc.IsChargeLateFeeonSpecific,
                                  FeeDate = fc.FeeDate
                              }).FirstOrDefault() : new FeeChargeDto(),
                              SecurityDeposits = lease != null ? lease.SecurityDeposit.Where(x => x.IsDeleted != true).Select(sd => new SecurityDepositDto
                              {
                                  SecurityDepositId = sd.SecurityDepositId,
                                  Amount = sd.Amount,
                                  Description = sd.Description
                              }).ToList() : new List<SecurityDepositDto>(),
                              Tenant = lease != null && lease.Tenants != null ? new TenantModelDto
                              {
                                  TenantId = lease.Tenants.TenantId,
                                  PictureName = lease.Tenants.Picture,
                                  Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Picture_" + lease.Tenants.TenantId + Path.GetExtension(lease.Tenants.Picture)}",
                                  DocumentName = lease.Tenants.Document,
                                  Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Document_" + lease.Tenants.TenantId + Path.GetExtension(lease.Tenants.Document)}",
                                  FirstName = lease.Tenants.FirstName,
                                  LastName = lease.Tenants.LastName,
                                  EmailAddress = lease.Tenants.EmailAddress,
                                  PhoneNumber = lease.Tenants.PhoneNumber,
                                  EmergencyContactInfo = lease.Tenants.EmergencyContactInfo,
                                  LeaseAgreementId = lease.Tenants.LeaseAgreementId,
                                  TenantNationality = lease.Tenants.TenantNationality,
                                  Gender = lease.Tenants.Gender,
                                  DOB = lease.Tenants.DOB,
                                  VAT = lease.Tenants.VAT,
                                  LegalName = lease.Tenants.LegalName,
                                  Account_Name = lease.Tenants.Account_Name,
                                  Account_Holder = lease.Tenants.Account_Holder,
                                  Account_IBAN = lease.Tenants.Account_IBAN,
                                  Account_Swift = lease.Tenants.Account_Swift,
                                  Account_Bank = lease.Tenants.Account_Bank,
                                  Account_Currency = lease.Tenants.Account_Currency,
                                  Address = lease.Tenants.Address,
                                  Address2 = lease.Tenants.Address2,
                                  Locality = lease.Tenants.Locality,
                                  Region = lease.Tenants.Region,
                                  PostalCode = lease.Tenants.PostalCode,
                                  Country = lease.Tenants.Country,
                                  CountryCode = lease.Tenants.CountryCode,
                                  Unit = lease.Tenants.Unit,
                                  Pets = lease.Tenants.Pets.Where(x => x.IsDeleted != true).Select(p => new PetDto
                                  {
                                      PetId = p.PetId,
                                      Name = p.Name,
                                      Breed = p.Breed,
                                      Type = p.Type,
                                      Quantity = p.Quantity,
                                      PictureName = p.Picture,
                                      Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Pets_Picture_" + p.PetId + Path.GetExtension(p.Picture)}"
                                  }).ToList(),
                                  Vehicles = lease.Tenants.Vehicle.Where(x => x.IsDeleted != true).Select(v => new VehicleDto
                                  {
                                      VehicleId = v.VehicleId,
                                      Manufacturer = v.Manufacturer,
                                      ModelName = v.ModelName,
                                      ModelVariant = v.ModelVariant,
                                      Year = v.Year
                                  }).ToList(),
                                  Dependent = lease.Tenants.TenantDependent.Where(x => x.IsDeleted != true).Select(d => new TenantDependentDto
                                  {
                                      TenantDependentId = d.TenantDependentId,
                                      TenantId = d.TenantId,
                                      FirstName = d.FirstName,
                                      LastName = d.LastName,
                                      EmailAddress = d.EmailAddress,
                                      PhoneNumber = d.PhoneNumber,
                                      DOB = d.DOB,
                                      Relation = d.Relation
                                  }).ToList(),
                                  CoTenant = lease.Tenants.CoTenant.Where(x => x.IsDeleted != true).Select(c => new CoTenantDto
                                  {
                                      CoTenantId = c.CoTenantId,
                                      TenantId = c.TenantId,
                                      FirstName = c.FirstName,
                                      LastName = c.LastName,
                                      EmailAddress = c.EmailAddress,
                                      PhoneNumber = c.PhoneNumber,
                                      Address = c.Address,
                                      Unit = c.Unit,
                                      District = c.District,
                                      Region = c.Region,
                                      PostalCode = c.PostalCode,
                                      Country = c.Country
                                  }).ToList()
                              } : null
                          }).ToListAsync();
        }

        private async Task<List<InvoiceDto>> GetInvoicesByOwnerId(int ownerId)
        {
            return await (from asset in _db.Assets
                          from lease in _db.Lease.Where(x => x.AssetId == asset.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                          from invoice in _db.Invoices.Where(x => x.LeaseId == lease.LeaseId && x.IsDeleted != true).DefaultIfEmpty()
                          from t in _db.Tenant.Where(x => x.TenantId == invoice.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                          from o in _db.Owner.Where(x => x.OwnerId == invoice.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                          where asset.OwnerId == ownerId && asset.IsDeleted != true
                          select new InvoiceDto
                          {
                              InvoiceId = invoice != null ? invoice.InvoiceId : 0,
                              OwnerId = invoice != null ? invoice.OwnerId : 0,
                              OwnerName = o != null ? o.FirstName + " " + o.LastName : string.Empty,
                              TenantId = invoice != null ? invoice.TenantId : 0,
                              TenantName = t != null ? t.FirstName + " " + t.LastName : string.Empty,
                              InvoiceCreatedDate = invoice != null ? (DateTime?)invoice.InvoiceCreatedDate ?? DateTime.MinValue : DateTime.MinValue,
                              InvoicePaid = invoice != null ? (bool?)invoice.InvoicePaid ?? false : false,
                              RentAmount = invoice != null ? (decimal?)invoice.RentAmount ?? 0.0m : 0.0m,
                              LeaseId = lease != null ? lease.LeaseId : 0,
                              InvoiceDate = invoice != null ? (DateTime?)invoice.InvoiceDate ?? DateTime.MinValue : DateTime.MinValue,
                              InvoicePaidToOwner = invoice != null ? (bool?)invoice.InvoicePaidToOwner ?? false : false,
                              AddedBy = invoice != null ? invoice.AddedBy : string.Empty
                          }).ToListAsync();
        }

        private async Task<List<TaskRequestDto>> GetTaskRequestsByOwnerId(int ownerId)
        {
            return await (from asset in _db.Assets
                          from task in _db.TaskRequest.Where(t => t.AssetId == asset.AssetId && t.IsDeleted != true).DefaultIfEmpty()
                          from unit in _db.AssetsUnits.Where(u => u.UnitId == task.UnitId && u.IsDeleted != true).DefaultIfEmpty()
                          from tenant in _db.Tenant.Where(tn => tn.TenantId == task.TenantId && tn.IsDeleted != true).DefaultIfEmpty()
                          from owner in _db.Owner.Where(o => o.OwnerId == ownerId && o.IsDeleted != true).DefaultIfEmpty()
                          where asset.OwnerId == ownerId && asset.IsDeleted != true
                          select new TaskRequestDto
                          {
                              TaskRequestId = task != null ? task.TaskRequestId : 0,
                              Type = task != null ? task.Type : string.Empty,
                              Subject = task != null ? task.Subject : string.Empty,
                              Description = task != null ? task.Description : string.Empty,
                              IsOneTimeTask = task != null ? task.IsOneTimeTask : false,
                              IsRecurringTask = task != null ? task.IsRecurringTask : false,
                              StartDate = task != null ? task.StartDate : DateTime.MinValue,
                              EndDate = task != null ? task.EndDate : DateTime.MinValue,
                              Frequency = task != null ? task.Frequency : string.Empty,
                              DueDays = task != null ? task.DueDays : 0,
                              IsTaskRepeat = task != null ? task.IsTaskRepeat : false,
                              DueDate = task != null ? task.DueDate : DateTime.MinValue,
                              Status = task != null ? task.Status : string.Empty,
                              Priority = task != null ? task.Priority : string.Empty,
                              Assignees = task != null ? task.Assignees : string.Empty,
                              IsNotifyAssignee = task != null ? task.IsNotifyAssignee : false,
                              AssetId = asset.AssetId,
                              Asset = asset.BuildingNo + "-" + asset.BuildingName,
                              UnitId = task != null ? task.UnitId : 0,
                              Unit = unit != null ? unit.UnitName : string.Empty,
                              TaskRequestFileName = task != null ? task.TaskRequestFile : string.Empty,
                              TaskRequestFile = task != null ? $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Task_TaskRequestFile_" + task.TaskRequestId + Path.GetExtension(task.TaskRequestFile)}" : string.Empty,
                              OwnerId = ownerId,
                              Owner = owner != null ? owner.FirstName + " " + owner.LastName : string.Empty,
                              IsNotifyOwner = task != null ? task.IsNotifyOwner : false,
                              TenantId = task != null ? task.TenantId : 0,
                              Tenant = tenant != null ? tenant.FirstName + " " + tenant.LastName : string.Empty,
                              IsNotifyTenant = task != null ? task.IsNotifyTenant : false,
                              HasPermissionToEnter = task != null ? task.HasPermissionToEnter : false,
                              EntryNotes = task != null ? task.EntryNotes : string.Empty,
                              VendorId = task != null ? task.VendorId : 0,
                              ApprovedByOwner = task != null ? task.ApprovedByOwner : false,
                              PartsAndLabor = task != null ? task.PartsAndLabor : false,
                              AddedBy = task != null ? task.AddedBy : string.Empty,
                              LineItems = task != null ? (from item in _db.LineItem
                                                          from ca in _db.ChartAccount.Where(c => c.ChartAccountId == item.ChartAccountId && c.IsDeleted != true).DefaultIfEmpty()
                                                          where item.TaskRequestId == task.TaskRequestId && item.IsDeleted != true
                                                          select new LineItemDto
                                                          {
                                                              LineItemId = item.LineItemId,
                                                              TaskRequestId = item.TaskRequestId,
                                                              Quantity = item.Quantity,
                                                              Price = item.Price,
                                                              ChartAccountId = item.ChartAccountId,
                                                              AccountName = ca != null ? ca.Name : string.Empty,
                                                              Memo = item.Memo
                                                          }).ToList() : new List<LineItemDto>()
                          }).ToListAsync();
        }

        #endregion

        public async Task<TenantDataDto> GetTenantDataById(int tenantId)
        {
            try
            {
                var tenant = await _db.Tenant
                    .Where(t => t.TenantId == tenantId && t.IsDeleted != true)
                    .Select(t => new TenantDataDto
                    {
                        TenantId = t.TenantId,
                        PictureName = t.Picture,
                        Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Picture_" + t.TenantId + Path.GetExtension(t.Picture)}",
                        DocumentName = t.Document,
                        Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Document_" + t.TenantId + Path.GetExtension(t.Document)}",
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        EmailAddress = t.EmailAddress,
                        PhoneNumber = t.PhoneNumber,
                        EmergencyContactInfo = t.EmergencyContactInfo,
                        LeaseAgreementId = t.LeaseAgreementId,
                        TenantNationality = t.TenantNationality,
                        Gender = t.Gender,
                        DOB = t.DOB,
                        VAT = t.VAT,
                        LegalName = t.LegalName,
                        Account_Name = t.Account_Name,
                        Account_Holder = t.Account_Holder,
                        Account_IBAN = t.Account_IBAN,
                        Account_Swift = t.Account_Swift,
                        Account_Bank = t.Account_Bank,
                        Account_Currency = t.Account_Currency,
                        Address = t.Address,
                        Address2 = t.Address2,
                        Locality = t.Locality,
                        Region = t.Region,
                        PostalCode = t.PostalCode,
                        Country = t.Country,
                        CountryCode = t.CountryCode,
                        Unit = t.Unit,
                        AddedBy = t.AddedBy,
                        AddedDate = t.AddedDate
                    })
                    .FirstOrDefaultAsync();

                if (tenant == null)
                {
                    return null;
                }

                tenant.Pets = await _db.Pets
                    .Where(p => p.TenantId == tenantId && p.IsDeleted != true)
                    .Select(p => new PetDto
                    {
                        PetId = p.PetId,
                        Name = p.Name,
                        Breed = p.Breed,
                        Type = p.Type,
                        Quantity = p.Quantity,
                        PictureName = p.Picture,
                        Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Pets_Picture_" + p.PetId + Path.GetExtension(p.Picture)}"
                    })
                    .ToListAsync();

                tenant.Vehicles = await _db.Vehicle
                    .Where(v => v.TenantId == tenantId && v.IsDeleted != true)
                    .Select(v => new VehicleDto
                    {
                        VehicleId = v.VehicleId,
                        Manufacturer = v.Manufacturer,
                        ModelName = v.ModelName,
                        ModelVariant = v.ModelVariant,
                        Year = v.Year
                    })
                    .ToListAsync();

                tenant.Dependent = await _db.TenantDependent
                    .Where(d => d.TenantId == tenantId && d.IsDeleted != true)
                    .Select(d => new TenantDependentDto
                    {
                        TenantDependentId = d.TenantDependentId,
                        TenantId = d.TenantId,
                        FirstName = d.FirstName,
                        LastName = d.LastName,
                        EmailAddress = d.EmailAddress,
                        PhoneNumber = d.PhoneNumber,
                        DOB = d.DOB,
                        Relation = d.Relation
                    })
                    .ToListAsync();

                tenant.CoTenant = await _db.CoTenant
                    .Where(c => c.TenantId == tenantId && c.IsDeleted != true)
                    .Select(c => new CoTenantDto
                    {
                        CoTenantId = c.CoTenantId,
                        TenantId = c.TenantId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        EmailAddress = c.EmailAddress,
                        PhoneNumber = c.PhoneNumber,
                        Address = c.Address,
                        Unit = c.Unit,
                        District = c.District,
                        Region = c.Region,
                        PostalCode = c.PostalCode,
                        Country = c.Country
                    })
                    .ToListAsync();

                tenant.Leases = await _db.Lease
                    .Where(l => l.TenantsTenantId == tenantId && l.IsDeleted != true)
                    .Select(l => new LeaseDto
                    {
                        LeaseId = l.LeaseId,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        IsSigned = l.IsSigned,
                        SelectedProperty = l.SelectedProperty,
                        SelectedUnit = l.SelectedUnit,
                        SignatureImagePath = l.SignatureImagePath,
                        IsFixedTerm = l.IsFixedTerm,
                        IsMonthToMonth = l.IsMonthToMonth,
                        HasSecurityDeposit = l.HasSecurityDeposit,
                        LateFeesPolicy = l.LateFeesPolicy,
                        TenantId = l.TenantsTenantId,
                        AppTenantId = l.AppTenantId,
                        RentCharges = l.RentCharges.Where(x => x.IsDeleted != true).Select(rc => new RentChargeDto
                        {
                            RentChargeId = rc.RentChargeId,
                            Amount = rc.Amount,
                            Description = rc.Description,
                            RentDate = rc.RentDate,
                            RentPeriod = rc.RentPeriod
                        }).ToList(),
                        FeeCharge = l.FeeCharge.Where(x => x.IsDeleted != true).Select(fc => new FeeChargeDto
                        {
                            FeeChargeId = fc.FeeChargeId,
                            ChargeLatefeeActive = fc.ChargeLatefeeActive,
                            UsePropertyDefaultStructure = fc.UsePropertyDefaultStructure,
                            SpecifyLateFeeStructure = fc.SpecifyLateFeeStructure,
                            DueDays = fc.DueDays,
                            Frequency = fc.Frequency,
                            CalculateFee = fc.CalculateFee,
                            Amount = fc.Amount,
                            ChartAccountId = fc.ChartAccountId,
                            Description = fc.Description,
                            IsSendARemainder = fc.IsSendARemainder,
                            IsNotifyTenants = fc.IsNotifyTenants,
                            IsEnableSms = fc.IsEnableSms,
                            IsChargeLateFee = fc.IsChargeLateFee,
                            IsMonthlyLimit = fc.IsMonthlyLimit,
                            IsDailyLimit = fc.IsDailyLimit,
                            IsMinimumBalance = fc.IsMinimumBalance,
                            IsChargeLateFeeonSpecific = fc.IsChargeLateFeeonSpecific,
                            FeeDate = fc.FeeDate
                        }).FirstOrDefault()

                    })
                    .ToListAsync();

                    tenant.Invoices = await (from lease in _db.Lease
                                         from invoice in _db.Invoices.Where(x => x.LeaseId == lease.LeaseId && x.IsDeleted != true).DefaultIfEmpty()
                                         from t in _db.Tenant.Where(x => x.TenantId == invoice.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                         from o in _db.Owner.Where(x => x.OwnerId == invoice.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                         where lease.TenantsTenantId == tenantId && lease.IsDeleted != true
                                         select new InvoiceDto
                                         {
                                             InvoiceId = invoice.InvoiceId,
                                             OwnerId = invoice.OwnerId,
                                             OwnerName = o.FirstName + " " + o.LastName,
                                             TenantId = invoice.TenantId,
                                             TenantName = t.FirstName + " " + t.LastName,
                                             InvoiceCreatedDate = invoice.InvoiceCreatedDate,
                                             InvoicePaid = invoice.InvoicePaid,
                                             RentAmount = invoice.RentAmount,
                                             LeaseId = invoice.LeaseId,
                                             InvoiceDate = invoice.InvoiceDate,
                                             InvoicePaidToOwner = invoice.InvoicePaidToOwner,
                                             AddedBy = invoice.AddedBy
                                         }).ToListAsync();

                tenant.Assets = await (from lease in _db.Lease
                                       from asset in _db.Assets.Where(x => x.AssetId == lease.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                       where lease.TenantsTenantId == tenantId && lease.IsDeleted != true
                                       select new AssetDTO
                                       {
                                           AssetId = asset.AssetId,
                                           SelectedPropertyType = asset.SelectedPropertyType,
                                           SelectedBankAccountOption = asset.SelectedBankAccountOption,
                                           SelectedReserveFundsOption = asset.SelectedReserveFundsOption,
                                           SelectedSubtype = asset.SelectedSubtype,
                                           SelectedOwnershipOption = asset.SelectedOwnershipOption,
                                           BuildingNo = asset.BuildingNo,
                                           BuildingName = asset.BuildingName,
                                           Street1 = asset.Street1,
                                           Street2 = asset.Street2,
                                           City = asset.City,
                                           Country = asset.Country,
                                           Zipcode = asset.Zipcode,
                                           State = asset.State,
                                           AppTid = asset.AppTenantId,
                                           PictureFileName = asset.Image,
                                           Image = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Assets_Image_" + asset.AssetId + Path.GetExtension(asset.Image)}",
                                           AddedBy = asset.AddedBy,
                                           AddedDate = asset.AddedDate,
                                           ModifiedDate = asset.ModifiedDate,
                                           Units = asset.Units.Where(x => x.IsDeleted != true).Select(unit => new UnitDTO
                                           {
                                               UnitId = unit.UnitId,
                                               AssetId = unit.AssetId,
                                               UnitName = unit.UnitName,
                                               Bath = unit.Bath,
                                               Beds = unit.Beds,
                                               Rent = unit.Rent,
                                               Size = unit.Size
                                           }).ToList()
                                       }).ToListAsync();

                tenant.TaskRequest = await (from task in _db.TaskRequest
                                            from asset in _db.Assets.Where(x => x.AssetId == task.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                            from unit in _db.AssetsUnits.Where(x => x.UnitId == task.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                            from owner in _db.Owner.Where(x => x.OwnerId == task.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                            where task.TenantId == tenantId && task.IsDeleted != true
                                            select new TaskRequestDto
                                            {
                                                TaskRequestId = task.TaskRequestId,
                                                Type = task.Type,
                                                Subject = task.Subject,
                                                Description = task.Description,
                                                IsOneTimeTask = task.IsOneTimeTask,
                                                IsRecurringTask = task.IsRecurringTask,
                                                StartDate = task.StartDate,
                                                EndDate = task.EndDate,
                                                Frequency = task.Frequency,
                                                DueDays = task.DueDays,
                                                IsTaskRepeat = task.IsTaskRepeat,
                                                DueDate = task.DueDate,
                                                Status = task.Status,
                                                Priority = task.Priority,
                                                Assignees = task.Assignees,
                                                IsNotifyAssignee = task.IsNotifyAssignee,
                                                AssetId = task.AssetId,
                                                Asset = asset.BuildingNo + "-" + asset.BuildingName,
                                                UnitId = task.UnitId,
                                                Unit = unit.UnitName,
                                                TaskRequestFileName = task.TaskRequestFile,
                                                TaskRequestFile = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Task_TaskRequestFile_" + task.TaskRequestId + Path.GetExtension(task.TaskRequestFile)}",
                                                OwnerId = task.OwnerId,
                                                Owner = owner.FirstName + " " + owner.LastName,
                                                IsNotifyOwner = task.IsNotifyOwner,
                                                TenantId = task.TenantId,
                                                Tenant = tenant.FirstName + " " + tenant.LastName,
                                                IsNotifyTenant = task.IsNotifyTenant,
                                                HasPermissionToEnter = task.HasPermissionToEnter,
                                                EntryNotes = task.EntryNotes,
                                                VendorId = task.VendorId,
                                                ApprovedByOwner = task.ApprovedByOwner,
                                                PartsAndLabor = task.PartsAndLabor,
                                                AddedBy = task.AddedBy,
                                                LineItems = (from item in _db.LineItem
                                                             from ca in _db.ChartAccount.Where(x => x.ChartAccountId == item.ChartAccountId && x.IsDeleted != true).DefaultIfEmpty()
                                                             where item.TaskRequestId == task.TaskRequestId && item.IsDeleted != true
                                                             select new LineItemDto
                                                             {
                                                                 LineItemId = item.LineItemId,
                                                                 TaskRequestId = item.TaskRequestId,
                                                                 Quantity = item.Quantity,
                                                                 Price = item.Price,
                                                                 ChartAccountId = item.ChartAccountId,
                                                                 AccountName = ca.Name,
                                                                 Memo = item.Memo
                                                             }).ToList()
                                            }).ToListAsync();

                return tenant;
            }
            catch (Exception ex)
            {
                // Handle exception (log it, etc.)
                throw;
            }
        }

        #region Calendar

        public async Task<List<CalendarEvent>> GetCalendarEventsAsync(CalendarFilterModel filter)
        {
            try
            {
                var eventsList = new List<CalendarEvent>();

                var taskRequestsQuery = _db.TaskRequest
                    .Where(x => !x.IsDeleted && x.AddedBy == filter.UserId)
                    .Select(x => new CalendarEvent
                    {
                        Title = $"TaskRequest: {x.Subject}",
                        Date = x.DueDate,
                        BoxColor = "#e2d8f7",
                        TextColor = "#504c83",
                        TenantId = x.TenantId
                    });

                eventsList.AddRange(await taskRequestsQuery.ToListAsync());

                IQueryable<CalendarEvent> moveInLeasesQuery = null;
                IQueryable<CalendarEvent> moveOutLeasesQuery = null;

                if (filter.StartDateFilter.HasValue)
                {
                    moveInLeasesQuery = _db.Lease
                        .Where(x => !x.IsDeleted && x.StartDate >= filter.StartDateFilter.Value && x.AddedBy == filter.UserId)
                        .SelectMany(lease => _db.Tenant.Where(t => t.TenantId == lease.TenantsTenantId && t.AddedBy == filter.UserId && t.IsDeleted != true).DefaultIfEmpty(), (lease, tenant) => new
                        {
                            Tenant = tenant,
                            Lease = lease
                        })
                        .Select(x => new CalendarEvent
                        {
                            Title = $"Move In: {x.Tenant.FirstName} {x.Tenant.LastName}",
                            Date = x.Lease.StartDate,
                            BoxColor = "#e5f7dd",
                            TextColor = "#4ea76a",
                            TenantId = x.Lease.TenantsTenantId
                        });
                }
                else
                {
                    moveInLeasesQuery = _db.Lease
                        .Where(x => !x.IsDeleted && x.AddedBy == filter.UserId)
                        .SelectMany(lease => _db.Tenant.Where(t => t.TenantId == lease.TenantsTenantId && t.AddedBy == filter.UserId && t.IsDeleted != true).DefaultIfEmpty(), (lease, tenant) => new
                        {
                            Tenant = tenant,
                            Lease = lease
                        })
                        .Select(x => new CalendarEvent
                        {
                            Title = $"Move In: {x.Tenant.FirstName} {x.Tenant.LastName}",
                            Date = x.Lease.StartDate,
                            BoxColor = "#e5f7dd",
                            TextColor = "#4ea76a",
                            TenantId = x.Lease.TenantsTenantId
                        });
                }

                eventsList.AddRange(await moveInLeasesQuery.ToListAsync());


                if (filter.EndDateFilter.HasValue)
                {
                    moveOutLeasesQuery = _db.Lease
                        .Where(x => !x.IsDeleted && x.EndDate <= filter.EndDateFilter.Value && x.AddedBy == filter.UserId)
                        .SelectMany(lease => _db.Tenant.Where(t => t.TenantId == lease.TenantsTenantId && t.AddedBy == filter.UserId && t.IsDeleted != true).DefaultIfEmpty(), (lease, tenant) => new
                        {
                            Tenant = tenant,
                            Lease = lease
                        })
                        .Select(x => new CalendarEvent
                        {
                            Title = $"Move Out: {x.Tenant.FirstName} {x.Tenant.LastName}",
                            Date = x.Lease.EndDate,
                            BoxColor = "#f5dde2",
                            TextColor = "#974249",
                            TenantId = x.Lease.TenantsTenantId
                        });
                }
                else
                {
                    moveOutLeasesQuery = _db.Lease
                        .Where(x => !x.IsDeleted && x.AddedBy == filter.UserId)
                        .SelectMany(lease => _db.Tenant.Where(t => t.TenantId == lease.TenantsTenantId && t.AddedBy == filter.UserId && t.IsDeleted != true).DefaultIfEmpty(), (lease, tenant) => new
                        {
                            Tenant = tenant,
                            Lease = lease
                        })
                        .Select(x => new CalendarEvent
                        {
                            Title = $"Move Out: {x.Tenant.FirstName} {x.Tenant.LastName}",
                            Date = x.Lease.EndDate,
                            BoxColor = "#f5dde2",
                            TextColor = "#974249",
                            TenantId = x.Lease.TenantsTenantId
                        });
                }
                eventsList.AddRange(await moveOutLeasesQuery.ToListAsync());


                if (filter.TenantFilter != null && filter.TenantFilter.Any())
                {
                    var tenantIds = filter.TenantFilter.Select(id => int.Parse(id)).ToList();
                    eventsList = eventsList.Where(x => tenantIds.Contains(x.TenantId.GetValueOrDefault())).ToList();
                }

                return eventsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping calendar: {ex.Message}");
                throw;
            }
        }

        public async Task<List<OccupancyOverviewEvents>> GetOccupancyOverviewEventsAsync(CalendarFilterModel filter)
        {
            try
            {
                var eventsList = new List<OccupancyOverviewEvents>();

                var query = _db.Lease.AsQueryable();

                if (filter.TenantFilter != null && filter.TenantFilter.Any())
                {
                    var tenantIds = filter.TenantFilter.Select(id => int.Parse(id)).ToList();
                    query = query.Where(x => tenantIds.Contains(x.TenantsTenantId) && x.AddedBy == filter.UserId && x.IsDeleted != true);
                }

                if (filter.StartDateFilter.HasValue)
                {
                    query = query.Where(x => x.StartDate >= filter.StartDateFilter.Value && x.AddedBy == filter.UserId && x.IsDeleted != true);
                }

                if (filter.EndDateFilter.HasValue)
                {
                    query = query.Where(x => x.EndDate <= filter.EndDateFilter.Value && x.AddedBy == filter.UserId && x.IsDeleted != true);
                }

                var leases = await query.Select(x => new OccupancyOverviewEvents
                {
                    Id = x.LeaseId,
                    ResourceTitle = x.SelectedProperty,
                    ResourceId = x.SelectedProperty,
                    Title = x.Tenants.FirstName + " " + x.Tenants.LastName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                }).ToListAsync();

                return leases;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping occupancy calendar: {ex.Message}");
                throw;
            }
        }

        public async Task<LeaseDataDto> GetLeaseDataByIdAsync(int id)
        {
            try
            {
                var lease = (from l in _db.Lease
                             from t in _db.Tenant.Where(x => x.TenantId == l.TenantsTenantId && x.IsDeleted != true).DefaultIfEmpty()
                             where l.LeaseId == id && l.IsDeleted != true
                             select new LeaseDataDto
                             {
                                 Id = l.LeaseId,
                                 Asset = l.SelectedProperty,
                                 AssetUnit = l.SelectedUnit,
                                 Tenant = t.FirstName + " " + t.LastName,
                                 StartDate = l.StartDate,
                                 EndDate = l.EndDate
                             }).FirstOrDefault();

                return lease;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping lease data: {ex.Message}");
                throw;
            }
        }

        #endregion


        #region Vendor Category

        public async Task<List<VendorCategory>> GetVendorCategoriesAsync()
        {
            try
            {
                var result = await (from v in _db.VendorCategory
                                    where v.IsDeleted != true
                                    select new VendorCategory
                                    {
                                        VendorCategoryId = v.VendorCategoryId,
                                        Name = v.Name,
                                        AddedBy = v.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendor Categories: {ex.Message}");
                throw;
            }
        }

        public async Task<List<VendorCategory>> GetVendorCategoriesDllAsync(Filter filter)
        {
            try
            {
                var result = await (from v in _db.VendorCategory
                                    where v.IsDeleted != true && v.AddedBy == filter.AddedBy
                                    select new VendorCategory
                                    {
                                        VendorCategoryId = v.VendorCategoryId,
                                        Name = v.Name,
                                        AddedBy = v.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendor Categories: {ex.Message}");
                throw;
            }
        }

        public async Task<VendorCategory> GetVendorCategoryByIdAsync(int id)
        {
            try
            {
                var result = await (from v in _db.VendorCategory
                                    where v.VendorCategoryId == id && v.IsDeleted != true
                                    select new VendorCategory
                                    {
                                        VendorCategoryId = v.VendorCategoryId,
                                        Name = v.Name,
                                        AddedBy = v.AddedBy,

                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendor Category: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveVendorCategoryAsync(VendorCategory model)
        {
            var vendorCategory = _db.VendorCategory.FirstOrDefault(x => x.VendorCategoryId == model.VendorCategoryId && x.IsDeleted != true);

            if (vendorCategory == null)
                vendorCategory = new VendorCategory();

            vendorCategory.VendorCategoryId = model.VendorCategoryId;
            vendorCategory.Name = model.Name;

            if (vendorCategory.VendorCategoryId > 0)
            {
                vendorCategory.ModifiedBy = model.AddedBy;
                vendorCategory.ModifiedDate = DateTime.Now;
                _db.VendorCategory.Update(vendorCategory);
            }
            else
            {
                vendorCategory.AddedBy = model.AddedBy;
                vendorCategory.AddedDate = DateTime.Now;
                _db.VendorCategory.Add(vendorCategory);
            }

            var result = await _db.SaveChangesAsync();


            return result > 0;

        }

        public async Task<bool> DeleteVendorCategoryAsync(int id)
        {
            var vendorCategory = await _db.VendorCategory.FindAsync(id);
            if (vendorCategory == null) return false;

            vendorCategory.IsDeleted = true;
            _db.VendorCategory.Update(vendorCategory);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
        }

        #endregion

        #region Vendor Classification

        public async Task<List<VendorClassification>> GetVendorClassificationsAsync()
        {
            try
            {
                var result = await (from v in _db.VendorClassification
                                    where v.IsDeleted != true
                                    select new VendorClassification
                                    {
                                        VendorClassificationId = v.VendorClassificationId,
                                        Name = v.Name,
                                        AddedBy = v.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendor Classifications: {ex.Message}");
                throw;
            }
        }

        public async Task<List<VendorClassification>> GetVendorClassificationsDllAsync(Filter filter)
        {
            try
            {
                var result = await (from v in _db.VendorClassification
                                    where v.IsDeleted != true && v.AddedBy == filter.AddedBy
                                    select new VendorClassification
                                    {
                                        VendorClassificationId = v.VendorClassificationId,
                                        Name = v.Name,
                                        AddedBy = v.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendor Classifications: {ex.Message}");
                throw;
            }
        }

        public async Task<VendorClassification> GetVendorClassificationByIdAsync(int id)
        {
            try
            {
                var result = await (from v in _db.VendorClassification
                                    where v.VendorClassificationId == id && v.IsDeleted != true
                                    select new VendorClassification
                                    {
                                        VendorClassificationId = v.VendorClassificationId,
                                        Name = v.Name,
                                        AddedBy = v.AddedBy,

                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendor Classification: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveVendorClassificationAsync(VendorClassification model)
        {
            var vendorClassification = _db.VendorClassification.FirstOrDefault(x => x.VendorClassificationId == model.VendorClassificationId && x.IsDeleted != true);

            if (vendorClassification == null)
                vendorClassification = new VendorClassification();

            vendorClassification.VendorClassificationId = model.VendorClassificationId;
            vendorClassification.Name = model.Name;

            if (vendorClassification.VendorClassificationId > 0)
            {
                vendorClassification.ModifiedBy = model.AddedBy;
                vendorClassification.ModifiedDate = DateTime.Now;
                _db.VendorClassification.Update(vendorClassification);
            }
            else
            {
                vendorClassification.AddedBy = model.AddedBy;
                vendorClassification.AddedDate = DateTime.Now;
                _db.VendorClassification.Add(vendorClassification);
            }

            var result = await _db.SaveChangesAsync();


            return result > 0;

        }

        public async Task<bool> DeleteVendorClassificationAsync(int id)
        {
            var vendorClassification = await _db.VendorClassification.FindAsync(id);
            if (vendorClassification == null) return false;

            vendorClassification.IsDeleted = true;
            _db.VendorClassification.Update(vendorClassification);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
        }

        #endregion

        #region Vendor 

        public async Task<List<VendorDto>> GetVendorsAsync()
        {
            try
            {
                var result = await (from v in _db.Vendor
                                    from vo in _db.VendorOrganization.Where(x => x.VendorId == v.VendorId && x.IsDeleted != true).DefaultIfEmpty()
                                    where v.IsDeleted != true
                                    select new VendorDto
                                    {
                                        VendorId = v.VendorId,
                                        FirstName = v.FirstName,
                                        MI = v.MI,
                                        LastName = v.LastName,
                                        Company = v.Company,
                                        JobTitle = v.JobTitle,
                                        Notes = v.Notes,
                                        PictureName = v.Picture,
                                        PictureUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Vendor_Picture_" + v.VendorId + Path.GetExtension(v.Picture)}",
                                        Email1 = v.Email1,
                                        Phone1 = v.Phone1,
                                        Email2 = v.Email2,
                                        Phone2 = v.Phone2,
                                        Street1 = v.Street1,
                                        Street2 = v.Street2,
                                        District = v.District,
                                        City = v.City,
                                        State = v.State,
                                        Country = v.Country,
                                        AlterStreet1 = v.AlterStreet1,
                                        AlterStreet2 = v.AlterStreet2,
                                        AlterDistrict = v.AlterDistrict,
                                        AlterCity = v.AlterCity,
                                        AlterState = v.AlterState,
                                        AlterCountry = v.AlterCountry,
                                        VendorClassificationIds = v.VendorClassificationIds,
                                        VendorCategoriesIds = v.VendorCategoriesIds,
                                        HasInsurance = v.HasInsurance,
                                        InsuranceCompany = v.InsuranceCompany,
                                        PolicyNumber = v.PolicyNumber,
                                        Amount = v.Amount,
                                        TaxId = v.TaxId,
                                        AccountName = v.AccountName,
                                        AccountHolder = v.AccountHolder,
                                        AccountIBAN = v.AccountIBAN,
                                        AccountSwift = v.AccountSwift,
                                        AccountBank = v.AccountBank,
                                        AccountCurrency = v.AccountCurrency,
                                        OrganizationName = vo.OrganizationName,
                                        OrganizationDescription = vo.OrganizationDescription,
                                        OrganizationIconName = vo.OrganizationIcon,
                                        OrganizationIconUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Vendor_OrganizationIcon_" + v.VendorId + Path.GetExtension(vo.OrganizationIcon)}",
                                        OrganizationLogoName = vo.OrganizationLogo,
                                        OrganizationLogoUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Vendor_OrganizationLogoName_" + v.VendorId + Path.GetExtension(vo.OrganizationLogo)}",
                                        Website = vo.Website,
                                        AddedBy = v.AddedBy
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendors: {ex.Message}");
                throw;
            }
        }

        public async Task<List<VendorDto>> GetVendorsDllAsync(Filter filter)
        {
            try
            {
                var result = await (from v in _db.Vendor
                                    where v.IsDeleted != true && v.AddedBy == filter.AddedBy
                                    select new VendorDto
                                    {
                                        VendorId = v.VendorId,
                                        FirstName = v.FirstName,
                                        LastName = v.LastName,
                                        AddedBy = v.AddedBy,
                                    })
               .AsNoTracking()
               .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendors: {ex.Message}");
                throw;
            }
        }

        public async Task<VendorDto> GetVendorByIdAsync(int id)
        {
            try
            {
                var result = await (from v in _db.Vendor
                                    from vo in _db.VendorOrganization.Where(x => x.VendorId == v.VendorId && x.IsDeleted != true).DefaultIfEmpty()
                                    where v.VendorId == id && v.IsDeleted != true
                                    select new VendorDto
                                    {
                                        VendorId = v.VendorId,
                                        FirstName = v.FirstName,
                                        MI = v.MI,
                                        LastName = v.LastName,
                                        Company = v.Company,
                                        JobTitle = v.JobTitle,
                                        Notes = v.Notes,
                                        PictureName = v.Picture,
                                        PictureUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Vendor_Picture_" + v.VendorId + Path.GetExtension(v.Picture)}",
                                        Email1 = v.Email1,
                                        Phone1 = v.Phone1,
                                        Email2 = v.Email2,
                                        Phone2 = v.Phone2,
                                        Street1 = v.Street1,
                                        Street2 = v.Street2,
                                        District = v.District,
                                        City = v.City,
                                        State = v.State,
                                        Country = v.Country,
                                        AlterStreet1 = v.AlterStreet1,
                                        AlterStreet2 = v.AlterStreet2,
                                        AlterDistrict = v.AlterDistrict,
                                        AlterCity = v.AlterCity,
                                        AlterState = v.AlterState,
                                        AlterCountry = v.AlterCountry,
                                        VendorClassificationIds = v.VendorClassificationIds,
                                        VendorCategoriesIds = v.VendorCategoriesIds,
                                        HasInsurance = v.HasInsurance,
                                        InsuranceCompany = v.InsuranceCompany,
                                        PolicyNumber = v.PolicyNumber,
                                        Amount = v.Amount,
                                        TaxId = v.TaxId,
                                        AccountName = v.AccountName,
                                        AccountHolder = v.AccountHolder,
                                        AccountIBAN = v.AccountIBAN,
                                        AccountSwift = v.AccountSwift,
                                        AccountBank = v.AccountBank,
                                        AccountCurrency = v.AccountCurrency,
                                        OrganizationName = vo.OrganizationName,
                                        OrganizationDescription = vo.OrganizationDescription,
                                        OrganizationIconName = vo.OrganizationIcon,
                                        OrganizationIconUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Vendor_OrganizationIcon_" + v.VendorId + Path.GetExtension(vo.OrganizationIcon)}",
                                        OrganizationLogoName = vo.OrganizationLogo,
                                        OrganizationLogoUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Vendor_OrganizationLogo_" + v.VendorId + Path.GetExtension(vo.OrganizationLogo)}",
                                        Website = vo.Website,
                                        AddedBy = v.AddedBy
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Vendor : {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveVendorAsync(VendorDto model)
        {
            var vendor = _db.Vendor.FirstOrDefault(x => x.VendorId == model.VendorId && x.IsDeleted != true);

            if (vendor == null)
                vendor = new Vendor();

            vendor.VendorId = model.VendorId;
            vendor.FirstName = model.FirstName;
            vendor.MI = model.MI;
            vendor.LastName = model.LastName;
            vendor.Company = model.Company;
            vendor.JobTitle = model.JobTitle;
            vendor.Notes = model.Notes;
            if (model.Picture != null)
            {
                vendor.Picture = model.Picture.FileName;
            }
            vendor.Email1 = model.Email1;
            vendor.Phone1 = model.Phone1;
            vendor.Email2 = model.Email2;
            vendor.Phone2 = model.Phone2;
            vendor.Street1 = model.Street1;
            vendor.Street2 = model.Street2;
            vendor.District = model.District;
            vendor.City = model.City;
            vendor.State = model.State;
            vendor.Country = model.Country;
            vendor.AlterStreet1 = model.AlterStreet1;
            vendor.AlterStreet2 = model.AlterStreet2;
            vendor.AlterDistrict = model.AlterDistrict;
            vendor.AlterCity = model.AlterCity;
            vendor.AlterState = model.AlterState;
            vendor.AlterCountry = model.AlterCountry;
            vendor.VendorClassificationIds = model.VendorClassificationIds;
            vendor.VendorCategoriesIds = model.VendorCategoriesIds;
            vendor.HasInsurance = model.HasInsurance;
            vendor.InsuranceCompany = model.HasInsurance == true ? model.InsuranceCompany : "";
            vendor.PolicyNumber = model.HasInsurance == true ? model.PolicyNumber : "";
            vendor.Amount = model.HasInsurance == true ? model.Amount : 0;
            vendor.TaxId = model.TaxId;
            vendor.AccountName = model.AccountName;
            vendor.AccountHolder = model.AccountHolder;
            vendor.AccountIBAN = model.AccountIBAN;
            vendor.AccountSwift = model.AccountSwift;
            vendor.AccountBank = model.AccountBank;
            vendor.AccountCurrency = model.AccountCurrency;

            if (vendor.VendorId > 0)
            {
                vendor.ModifiedBy = model.AddedBy;
                vendor.ModifiedDate = DateTime.Now;
                _db.Vendor.Update(vendor);
            }
            else
            {
                vendor.AddedBy = model.AddedBy;
                vendor.AddedDate = DateTime.Now;
                _db.Vendor.Add(vendor);
            }

            var result1 = await _db.SaveChangesAsync();

            var vendorOrganization = _db.VendorOrganization.FirstOrDefault(x => x.VendorId == vendor.VendorId && x.IsDeleted != true);

            if (vendorOrganization == null)
            {
                vendorOrganization = new VendorOrganization();

                vendorOrganization.VendorId = vendor.VendorId;
                vendorOrganization.OrganizationName = model.OrganizationName;
                vendorOrganization.OrganizationDescription = model.OrganizationDescription;
                vendorOrganization.AddedBy = model.AddedBy;
                vendorOrganization.AddedDate = DateTime.Now;
                if (model.OrganizationIcon != null)
                {
                    vendorOrganization.OrganizationIcon = model.OrganizationIcon.FileName;
                }
                if (model.OrganizationLogo != null)
                {
                    vendorOrganization.OrganizationLogo = model.OrganizationLogo.FileName;
                }
                vendorOrganization.Website = model.Website;
                await _db.VendorOrganization.AddAsync(vendorOrganization);
            }
            else
            {
                vendorOrganization.OrganizationName = model.OrganizationName;
                vendorOrganization.OrganizationDescription = model.OrganizationDescription;

                if (model.OrganizationIcon != null)
                {
                    vendorOrganization.OrganizationIcon = model.OrganizationIcon.FileName;
                }
                if (model.OrganizationLogo != null)
                {
                    vendorOrganization.OrganizationLogo = model.OrganizationLogo.FileName;
                }
                vendorOrganization.Website = model.Website;
                vendorOrganization.ModifiedBy = model.AddedBy;
                vendorOrganization.ModifiedDate = DateTime.Now;
                _db.VendorOrganization.Update(vendorOrganization);
            }

            var result2 = await _db.SaveChangesAsync();

            if (model.Picture != null)
            {
                var ext = Path.GetExtension(model.Picture.FileName);
                var res = await _googleCloudStorageService.UploadImageAsync(model.Picture, "Vendor_Picture_" + vendor.VendorId + ext);
            }

            if (model.OrganizationIcon != null)
            {
                var ext1 = Path.GetExtension(model.OrganizationIcon.FileName);
                var res1 = await _googleCloudStorageService.UploadImageAsync(model.OrganizationIcon, "Vendor_OrganizationIcon_" + vendor.VendorId + ext1);
            }

            if (model.OrganizationLogo != null)
            {
                var ext2 = Path.GetExtension(model.OrganizationLogo.FileName);
                var res2 = await _googleCloudStorageService.UploadImageAsync(model.OrganizationLogo, "Vendor_OrganizationLogo_" + vendor.VendorId + ext2);
            }

            return result1 > 0 && result2 > 0;

        }

        public async Task<APIResponse> DeleteVendorAsync(int id)
        {
            var response = new APIResponse();

            try
            {
                var isTaskRefrence = await _db.TaskRequest.AnyAsync(x => x.VendorId == id && x.IsDeleted != true);
                if (isTaskRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Vendor has refrence in task request.");
                    return response;
                }

                var vendor = await _db.Vendor.FirstOrDefaultAsync(x => x.VendorId == id && x.IsDeleted != true);
                if (vendor == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Vendor not found.");
                    return response;
                }

                vendor.IsDeleted = true;
                _db.Vendor.Update(vendor);

                var vendorOrganization = await _db.VendorOrganization.FirstOrDefaultAsync(x => x.VendorId == id && x.IsDeleted != true);
                if (vendorOrganization != null)
                {
                    vendorOrganization.IsDeleted = true;
                    _db.VendorOrganization.Update(vendorOrganization);
                }

                 await _db.SaveChangesAsync();

                 response.StatusCode = HttpStatusCode.OK;
                 response.Result = "Vendor deleted successfully.";
                
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
                throw new Exception(string.Join(", ", response.ErrorMessages));
            }

            return response;
        }


        #endregion

        #region Applications 

        public async Task<List<ApplicationsDto>> GetApplicationsAsync()
        {
            try
            {
                var result = await (from a in _db.Applications
                                    from p in _db.Assets.Where(x => x.AssetId == a.PropertyId && x.IsDeleted != true).DefaultIfEmpty()
                                    where a.IsDeleted != true
                                    select new ApplicationsDto
                                    {
                                        ApplicationId = a.ApplicationId,
                                        FirstName = a.FirstName,
                                        MiddleName = a.MiddleName,
                                        LastName = a.LastName,
                                        SSN = a.SSN,
                                        ITIN = a.ITIN,
                                        DOB = a.DOB,
                                        Email = a.Email,
                                        PhoneNumber = a.PhoneNumber,
                                        Gender = a.Gender,
                                        MaritalStatus = a.MaritalStatus,
                                        DriverLicenseState = a.DriverLicenseState,
                                        DriverLicenseNumber = a.DriverLicenseNumber,
                                        Note = a.Note,
                                        Address = a.Address,
                                        LandlordName = a.LandlordName,
                                        ContactEmail = a.ContactEmail,
                                        ContactPhoneNumber = a.ContactPhoneNumber,
                                        MoveInDate = a.MoveInDate,
                                        MonthlyPayment = a.MonthlyPayment,
                                        JobType = a.JobType,
                                        JobTitle = a.JobTitle,
                                        AnnualIncome = a.AnnualIncome,
                                        CompanyName = a.CompanyName,
                                        WorkStatus = a.WorkStatus,
                                        StartDate = a.StartDate,
                                        EndDate = a.EndDate,
                                        SupervisorName = a.SupervisorName,
                                        SupervisorEmail = a.SupervisorEmail,
                                        SupervisorPhoneNumber = a.SupervisorPhoneNumber,
                                        EmergencyFirstName = a.EmergencyFirstName,
                                        EmergencyLastName = a.EmergencyLastName,
                                        EmergencyEmail = a.EmergencyEmail,
                                        EmergencyPhoneNumber = a.EmergencyPhoneNumber,
                                        EmergencyAddress = a.EmergencyAddress,
                                        SourceOfIncome = a.SourceOfIncome,
                                        SourceAmount = a.SourceAmount,
                                        Assets = a.Assets,
                                        AssetAmount = a.AssetAmount,
                                        PropertyId = a.PropertyId,
                                        UnitIds = a.UnitIds,
                                        IsSmoker = a.IsSmoker,
                                        IsBankruptcy = a.IsBankruptcy,
                                        IsEvicted = a.IsEvicted,
                                        HasPayRentIssue = a.HasPayRentIssue,
                                        IsCriminal = a.IsCriminal,
                                        StubPicture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Applications_StubPicture_" + a.ApplicationId + Path.GetExtension(a.StubPicture)}",
                                        StubPictureName = a.StubPicture,
                                        LicensePicture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Applications_LicensePicture_" + a.ApplicationId + Path.GetExtension(a.LicensePicture)}",
                                        LicensePictureName = a.LicensePicture,
                                        IsAgree = a.IsAgree,
                                        Pets = _db.ApplicationPets
                                           .Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true)
                                           .Select(x => new ApplicationPetsDto
                                           {
                                               // Map properties from ApplicationPets to ApplicationPetsDto
                                               PetId = x.PetId,
                                               ApplicationId = x.ApplicationId,
                                               Type = x.Type,
                                               Name = x.Name,
                                               Breed = x.Breed,
                                               Quantity = x.Quantity,
                                               Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"ApplicationPets_Picture_" + x.PetId + Path.GetExtension(x.Picture)}",
                                               PictureName = x.Picture,
                                           }).ToList(),
                                        Vehicles = _db.ApplicationVehicles.Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                        Dependent = _db.ApplicationDependent.Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                        AddedBy = p.AddedBy,
                                        AddedDate = a.AddedDate
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Applications: {ex.Message}");
                throw;
            }
        }

        public async Task<ApplicationsDto> GetApplicationByIdAsync(int id)
        {
            try
            {
                var result = await (from a in _db.Applications
                                    where a.ApplicationId == id && a.IsDeleted != true
                                    select new ApplicationsDto
                                    {
                                        ApplicationId = a.ApplicationId,
                                        FirstName = a.FirstName,
                                        MiddleName = a.MiddleName,
                                        LastName = a.LastName,
                                        SSN = a.SSN,
                                        ITIN = a.ITIN,
                                        DOB = a.DOB,
                                        Email = a.Email,
                                        PhoneNumber = a.PhoneNumber,
                                        Gender = a.Gender,
                                        MaritalStatus = a.MaritalStatus,
                                        DriverLicenseState = a.DriverLicenseState,
                                        DriverLicenseNumber = a.DriverLicenseNumber,
                                        Note = a.Note,
                                        Address = a.Address,
                                        LandlordName = a.LandlordName,
                                        ContactEmail = a.ContactEmail,
                                        ContactPhoneNumber = a.ContactPhoneNumber,
                                        MoveInDate = a.MoveInDate,
                                        MonthlyPayment = a.MonthlyPayment,
                                        JobType = a.JobType,
                                        JobTitle = a.JobTitle,
                                        AnnualIncome = a.AnnualIncome,
                                        CompanyName = a.CompanyName,
                                        WorkStatus = a.WorkStatus,
                                        StartDate = a.StartDate,
                                        EndDate = a.EndDate,
                                        SupervisorName = a.SupervisorName,
                                        SupervisorEmail = a.SupervisorEmail,
                                        SupervisorPhoneNumber = a.SupervisorPhoneNumber,
                                        EmergencyFirstName = a.EmergencyFirstName,
                                        EmergencyLastName = a.EmergencyLastName,
                                        EmergencyEmail = a.EmergencyEmail,
                                        EmergencyPhoneNumber = a.EmergencyPhoneNumber,
                                        EmergencyAddress = a.EmergencyAddress,
                                        SourceOfIncome = a.SourceOfIncome,
                                        SourceAmount = a.SourceAmount,
                                        Assets = a.Assets,
                                        AssetAmount = a.AssetAmount,
                                        PropertyId = a.PropertyId,
                                        UnitIds = a.UnitIds,
                                        IsSmoker = a.IsSmoker,
                                        IsBankruptcy = a.IsBankruptcy,
                                        IsEvicted = a.IsEvicted,
                                        HasPayRentIssue = a.HasPayRentIssue,
                                        IsCriminal = a.IsCriminal,
                                        StubPicture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Applications_StubPicture_" + a.ApplicationId + Path.GetExtension(a.StubPicture)}",
                                        StubPictureName = a.StubPicture,
                                        LicensePicture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Applications_LicensePicture_" + a.ApplicationId + Path.GetExtension(a.LicensePicture)}",
                                        LicensePictureName = a.LicensePicture,
                                        IsAgree = a.IsAgree,
                                        Pets = _db.ApplicationPets
                                           .Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true)
                                           .Select(x => new ApplicationPetsDto
                                           {
                                               // Map properties from ApplicationPets to ApplicationPetsDto
                                               PetId = x.PetId,
                                               ApplicationId = x.ApplicationId,
                                               Type = x.Type,
                                               Name = x.Name,
                                               Breed = x.Breed,
                                               Quantity = x.Quantity,
                                               Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"ApplicationPets_Picture_" + x.PetId + Path.GetExtension(x.Picture)}",
                                               PictureName = x.Picture,
                                           }).ToList(),
                                        Vehicles = _db.ApplicationVehicles.Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                        Dependent = _db.ApplicationDependent.Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                        AddedBy = a.AddedBy,
                                        AddedDate = a.AddedDate
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Application : {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveApplicationAsync(ApplicationsDto model)
        {
            var application = _db.Applications.FirstOrDefault(x => x.ApplicationId == model.ApplicationId && x.IsDeleted != true);

            if (application == null)
                application = new Applications();

            application.ApplicationId = model.ApplicationId;
            application.FirstName = model.FirstName;
            application.MiddleName = model.MiddleName;
            application.LastName = model.LastName;
            application.SSN = model.SSN;
            application.ITIN = model.ITIN;
            application.DOB = model.DOB;
            application.Email = model.Email;
            application.PhoneNumber = model.PhoneNumber;
            application.Gender = model.Gender;
            application.MaritalStatus = model.MaritalStatus;
            application.DriverLicenseState = model.DriverLicenseState;
            application.DriverLicenseNumber = model.DriverLicenseNumber;
            application.Note = model.Note;
            application.Address = model.Address;
            application.LandlordName = model.LandlordName;
            application.ContactEmail = model.ContactEmail;
            application.ContactPhoneNumber = model.ContactPhoneNumber;
            application.MoveInDate = model.MoveInDate;
            application.MonthlyPayment = model.MonthlyPayment;
            application.JobType = model.JobType;
            application.JobTitle = model.JobTitle;
            application.AnnualIncome = model.AnnualIncome;
            application.CompanyName = model.CompanyName;
            application.WorkStatus = model.WorkStatus;
            application.StartDate = model.StartDate;
            application.EndDate = model.EndDate;
            application.SupervisorName = model.SupervisorName;
            application.SupervisorEmail = model.SupervisorEmail;
            application.SupervisorPhoneNumber = model.SupervisorPhoneNumber;
            application.EmergencyFirstName = model.EmergencyFirstName;
            application.EmergencyLastName = model.EmergencyLastName;
            application.EmergencyEmail = model.EmergencyEmail;
            application.EmergencyPhoneNumber = model.EmergencyPhoneNumber;
            application.EmergencyAddress = model.EmergencyAddress;
            application.SourceOfIncome = model.SourceOfIncome;
            application.SourceAmount = model.SourceAmount;
            application.Assets = model.Assets;
            application.AssetAmount = model.AssetAmount;
            application.PropertyId = model.PropertyId;
            application.UnitIds = model.UnitIds;
            application.IsSmoker = model.IsSmoker;
            application.IsBankruptcy = model.IsBankruptcy;
            application.IsEvicted = model.IsEvicted;
            application.HasPayRentIssue = model.HasPayRentIssue;
            application.IsCriminal = model.IsCriminal;
            if (model.LicensePictureName != null)
            {
                application.LicensePicture = model.LicensePictureName;
            }
            if (model.StubPictureName != null)
            {
                application.StubPicture = model.StubPictureName;
            }
            application.IsAgree = model.IsAgree;


            if (application.ApplicationId > 0)
            {
                application.ModifiedBy = model.AddedBy;
                application.ModifiedDate = DateTime.Now;
                _db.Applications.Update(application);
            }
            else
            {
                application.AddedBy = model.AddedBy;
                application.AddedDate = DateTime.Now;
                _db.Applications.Add(application);
            }

            var result = await _db.SaveChangesAsync();

            int[] petsIds = model.Pets.Select(x => x.PetId).ToArray();
            var petsToBeDeleted = _db.ApplicationPets
                .Where(item => item.ApplicationId == model.ApplicationId && !petsIds.Contains(item.PetId) && item.IsDeleted != true)
                .ToList();

            foreach (var petToBeDeleted in petsToBeDeleted)
            {
                petToBeDeleted.IsDeleted = true;
                petToBeDeleted.ModifiedBy = model.AddedBy;
                petToBeDeleted.ModifiedDate = DateTime.Now;
                _db.ApplicationPets.Update(petToBeDeleted);
            }
            if (model.Pets != null && model.Pets.Any())
            {
                var existingPets = _db.ApplicationPets.Where(item => item.ApplicationId == model.ApplicationId && item.IsDeleted != true).ToList();
                foreach (var petDto in model.Pets)
                {
                    var existingPet = existingPets.FirstOrDefault(item => item.PetId == petDto.PetId && item.IsDeleted != true);

                    if (existingPet != null)
                    {
                        existingPet.ApplicationId = application.ApplicationId;
                        existingPet.Name = petDto.Name;
                        existingPet.Breed = petDto.Breed;
                        existingPet.Type = petDto.Type;
                        existingPet.Quantity = petDto.Quantity;
                        existingPet.Picture = petDto.Picture != null ? petDto.Picture : existingPet.Picture;
                        if (petDto.PictureName != null)
                        {
                            existingPet.Picture = petDto.PictureName;
                        }
                        existingPet.ModifiedBy = model.AddedBy;
                        existingPet.ModifiedDate = DateTime.Now;
                        _db.ApplicationPets.Update(existingPet);
                        if (petDto.Picture != null)
                        {
                            var ext = Path.GetExtension(petDto.PictureName);
                            await _googleCloudStorageService.UploadImagebyBase64Async(petDto.Picture, "ApplicationPets_Picture_" + existingPet.PetId + ext);
                        }
                    }
                    else
                    {
                        var newPet = new ApplicationPets
                        {
                            ApplicationId = application.ApplicationId,
                            Name = petDto.Name,
                            Breed = petDto.Breed,
                            Type = petDto.Type,
                            Quantity = petDto.Quantity,
                            Picture = petDto.Picture != null ? petDto.Picture : "",
                            AddedBy = model.AddedBy,
                            AddedDate = DateTime.Now,
                        };
                        if (petDto.PictureName != null)
                        {
                            newPet.Picture = petDto.PictureName;
                        }

                        _db.ApplicationPets.Add(newPet);
                        await _db.SaveChangesAsync();

                        if (petDto.Picture != null)
                        {
                            var ext = Path.GetExtension(petDto.PictureName);
                            await _googleCloudStorageService.UploadImagebyBase64Async(petDto.Picture, "ApplicationPets_Picture_" + newPet.PetId + ext);
                        }
                    }
                }
            }

            int[] vehiclesIds = model.Vehicles.Select(x => x.VehicleId).ToArray();
            var vehiclesToBeDeleted = _db.ApplicationVehicles
                .Where(item => item.ApplicationId == model.ApplicationId && !vehiclesIds.Contains(item.VehicleId) && item.IsDeleted != true)
                .ToList();

            foreach (var vehicleToBeDeleted in vehiclesToBeDeleted)
            {
                vehicleToBeDeleted.IsDeleted = true;
                vehicleToBeDeleted.ModifiedBy = model.AddedBy;
                vehicleToBeDeleted.ModifiedDate = DateTime.Now;
                _db.ApplicationVehicles.Update(vehicleToBeDeleted);
            }
            if (model.Vehicles != null && model.Vehicles.Any())
            {
                var existingVehicles = _db.ApplicationVehicles.Where(item => item.ApplicationId == model.ApplicationId && item.IsDeleted != true).ToList();
                foreach (var vehicleDto in model.Vehicles)
                {
                    var existingVehicle = existingVehicles.FirstOrDefault(item => item.VehicleId == vehicleDto.VehicleId && item.IsDeleted != true);

                    if (existingVehicle != null)
                    {
                        existingVehicle.ApplicationId = application.ApplicationId;
                        existingVehicle.Manufacturer = vehicleDto.Manufacturer;
                        existingVehicle.ModelName = vehicleDto.ModelName;
                        existingVehicle.Color = vehicleDto.Color;
                        existingVehicle.LicensePlate = vehicleDto.LicensePlate;
                        existingVehicle.Year = vehicleDto.Year;
                        existingVehicle.ModifiedBy = model.AddedBy;
                        existingVehicle.ModifiedDate = DateTime.Now;
                        _db.ApplicationVehicles.Update(existingVehicle);
                    }
                    else
                    {
                        var newVehicle = new ApplicationVehicles
                        {
                            ApplicationId = application.ApplicationId,
                            Manufacturer = vehicleDto.Manufacturer,
                            ModelName = vehicleDto.ModelName,
                            Color = vehicleDto.Color,
                            LicensePlate = vehicleDto.LicensePlate,
                            Year = vehicleDto.Year,
                            AddedBy = model.AddedBy,
                            AddedDate = DateTime.Now,
                        };
                        _db.ApplicationVehicles.Add(newVehicle);
                    }
                }
            }

            int[] dependentsIds = model.Dependent.Select(x => x.DependentId).ToArray();
            var dependentsToBeDeleted = _db.ApplicationDependent
                .Where(item => item.ApplicationId == model.ApplicationId && !dependentsIds.Contains(item.DependentId) && item.IsDeleted != true)
                .ToList();

            foreach (var dependentToBeDeleted in dependentsToBeDeleted)
            {
                dependentToBeDeleted.IsDeleted = true;
                dependentToBeDeleted.ModifiedBy = model.AddedBy;
                dependentToBeDeleted.ModifiedDate = DateTime.Now;
                _db.ApplicationDependent.Update(dependentToBeDeleted);
            }
            if (model.Dependent != null && model.Dependent.Any())
            {
                var existingDependents = _db.ApplicationDependent.Where(item => item.ApplicationId == model.ApplicationId && item.IsDeleted != true).ToList();
                foreach (var dependentDto in model.Dependent)
                {
                    var existingDependent = existingDependents.FirstOrDefault(item => item.DependentId == dependentDto.DependentId && item.IsDeleted != true);

                    if (existingDependent != null)
                    {
                        existingDependent.ApplicationId = application.ApplicationId;
                        existingDependent.FirstName = dependentDto.FirstName;
                        existingDependent.LastName = dependentDto.LastName;
                        existingDependent.Email = dependentDto.Email;
                        existingDependent.PhoneNumber = dependentDto.PhoneNumber;
                        existingDependent.DOB = dependentDto.DOB;
                        existingDependent.Relation = dependentDto.Relation;
                        existingDependent.ModifiedBy = model.AddedBy;
                        existingDependent.ModifiedDate = DateTime.Now;
                        _db.ApplicationDependent.Update(existingDependent);
                    }
                    else
                    {
                        var newPet = new ApplicationDependent
                        {
                            ApplicationId = application.ApplicationId,
                            FirstName = dependentDto.FirstName,
                            LastName = dependentDto.LastName,
                            Email = dependentDto.Email,
                            PhoneNumber = dependentDto.PhoneNumber,
                            DOB = dependentDto.DOB,
                            Relation = dependentDto.Relation,
                            AddedBy = model.AddedBy,
                            AddedDate = DateTime.Now,
                        };
                        _db.ApplicationDependent.Add(newPet);
                    }
                }
            }
            var result1 = await _db.SaveChangesAsync();

            if (model.LicensePicture != null)
            {

                var ext = Path.GetExtension(model.LicensePictureName);
                await _googleCloudStorageService.UploadImagebyBase64Async(model.LicensePicture, "Applications_LicensePicture_" + application.ApplicationId + ext);

            }
            if (model.StubPicture != null)
            {

                var ext = Path.GetExtension(model.StubPictureName);
                await _googleCloudStorageService.UploadImagebyBase64Async(model.StubPicture, "Applications_StubPicture_" + application.ApplicationId + ext);

            }
            return result > 0;

        }

        public async Task<bool> DeleteApplicationAsync(int id)
        {
            var application = await _db.Applications.FindAsync(id);
            if (application == null) return false;

            application.IsDeleted = true;
            _db.Applications.Update(application);
            var saveResult = await _db.SaveChangesAsync();

            if (saveResult <= 0) return false;

            var pets = await _db.ApplicationPets.Where(x => x.ApplicationId == id && x.IsDeleted != true).ToListAsync();
            pets.ForEach(x => x.IsDeleted = true);
            _db.ApplicationPets.UpdateRange(pets);

            var vehicles = await _db.ApplicationVehicles.Where(x => x.ApplicationId == id && x.IsDeleted != true).ToListAsync();
            vehicles.ForEach(x => x.IsDeleted = true);
            _db.ApplicationVehicles.UpdateRange(vehicles);

            var dependents = await _db.ApplicationDependent.Where(x => x.ApplicationId == id && x.IsDeleted != true).ToListAsync();
            dependents.ForEach(x => x.IsDeleted = true);
            _db.ApplicationDependent.UpdateRange(dependents);

            saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
        }

        public async Task<string> GetTermsbyId(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            var TermsAndCondtion = user.TermsAndConditons ?? "";
            return TermsAndCondtion;
        }

        #endregion

        #region AccountType

        public async Task<List<AccountType>> GetAccountTypesAsync()
        {
            try
            {
                var result = await (from at in _db.AccountType
                                    where at.IsDeleted != true
                                    select new AccountType
                                    {
                                        AccountTypeId = at.AccountTypeId,
                                        Type = at.Type,
                                        AddedBy = at.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Account Types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AccountType>> GetAccountTypesDllAsync(Filter filter)
        {
            try
            {
                var result = await (from at in _db.AccountType
                                    where at.IsDeleted != true && at.AddedBy == filter.AddedBy
                                    select new AccountType
                                    {
                                        AccountTypeId = at.AccountTypeId,
                                        Type = at.Type,
                                        AddedBy = at.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Account Types: {ex.Message}");
                throw;
            }
        }
        public async Task<AccountType> GetAccountTypeByIdAsync(int id)
        {
            try
            {
                var result = await (from at in _db.AccountType
                                    where at.AccountTypeId == id && at.IsDeleted != true
                                    select new AccountType
                                    {
                                        AccountTypeId = at.AccountTypeId,
                                        Type = at.Type,
                                        AddedBy = at.AddedBy,

                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Account Type: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveAccountTypeAsync(AccountType model)
        {
            var accountType = _db.AccountType.FirstOrDefault(x => x.AccountTypeId == model.AccountTypeId && x.IsDeleted != true);

            if (accountType == null)
                accountType = new AccountType();

            accountType.AccountTypeId = model.AccountTypeId;
            accountType.Type = model.Type;

            if (accountType.AccountTypeId > 0)
            {
                accountType.ModifiedBy = model.AddedBy;
                accountType.ModifiedDate = DateTime.Now;
                _db.AccountType.Update(accountType);
            }
            else
            {
                accountType.AddedBy = model.AddedBy;
                accountType.AddedDate = DateTime.Now;
                _db.AccountType.Add(accountType);
            }

            var result = await _db.SaveChangesAsync();


            return result > 0;

        }

        public async Task<APIResponse> DeleteAccountTypeAsync(int id)
        {
            var response = new APIResponse();

            try
            {
                var isAccountSubTypeRefrence = await _db.AccountSubType.AnyAsync(x => x.AccountTypeId == id && x.IsDeleted != true);
                if (isAccountSubTypeRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Account Type has refrence in Account Sub-Type.");
                    return response;
                }

                var isChartAccountRefrence = await _db.ChartAccount.AnyAsync(x => x.AccountTypeId == id && x.IsDeleted != true);
                if (isChartAccountRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Account Type has refrence in Chart Account.");
                    return response;
                }

                var accountType = await _db.AccountType.FindAsync(id);
                if (accountType == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Account type not found.");
                    return response;
                }

                accountType.IsDeleted = true;
                _db.AccountType.Update(accountType);
                await _db.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Result = "Account type deleted successfully.";
                
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
                throw new Exception(string.Join(", ", response.ErrorMessages));
            }

            return response;
        }


        #endregion

        #region AccountSubType

        public async Task<List<AccountSubTypeDto>> GetAccountSubTypesAsync()
        {
            try
            {
                var result = await (from ast in _db.AccountSubType
                                    from at in _db.AccountType.Where(x => x.AccountTypeId == ast.AccountTypeId && x.IsDeleted != true).DefaultIfEmpty()
                                    where ast.IsDeleted != true
                                    select new AccountSubTypeDto
                                    {
                                        AccountSubTypeId = ast.AccountSubTypeId,
                                        AccountTypeId = ast.AccountTypeId,
                                        AccountType = at.Type,
                                        Type = ast.Type,
                                        Description = ast.Description,
                                        AddedBy = ast.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Account Sub Types: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AccountSubTypeDto>> GetAccountSubTypesDllAsync(Filter filter)
        {
            try
            {
                var subAccounts = await (from ast in _db.AccountSubType
                                         from at in _db.AccountType.Where(x => x.AccountTypeId == ast.AccountTypeId && x.IsDeleted != true).DefaultIfEmpty()
                                         where ast.IsDeleted != true
                                         select new AccountSubTypeDto
                                         {
                                             AccountSubTypeId = ast.AccountSubTypeId,
                                             AccountTypeId = ast.AccountTypeId,
                                             AccountType = at.Type,
                                             Type = ast.Type,
                                             Description = ast.Description,
                                             AddedBy = ast.AddedBy,
                                         })
                     .AsNoTracking()
                     .ToListAsync();

                if (filter.AccountTypeIds.Count() > 0)
                {
                    subAccounts = subAccounts.Where(x => filter.AccountTypeIds.Contains(x.AccountTypeId)).ToList();
                }


                return subAccounts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Account Sub Types: {ex.Message}");
                throw;
            }
        }

        public async Task<AccountSubType> GetAccountSubTypeByIdAsync(int id)
        {
            try
            {
                var result = await (from ast in _db.AccountSubType
                                    where ast.AccountSubTypeId == id && ast.IsDeleted != true
                                    select new AccountSubType
                                    {
                                        AccountSubTypeId = ast.AccountSubTypeId,
                                        AccountTypeId = ast.AccountTypeId,
                                        Type = ast.Type,
                                        Description = ast.Description,
                                        AddedBy = ast.AddedBy,

                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Account Sub Type: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveAccountSubTypeAsync(AccountSubType model)
        {
            var accountSubType = _db.AccountSubType.FirstOrDefault(x => x.AccountSubTypeId == model.AccountSubTypeId && x.IsDeleted != true);

            if (accountSubType == null)
                accountSubType = new AccountSubType();

            accountSubType.AccountSubTypeId = model.AccountSubTypeId;
            accountSubType.AccountTypeId = model.AccountTypeId;
            accountSubType.Type = model.Type;
            accountSubType.Description = model.Description;

            if (accountSubType.AccountSubTypeId > 0)
            {
                accountSubType.ModifiedBy = model.AddedBy;
                accountSubType.ModifiedDate = DateTime.Now;
                _db.AccountSubType.Update(accountSubType);
            }
            else
            {
                accountSubType.AddedBy = model.AddedBy;
                accountSubType.AddedDate = DateTime.Now;
                _db.AccountSubType.Add(accountSubType);
            }

            var result = await _db.SaveChangesAsync();


            return result > 0;

        }

        public async Task<APIResponse> DeleteAccountSubTypeAsync(int id)
        {
            var response = new APIResponse();

            try
            {
                var isChartAccountRefrence = await _db.ChartAccount.AnyAsync(x => x.AccountSubTypeId == id && x.IsDeleted != true);
                if (isChartAccountRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This Account sub-type has refrence in Chart Account.");
                    return response;
                }

                var accountSubType = await _db.AccountSubType.FindAsync(id);
                if (accountSubType == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Account sub-type not found.");
                    return response;
                }

                accountSubType.IsDeleted = true;
                _db.AccountSubType.Update(accountSubType);
                await _db.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Result = "Account sub-type deleted successfully.";
                
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
                throw new Exception(string.Join(", ", response.ErrorMessages));
            }

            return response;
        }


        #endregion


        #region ChartAccount

        public async Task<List<ChartAccountDto>> GetChartAccountsAsync()
        {
            try
            {
                var result = await (from ca in _db.ChartAccount
                                    from ast in _db.AccountSubType.Where(x => x.AccountSubTypeId == ca.AccountSubTypeId && x.IsDeleted != true).DefaultIfEmpty()
                                    from at in _db.AccountType.Where(x => x.AccountTypeId == ca.AccountTypeId && x.IsDeleted != true).DefaultIfEmpty()
                                    where ca.IsDeleted != true
                                    select new ChartAccountDto
                                    {
                                        ChartAccountId = ca.ChartAccountId,
                                        AccountSubTypeId = ca.AccountSubTypeId,
                                        AccountSubType = ast.Type,
                                        AccountTypeId = ca.AccountTypeId,
                                        AccountType = at.Type,
                                        Name = ca.Name,
                                        Description = ca.Description,
                                        ParentAccountId = ca.ParentAccountId,
                                        IsActive = ca.IsActive,
                                        IsSubAccount = ca.IsSubAccount,
                                        ParentAccount = ca.ParentAccountId != null ? _db.ChartAccount.FirstOrDefault(x => x.ChartAccountId == ca.ParentAccountId && x.IsDeleted != true).Name : "",
                                        AddedBy = ca.AddedBy,
                                        ChildAccountsDto = (from cca in _db.ChartAccount
                                                            where cca.ParentAccountId == ca.ChartAccountId && cca.IsDeleted != true
                                                            select new ChildAccountDto
                                                            {
                                                                ChartAccountId = cca.ChartAccountId,
                                                                AccountTypeId = cca.AccountTypeId,
                                                                Name = cca.Name,
                                                                ParentAccountId = cca.ParentAccountId,
                                                            }).ToList()
                                    })
                    .AsNoTracking()
                    .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Chart Account: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ChartAccountDto>> GetChartAccountsDllAsync(Filter filter)
        {
            try
            {
                var chartAccount = _db.ChartAccount.Where(x => x.IsDeleted != true)
                 .Select(ca => new ChartAccountDto
                 {
                     ChartAccountId = ca.ChartAccountId,
                     Name = ca.Name,
                     AddedBy = ca.AddedBy,
                 });

                if (!string.IsNullOrEmpty(filter.AddedBy))
                {
                    chartAccount = chartAccount.Where(x => x.AddedBy == filter.AddedBy);
                }

                var chartAccounts = await chartAccount.ToListAsync();

                return chartAccounts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Chart Account: {ex.Message}");
                throw;
            }
        }

        public async Task<ChartAccount> GetChartAccountByIdAsync(int id)
        {
            try
            {
                var result = await (from ca in _db.ChartAccount
                                    from ast in _db.AccountSubType.Where(x => x.AccountSubTypeId == ca.AccountSubTypeId && x.IsDeleted != true).DefaultIfEmpty()
                                    from at in _db.AccountType.Where(x => x.AccountTypeId == ca.AccountTypeId && x.IsDeleted != true).DefaultIfEmpty()
                                    where ca.ChartAccountId == id && ca.IsDeleted != true
                                    select new ChartAccount
                                    {
                                        ChartAccountId = ca.ChartAccountId,
                                        AccountSubTypeId = ca.AccountSubTypeId,
                                        AccountTypeId = ca.AccountTypeId,
                                        Name = ca.Name,
                                        Description = ca.Description,
                                        ParentAccountId = ca.ParentAccountId,
                                        IsActive = ca.IsActive,
                                        IsSubAccount = ca.IsSubAccount,
                                        AddedBy = ca.AddedBy
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Chart Account: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveChartAccountAsync(ChartAccount model)
        {
            var chartAccount = _db.ChartAccount.FirstOrDefault(x => x.ChartAccountId == model.ChartAccountId && x.IsDeleted != true);

            if (chartAccount == null)
                chartAccount = new ChartAccount();

            chartAccount.AccountSubTypeId = model.AccountSubTypeId;
            chartAccount.AccountTypeId = model.AccountTypeId;
            chartAccount.Name = model.Name;
            chartAccount.Description = model.Description;
            chartAccount.IsActive = model.IsActive;
            chartAccount.IsSubAccount = model.IsSubAccount;
            chartAccount.ParentAccountId = model.ParentAccountId;

            if (chartAccount.ChartAccountId > 0)
            {
                chartAccount.ModifiedBy = model.AddedBy;
                chartAccount.ModifiedDate = DateTime.Now;
                _db.ChartAccount.Update(chartAccount);
            }
            else
            {
                chartAccount.AddedBy = model.AddedBy;
                chartAccount.AddedDate = DateTime.Now;
                _db.ChartAccount.Add(chartAccount);
            }

            var result = await _db.SaveChangesAsync();


            return result > 0;

        }

        public async Task<APIResponse> DeleteChartAccountAsync(int id)
        {
            var response = new APIResponse();

            try
            {
                var isLineItemRefrence = await _db.LineItem.AnyAsync(x => x.ChartAccountId == id && x.IsDeleted != true);
                if (isLineItemRefrence)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("This ChartAccount has refrence in TaskRequest-LineItem.");
                    return response;
                }

                var chartAccount = await _db.ChartAccount.FindAsync(id);
                if (chartAccount == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Chart account not found.");
                    return response;
                }

                chartAccount.IsDeleted = true;
                _db.ChartAccount.Update(chartAccount);
                await _db.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Result = "Chart account deleted successfully.";
                
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);
                throw new Exception(string.Join(", ", response.ErrorMessages));
            }

            return response;
        }


        #endregion

        #region Budget

        public async Task<List<BudgetDto>> GetBudgetsAsync()
        {
            try
            {
                var result = await (from b in _db.Budgets
                                    where b.IsDeleted != true
                                    select new BudgetDto
                                    {
                                        BudgetId = b.BudgetId,
                                        BudgetType = b.BudgetType,
                                        BudgetBy = b.BudgetBy,
                                        PropertyId = b.PropertyId,
                                        StartingMonth = b.StartingMonth,
                                        FiscalYear = b.FiscalYear,
                                        BudgetName = b.BudgetName,
                                        Period = b.Period,
                                        ReferenceData = b.ReferenceData,
                                        AccountingMethod = b.AccountingMethod,
                                        ShowReferenceData = b.ShowReferenceData,
                                        AddedBy = b.AddedBy
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Budget: {ex.Message}");
                throw;
            }
        }

        public async Task<Budget> GetBudgetByIdAsync(int id)
        {
            try
            {
                var result = await _db.Budgets.Include(x => x.Items).Where(x => x.BudgetId == id && x.IsDeleted != true).FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Budget: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveBudgetAsync(BudgetDto model)
        {
            var budget = new Budget();

            List<BudgetItem> li = new List<BudgetItem>();
            var budgetItems = JsonConvert.DeserializeObject<List<BudgetItemDto>>(model.ItemsJson);
            foreach (var x in budgetItems)
            {
                if (x.Total != "Total" && x.AccountName != "AccountName")
                {
                    BudgetItem item = new BudgetItem()
                    {
                        AccountName = x.AccountName,
                        Total = decimal.Parse(x.Total.Replace(".", ",")),
                        Period = x.Period,
                        BudgetItemMonth = new BudgetItemMonth()
                        {
                            Jan = x.BudgetItemMonth.Jan != null ? decimal.Parse(x.BudgetItemMonth.Jan.Replace(".", ",")) : 0,
                            Feb = x.BudgetItemMonth.Feb != null ? decimal.Parse(x.BudgetItemMonth.Feb.Replace(".", ",")) : 0,
                            March = x.BudgetItemMonth.March != null ? decimal.Parse(x.BudgetItemMonth.March.Replace(".", ",")) : 0,
                            April = x.BudgetItemMonth.April != null ? decimal.Parse(x.BudgetItemMonth.April.Replace(".", ",")) : 0,
                            May = x.BudgetItemMonth.May != null ? decimal.Parse(x.BudgetItemMonth.May.Replace(".", ",")) : 0,
                            June = x.BudgetItemMonth.June != null ? decimal.Parse(x.BudgetItemMonth.June.Replace(".", ",")) : 0,
                            July = x.BudgetItemMonth.July != null ? decimal.Parse(x.BudgetItemMonth.July.Replace(".", ",")) : 0,
                            Aug = x.BudgetItemMonth.Aug != null ? decimal.Parse(x.BudgetItemMonth.Aug.Replace(".", ",")) : 0,
                            Sep = x.BudgetItemMonth.Sep != null ? decimal.Parse(x.BudgetItemMonth.Sep.Replace(".", ",")) : 0,
                            Oct = x.BudgetItemMonth.Oct != null ? decimal.Parse(x.BudgetItemMonth.Oct.Replace(".", ",")) : 0,
                            Nov = x.BudgetItemMonth.Nov != null ? decimal.Parse(x.BudgetItemMonth.Nov.Replace(".", ",")) : 0,
                            Dec = x.BudgetItemMonth.Dec != null ? decimal.Parse(x.BudgetItemMonth.Dec.Replace(".", ",")) : 0,
                            quat1 = x.BudgetItemMonth.quat1 != null ? decimal.Parse(x.BudgetItemMonth.quat1.Replace(".", ",")) : 0,
                            quat2 = x.BudgetItemMonth.quat2 != null ? decimal.Parse(x.BudgetItemMonth.quat2.Replace(".", ",")) : 0,
                            quat4 = x.BudgetItemMonth.quat4 != null ? decimal.Parse(x.BudgetItemMonth.quat4.Replace(".", ",")) : 0,
                            quat5 = x.BudgetItemMonth.quat5 != null ? decimal.Parse(x.BudgetItemMonth.quat5.Replace(".", ",")) : 0,
                            YearStart = x.BudgetItemMonth.YearStart != null ? decimal.Parse(x.BudgetItemMonth.YearStart.Replace(".", ",")) : 0,
                            YearEnd = x.BudgetItemMonth.YearEnd != null ? decimal.Parse(x.BudgetItemMonth.YearEnd.Replace(".", ",")) : 0,
                            AddedBy = model.AddedBy,
                            AddedDate = DateTime.Now


                        }

                    };
                    li.Add(item);
                }
            }

            budget.Items = li;
            budget.AddedDate = DateTime.Now;
            budget.AddedBy = model.AddedBy;
            budget.BudgetType = model.BudgetType;
            budget.BudgetBy = model.BudgetBy;
            budget.PropertyId = model.PropertyId;
            budget.StartingMonth = model.StartingMonth;
            budget.FiscalYear = model.FiscalYear;
            budget.BudgetName = model.BudgetName;
            budget.Period = model.Period;
            budget.ReferenceData = model.ReferenceData;
            budget.AccountingMethod = model.AccountingMethod;
            budget.ShowReferenceData = model.ShowReferenceData;
            await _db.Budgets.AddAsync(budget);
            var result = await _db.SaveChangesAsync();

            return result > 0;

        }

        public async Task<bool> SaveDuplicateBudgetAsync(BudgetDto model)
        {
            var budget = new Budget();

            List<BudgetItem> li = new List<BudgetItem>();
            try
            {
                var budgetItems = JsonConvert.DeserializeObject<List<BudgetItemDto>>(model.ItemsJson);
                foreach (var x in budgetItems)
                {
                    if (x.Total != "Total" && x.AccountName != "AccountName")
                    {
                        BudgetItem item = new BudgetItem()
                        {
                            AccountName = x.AccountName,
                            Total = decimal.Parse(x.Total.Replace(".", ",")),
                            Period = x.Period,
                            BudgetItemMonth = new BudgetItemMonth()
                            {
                                Jan = x.BudgetItemMonth.Jan != null ? decimal.Parse(x.BudgetItemMonth.Jan.Replace(".", ",")) : 0,
                                Feb = x.BudgetItemMonth.Feb != null ? decimal.Parse(x.BudgetItemMonth.Feb.Replace(".", ",")) : 0,
                                March = x.BudgetItemMonth.March != null ? decimal.Parse(x.BudgetItemMonth.March.Replace(".", ",")) : 0,
                                April = x.BudgetItemMonth.April != null ? decimal.Parse(x.BudgetItemMonth.April.Replace(".", ",")) : 0,
                                May = x.BudgetItemMonth.May != null ? decimal.Parse(x.BudgetItemMonth.May.Replace(".", ",")) : 0,
                                June = x.BudgetItemMonth.June != null ? decimal.Parse(x.BudgetItemMonth.June.Replace(".", ",")) : 0,
                                July = x.BudgetItemMonth.July != null ? decimal.Parse(x.BudgetItemMonth.July.Replace(".", ",")) : 0,
                                Aug = x.BudgetItemMonth.Aug != null ? decimal.Parse(x.BudgetItemMonth.Aug.Replace(".", ",")) : 0,
                                Sep = x.BudgetItemMonth.Sep != null ? decimal.Parse(x.BudgetItemMonth.Sep.Replace(".", ",")) : 0,
                                Oct = x.BudgetItemMonth.Oct != null ? decimal.Parse(x.BudgetItemMonth.Oct.Replace(".", ",")) : 0,
                                Nov = x.BudgetItemMonth.Nov != null ? decimal.Parse(x.BudgetItemMonth.Nov.Replace(".", ",")) : 0,
                                Dec = x.BudgetItemMonth.Dec != null ? decimal.Parse(x.BudgetItemMonth.Dec.Replace(".", ",")) : 0,
                                quat1 = x.BudgetItemMonth.quat1 != null ? decimal.Parse(x.BudgetItemMonth.quat1.Replace(".", ",")) : 0,
                                quat2 = x.BudgetItemMonth.quat2 != null ? decimal.Parse(x.BudgetItemMonth.quat2.Replace(".", ",")) : 0,
                                quat4 = x.BudgetItemMonth.quat4 != null ? decimal.Parse(x.BudgetItemMonth.quat4.Replace(".", ",")) : 0,
                                quat5 = x.BudgetItemMonth.quat5 != null ? decimal.Parse(x.BudgetItemMonth.quat5.Replace(".", ",")) : 0,
                                YearStart = x.BudgetItemMonth.YearStart != null ? decimal.Parse(x.BudgetItemMonth.YearStart.Replace(".", ",")) : 0,
                                YearEnd = x.BudgetItemMonth.YearEnd != null ? decimal.Parse(x.BudgetItemMonth.YearEnd.Replace(".", ",")) : 0,
                                AddedBy = model.AddedBy,
                                AddedDate = DateTime.Now
                            }

                        };
                        li.Add(item);
                    }
                }

                budget.Items = li;
                budget.AddedDate = DateTime.Now;
                budget.IsDeleted = false;
                budget.AddedBy = model.AddedBy;
                budget.BudgetType = model.BudgetType;
                budget.BudgetBy = model.BudgetBy;
                budget.PropertyId = model.PropertyId;
                budget.StartingMonth = model.StartingMonth;
                budget.FiscalYear = model.FiscalYear;
                budget.BudgetName = model.BudgetName;
                budget.Period = model.Period;
                budget.ReferenceData = model.ReferenceData;
                budget.AccountingMethod = model.AccountingMethod;
                budget.ShowReferenceData = model.ShowReferenceData;
                await _db.Budgets.AddAsync(budget);
                var result = await _db.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Budget: {ex.Message}");
                throw;
            }

        }

        public async Task<bool> DeleteBudgetAsync(int id)
        {
            var budget = await _db.Budgets.FindAsync(id);
            if (budget == null) return false;

            budget.IsDeleted = true;
            _db.Budgets.Update(budget);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
        }

        #endregion

        #region Reports

        public async Task<List<LeaseReportDto>> GetLeaseReportAsync(ReportFilter reportFilter)
        {
            try
            {
                var leasesWithProperties = await (from l in _db.Lease
                                                  join p in _db.Assets on l.AssetId equals p.AssetId into properties
                                                  from p in properties.DefaultIfEmpty()
                                                  where l.IsDeleted != true && (p == null || p.IsDeleted != true)
                                                  select new
                                                  {
                                                      l.LeaseId,
                                                      l.StartDate,
                                                      l.EndDate,
                                                      l.Status,
                                                      PropertyId = (int?)p.AssetId,
                                                      Property = p != null ? p.BuildingNo + " - " + p.BuildingName : null,
                                                      l.UnitId,
                                                      l.SelectedUnit
                                                  }).ToListAsync();


                var rentCharges = await _db.RentCharge
                          .GroupBy(rc => rc.LeaseId)
                          .Select(g => new
                          {
                              LeaseId = g.Key,
                              TotalRentCharges = g.Sum(x => x.Amount)
                          }).ToListAsync();

                var securityDeposits = await _db.SecurityDeposit
                         .GroupBy(rc => rc.LeaseId)
                         .Select(g => new
                         {
                             LeaseId = g.Key,
                             TotalSecurityDeposit = g.Sum(x => x.Amount)
                         }).ToListAsync();

                var leaseReportDtoList = (from l in leasesWithProperties
                                          join rc in rentCharges on l.LeaseId equals rc.LeaseId into leaseRentCharges
                                          from rc in leaseRentCharges.DefaultIfEmpty()
                                          join sc in securityDeposits on l.LeaseId equals sc.LeaseId into leaseSecurityDeposits
                                          from sc in leaseSecurityDeposits.DefaultIfEmpty()
                                          select new LeaseReportDto
                                          {
                                              Lease = l.LeaseId.ToString(),
                                              StartDate = l.StartDate,
                                              EndDate = l.EndDate,
                                              Status = l.Status,
                                              PropertyId = l.PropertyId,
                                              Property = l.Property,
                                              Unit = l.SelectedUnit,
                                              UnitId = l.UnitId,
                                              RentCharges = rc != null ? rc.TotalRentCharges : 0,
                                              SecurityDeposit = sc != null ? sc.TotalSecurityDeposit : 0
                                          }).ToList();

                if (reportFilter.PropertiesIds.Count() > 0 && reportFilter.PropertiesIds.Any())
                {
                    leaseReportDtoList = leaseReportDtoList.Where(x => reportFilter.PropertiesIds.Contains(x.PropertyId)).ToList();
                }
                if (reportFilter.UnitsIds.Count() > 0 && reportFilter.UnitsIds.Any())
                {
                    leaseReportDtoList = leaseReportDtoList.Where(x => reportFilter.UnitsIds.Contains(x.UnitId)).ToList();
                }


                if (reportFilter.LeaseStartDateFilter != null && reportFilter.LeaseEndDateFilter != null)
                {
                    leaseReportDtoList = leaseReportDtoList
                        .Where(x => x.StartDate >= reportFilter.LeaseStartDateFilter && x.EndDate <= reportFilter.LeaseEndDateFilter)
                        .ToList();
                }
                if (reportFilter.LeaseMinRentFilter != null)
                {
                    leaseReportDtoList = leaseReportDtoList
                        .Where(x => x.RentCharges >= reportFilter.LeaseMinRentFilter)
                        .ToList();
                }

                if (reportFilter.LeaseMaxRentFilter != null)
                {
                    leaseReportDtoList = leaseReportDtoList
                        .Where(x => x.RentCharges <= reportFilter.LeaseMaxRentFilter)
                        .ToList();
                }


                //if (reportFilter.LeaseMinRentFilter != null && reportFilter.LeaseMinRentFilter.Any() &&
                //    reportFilter.LeaseMaxRentFilter != null && reportFilter.LeaseMaxRentFilter.Any())
                //{
                //    leaseReportDtoList = leaseReportDtoList
                //        .Where(x => reportFilter.LeaseMinRentFilter.All(min => min == null || x.RentCharges >= min) &&
                //                    reportFilter.LeaseMaxRentFilter.All(max => max == null || x.RentCharges <= max))
                //        .ToList();
                //}


                return leaseReportDtoList;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Leases: {ex.Message}");
                throw;
            }
        }

        public async Task<List<InvoiceReportDto>> GetInvoiceReportAsync(ReportFilter reportFilter)
        {
            try
            {
                //var invoiceReportDtoList = await (from i in _db.Invoices
                //                                  from l in _db.Lease.Where(x => x.LeaseId == i.LeaseId && x.IsDeleted != true).DefaultIfEmpty()
                //                                  join p in _db.Assets on l.AssetId equals p.AssetId into properties
                //                                  from p in properties.DefaultIfEmpty()
                //                                  where i.IsDeleted != true && (p == null || p.IsDeleted != true)
                //                                  select new
                //                                  {
                //                                      i.InvoiceId,
                //                                      l.LeaseId,
                //                                      l.StartDate,
                //                                      l.EndDate,
                //                                      i.InvoiceDate,
                //                                      i.Status,
                //                                      PropertyId = (int?)p.AssetId,
                //                                      Property = p != null ? p.BuildingNo + " - " + p.BuildingName : null,
                //                                      l.SelectedUnit
                //                                  }).ToListAsync();

                //var invoiceRentCharges = await (from rc in _db.RentCharge
                //                                group rc by rc.LeaseId into g
                //                                select new
                //                                {
                //                                    LeaseId = g.Key,
                //                                    TotalRentCharges = g.Sum(x => x.Amount)
                //                                }).ToListAsync();

                //var invoiceReportDtoListWithCharges = (from i in invoiceReportDtoList
                //                                       from rc in invoiceRentCharges.Where(x => x.LeaseId == i.LeaseId).DefaultIfEmpty()
                //                                       select new InvoiceReportDto
                //                                       {
                //                                           Invoice = i.InvoiceId.ToString(),
                //                                           StartDate = i.StartDate,
                //                                           EndDate = i.EndDate,
                //                                           Status = i.Status,
                //                                           PropertyId = i.PropertyId,
                //                                           Property = i.Property,
                //                                           Unit = i.SelectedUnit,
                //                                           InvoiceDate = i.InvoiceDate,
                //                                           RentCharges = rc != null ? rc.TotalRentCharges : 0
                //                                       }).ToList();
                var invoiceReportDtoList = await (from i in _db.Invoices
                                                  join l in _db.Lease on i.LeaseId equals l.LeaseId into leaseGroup
                                                  from l in leaseGroup.Where(x => x.IsDeleted != true).DefaultIfEmpty()
                                                  join p in _db.Assets on l.AssetId equals p.AssetId into properties
                                                  from p in properties.Where(x => x.IsDeleted != true).DefaultIfEmpty()
                                                  join u in _db.AssetsUnits on l.UnitId equals u.UnitId into units
                                                  from u in units.Where(x => x.IsDeleted != true).DefaultIfEmpty()
                                                  join rc in _db.RentCharge on l.LeaseId equals rc.LeaseId into rentCharges
                                                  from rc in rentCharges.DefaultIfEmpty()
                                                  where i.IsDeleted != true
                                                  group new { i, l, p, rc } by new
                                                  {
                                                      i.InvoiceId,
                                                      l.LeaseId,
                                                      l.StartDate,
                                                      l.EndDate,
                                                      i.InvoiceDate,
                                                      i.Status,
                                                      p.AssetId,
                                                      p.BuildingNo,
                                                      p.BuildingName,
                                                      u.UnitId,
                                                      u.UnitName,
                                                  } into g
                                                  select new InvoiceReportDto
                                                  {
                                                      Invoice = g.Key.InvoiceId.ToString(),
                                                      StartDate = g.Key.StartDate,
                                                      EndDate = g.Key.EndDate,
                                                      Status = g.Key.Status,
                                                      PropertyId = (int?)g.Key.AssetId,
                                                      Property = g.Key.BuildingNo + " - " + g.Key.BuildingName,
                                                      UnitId = g.Key.UnitId,
                                                      Unit = g.Key.UnitName,
                                                      InvoiceDate = g.Key.InvoiceDate,
                                                      RentCharges = g.Sum(x => x.rc.Amount)
                                                  }).ToListAsync();


                if (reportFilter.PropertiesIds.Count() > 0 && reportFilter.PropertiesIds.Any())
                {
                    invoiceReportDtoList = invoiceReportDtoList
                        .Where(x => reportFilter.PropertiesIds.Contains(x.PropertyId))
                        .ToList();
                }

                if (reportFilter.UnitsIds.Count() > 0 && reportFilter.UnitsIds.Any())
                {
                    invoiceReportDtoList = invoiceReportDtoList
                        .Where(x => reportFilter.UnitsIds.Contains(x.UnitId))
                        .ToList();
                }

                if (reportFilter.LeaseStartDateFilter != null && reportFilter.LeaseEndDateFilter != null)
                {
                    invoiceReportDtoList = invoiceReportDtoList
                        .Where(x => x.StartDate >= reportFilter.LeaseStartDateFilter && x.EndDate <= reportFilter.LeaseEndDateFilter)
                        .ToList();
                }


                if (reportFilter.InvoiceStartDateFilter != null && reportFilter.InvoiceEndDateFilter != null)
                {
                    invoiceReportDtoList = invoiceReportDtoList
                        .Where(x => x.InvoiceDate >= reportFilter.InvoiceStartDateFilter && x.InvoiceDate <= reportFilter.InvoiceEndDateFilter)
                        .ToList();
                }

                return invoiceReportDtoList;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Invoices: {ex.Message}");
                throw;
            }
        }

        public async Task<List<TaskRequestReportDto>> GetTaskRequestReportAsync(ReportFilter reportFilter)
        {
            try
            {
                var taskRequestReports = await (from l in _db.LineItem
                                                from t in _db.TaskRequest.Where(x=>x.TaskRequestId == l.TaskRequestId).DefaultIfEmpty()
                                                join p in _db.Assets on t.AssetId equals p.AssetId into properties
                                                from p in properties.DefaultIfEmpty()
                                                where t.IsDeleted != true && (p == null || p.IsDeleted != true)
                                                select new TaskRequestReportDto
                                                {
                                                    Memo = l.Memo,
                                                    Quantity = l.Quantity,
                                                    Price = l.Price,
                                                    Total = l.Price * l.Quantity,
                                                    Task = t.Subject,
                                                    DueDate = t.DueDate,
                                                    StartDate = t.StartDate,
                                                    EndDate = t.EndDate,
                                                    PropertyId = p.AssetId,
                                                    Property = p != null ? p.BuildingNo + " - " + p.BuildingName : null,
                                                    UnitId = t.UnitId,
                                                    Unit = t.Unit.UnitName,
                                                    Status = t.Status
                                                }).ToListAsync();

                // Apply filters
                if (reportFilter.PropertiesIds != null && reportFilter.PropertiesIds.Any())
                {
                    taskRequestReports = taskRequestReports
                        .Where(x => reportFilter.PropertiesIds.Contains(x.PropertyId))
                        .ToList();
                }

                if (reportFilter.UnitsIds != null && reportFilter.UnitsIds.Any())
                {
                    taskRequestReports = taskRequestReports
                        .Where(x => reportFilter.UnitsIds.Contains(x.UnitId))
                        .ToList();
                }

                if (reportFilter.TaskStartDateFilter != null && reportFilter.TaskEndDateFilter != null)
                {
                    taskRequestReports = taskRequestReports
                        .Where(x => x.StartDate >= reportFilter.TaskStartDateFilter && x.EndDate <= reportFilter.TaskEndDateFilter)
                        .ToList();
                }

                if (reportFilter.TaskDueStartDateFilter != null && reportFilter.TaskDueEndDateFilter != null)
                {
                    taskRequestReports = taskRequestReports
                        .Where(x => x.DueDate >= reportFilter.TaskDueStartDateFilter && x.DueDate <= reportFilter.TaskDueEndDateFilter)
                        .ToList();
                }


                return taskRequestReports;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Task Requests: {ex.Message}");
                throw;
            }
        }

        public async Task<List<FinanceReportDto>> GetFinanceReportsAsync(ReportFilter reportFilter)
        {
            try
            {
                var financeReports1 = await (from l in _db.LineItem
                                            from t in _db.TaskRequest.Where(x => x.TaskRequestId == l.TaskRequestId).DefaultIfEmpty()
                                            join p in _db.Assets on t.AssetId equals p.AssetId into properties
                                            from p in properties.DefaultIfEmpty()
                                            where t.IsDeleted != true && (p == null || p.IsDeleted != true)
                                            select new FinanceReportDto
                                            {
                                                Total = l.Price * l.Quantity,
                                                Task = t.Subject,
                                                StartDate = t.StartDate,
                                                EndDate = t.EndDate,
                                                PropertyId = p.AssetId,
                                                Property = p != null ? p.BuildingNo + " - " + p.BuildingName : null,
                                                UnitId = t.UnitId,
                                                Unit = t.Unit.UnitName,
                                            }).ToListAsync();

                var financeReports2 = await (from i in _db.Invoices
                                                  join l in _db.Lease on i.LeaseId equals l.LeaseId into leaseGroup
                                                  from l in leaseGroup.Where(x => x.IsDeleted != true).DefaultIfEmpty()
                                                  join p in _db.Assets on l.AssetId equals p.AssetId into properties
                                                  from p in properties.Where(x => x.IsDeleted != true).DefaultIfEmpty()
                                                  join rc in _db.RentCharge on l.LeaseId equals rc.LeaseId into rentCharges
                                                  from rc in rentCharges.DefaultIfEmpty()
                                                  where i.IsDeleted != true
                                                  group new { i, l, p, rc } by new
                                                  {
                                                      i.InvoiceId,
                                                      l.LeaseId,
                                                      l.StartDate,
                                                      l.EndDate,
                                                      p.AssetId,
                                                      p.BuildingNo,
                                                      p.BuildingName,
                                                      l.SelectedUnit
                                                  } into g
                                                  select new FinanceReportDto
                                                  {
                                                      Invoice = g.Key.InvoiceId.ToString(),
                                                      StartDate = g.Key.StartDate,
                                                      EndDate = g.Key.EndDate,
                                                      PropertyId = (int?)g.Key.AssetId,
                                                      Property = g.Key.BuildingNo + " - " + g.Key.BuildingName,
                                                      Unit = g.Key.SelectedUnit,
                                                      RentCharges = g.Sum(x => x.rc.Amount)
                                                  }).ToListAsync();
                // Concatenate both lists
        var combinedReports = financeReports1.Concat(financeReports2).ToList();

        // Apply filters
        if (reportFilter.PropertiesIds != null && reportFilter.PropertiesIds.Any())
        {
            combinedReports = combinedReports
                .Where(x => reportFilter.PropertiesIds.Contains(x.PropertyId))
                .ToList();
        }

        if (reportFilter.UnitsIds != null && reportFilter.UnitsIds.Any())
        {
            combinedReports = combinedReports
                .Where(x => reportFilter.UnitsIds.Contains(x.UnitId))
                .ToList();
        }

        if (reportFilter.TaskStartDateFilter != null && reportFilter.TaskEndDateFilter != null)
        {
            combinedReports = combinedReports
                .Where(x => x.StartDate >= reportFilter.TaskStartDateFilter && x.EndDate <= reportFilter.TaskEndDateFilter)
                .ToList();
        }

        return combinedReports;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Task Requests: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UnitDTO>> GetUnitsByAssetAsync(ReportFilter reportFilter)
        {
            try
            {
                var units = await (from u in _db.AssetsUnits
                                    from a in _db.Assets.Where(x => x.AssetId == u.AssetId).DefaultIfEmpty()
                                    where u.IsDeleted != true && u.AddedBy == reportFilter.AddedBy
                                    select new UnitDTO
                                    {
                                        AssetId = u.AssetId,
                                        Asset = a.BuildingNo + " - " + a.BuildingName,
                                        UnitId = u.UnitId,
                                        UnitName = u.UnitName,
                                        Bath = u.Bath,
                                        Beds = u.Beds,
                                        Rent = u.Rent,
                                        Size = u.Size
                                    })
                     .AsNoTracking()
                     .ToListAsync();

                if (reportFilter.AssetsIds.Count() > 0 && reportFilter.AssetsIds.Any())
                {
                    units = units
                        .Where(x => reportFilter.AssetsIds.Contains(x.AssetId))
                        .ToList();
                }

                return units;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Assets: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Documents

        public async Task<List<DocumentsDto>> GetDocumentsAsync()
        {
            try
            {
                var result = await (from d in _db.Documents
                                    from a in _db.Assets.Where(x => x.AssetId == d.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == d.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == d.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == d.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                    where d.IsDeleted != true
                                    select new DocumentsDto
                                    {
                                        DocumentsId = d.DocumentsId,
                                        Title = d.Title,
                                        Description = d.Description,
                                        Type = d.Type,
                                        AssetId = d.AssetId,
                                        AssetName = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = d.UnitId,
                                        UnitName = u.UnitName,
                                        OwnerId = d.OwnerId,
                                        OwnerName = o.FirstName + " " + o.LastName,
                                        TenantId = d.TenantId,
                                        TenantName = tnt.FirstName + " " + tnt.LastName,
                                        DocumentName = d.DocumentUrl,
                                        DocumentUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Documents_DocumentUrl_" + d.DocumentsId + Path.GetExtension(d.DocumentUrl)}",
                                        AddedBy = d.AddedBy,
                                        CreatedDate = d.AddedDate,
                                        ModifiedDate = d.ModifiedDate
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Document: {ex.Message}");
                throw;
            }
        }

        public async Task<DocumentsDto> GetDocumentByIdAsync(int id)
        {
            try
            {
                var result = await (from d in _db.Documents
                                    from a in _db.Assets.Where(x => x.AssetId == d.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == d.UnitId).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == d.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == d.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                    where d.DocumentsId == id && d.IsDeleted != true
                                    select new DocumentsDto
                                    {
                                        DocumentsId = d.DocumentsId,
                                        Title = d.Title,
                                        Description = d.Description,
                                        Type = d.Type,
                                        AssetId = d.AssetId,
                                        AssetName = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = d.UnitId,
                                        UnitName = u.UnitName,
                                        OwnerId = d.OwnerId,
                                        OwnerName = o.FirstName + " " + o.LastName,
                                        TenantId = d.TenantId,
                                        TenantName = tnt.FirstName + " " + tnt.LastName,
                                        DocumentName = d.DocumentUrl,
                                        DocumentUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Documents_DocumentUrl_" + d.DocumentsId + Path.GetExtension(d.DocumentUrl)}",
                                        AddedBy = d.AddedBy,

                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Document: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveDocumentAsync(DocumentsDto model)
        {
            var document = _db.Documents.FirstOrDefault(x => x.DocumentsId == model.DocumentsId && x.IsDeleted != true);

            if (document == null)
                document = new Documents();

            document.DocumentsId = model.DocumentsId;
            document.Title = model.Title;
            document.Description = model.Description;
            document.Type = model.Type;
            document.AssetId = model.AssetId;
            document.UnitId = model.UnitId;
            document.OwnerId = model.OwnerId;
            document.TenantId = model.TenantId;

            if (model.DocumentUrl != null)
            {
                document.DocumentUrl = model.DocumentName;
            }

            if (document.DocumentsId > 0)
            {
                document.ModifiedBy = model.AddedBy;
                document.ModifiedDate = DateTime.Now;
                _db.Documents.Update(document);
            }
            else
            {
                document.AddedBy = model.AddedBy;
                document.AddedDate = DateTime.Now;
                _db.Documents.Add(document);
            }

            var result = await _db.SaveChangesAsync();

            if (model.DocumentUrl != null)
            {
                var ext = Path.GetExtension(model.DocumentName);
                await _googleCloudStorageService.UploadImagebyBase64Async(model.DocumentUrl, "Documents_DocumentUrl_" + document.DocumentsId + ext);
            }

            return result > 0;
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _db.Documents.FindAsync(id);
            if (document == null) return false;

            document.IsDeleted = true;
            _db.Documents.Update(document);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
        }



        public async Task<List<DocumentsDto>> GetDocumentByAssetAsync(int assetId)
        {
            try
            {
                var result = await (from d in _db.Documents
                                    from a in _db.Assets.Where(x => x.AssetId == d.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                    from u in _db.AssetsUnits.Where(x => x.UnitId == d.UnitId).DefaultIfEmpty()
                                    where d.AssetId == assetId && d.IsDeleted != true
                                    select new DocumentsDto
                                    {
                                        DocumentsId = d.DocumentsId,
                                        Title = d.Title,
                                        Description = d.Description,
                                        Type = d.Type,
                                        AssetId = d.AssetId,
                                        AssetName = a.BuildingNo + "-" + a.BuildingName,
                                        UnitId = d.UnitId,
                                        UnitName = u.UnitName,
                                        DocumentName = d.DocumentUrl,
                                        DocumentUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Documents_DocumentUrl_" + d.DocumentsId + Path.GetExtension(d.DocumentUrl)}",
                                        CreatedDate = d.AddedDate,
                                        AddedBy = d.AddedBy,

                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Document: {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<DocumentsDto>> GetDocumentByTenantAsync(int tenantId)
        {
            try
            {
                var result = await (from d in _db.Documents
                                    from t in _db.Tenant.Where(x => x.TenantId == d.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                    where d.TenantId == tenantId && d.IsDeleted != true
                                    select new DocumentsDto
                                    {
                                        DocumentsId = d.DocumentsId,
                                        Title = d.Title,
                                        Description = d.Description,
                                        Type = d.Type,
                                        TenantId = d.TenantId,
                                        TenantName = t.FirstName + " " + t.LastName,
                                        DocumentName = d.DocumentUrl,
                                        DocumentUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Documents_DocumentUrl_" + d.DocumentsId + Path.GetExtension(d.DocumentUrl)}",
                                        CreatedDate = d.AddedDate,
                                        AddedBy = d.AddedBy,

                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Document: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DocumentsDto>> GetDocumentByLandLordAsync(int landlordId)
        {
            try
            {
                var result = await (from d in _db.Documents
                                    from o in _db.Owner.Where(x => x.OwnerId == d.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                    where d.OwnerId == landlordId && d.IsDeleted != true
                                    select new DocumentsDto
                                    {
                                        DocumentsId = d.DocumentsId,
                                        Title = d.Title,
                                        Description = d.Description,
                                        Type = d.Type,
                                        OwnerId = d.OwnerId,
                                        OwnerName = o.FirstName + " " + o.LastName,
                                        DocumentName = d.DocumentUrl,
                                        DocumentUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Documents_DocumentUrl_" + d.DocumentsId + Path.GetExtension(d.DocumentUrl)}",
                                        CreatedDate = d.AddedDate,
                                        AddedBy = d.AddedBy,

                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Document: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region SupportCenter

        public async Task<List<FAQ>> GetFAQsAsync()
        {
            try
            {
                var result = await (from faq in _db.FAQs
                                    where faq.IsDeleted != true
                                    select new FAQ
                                    {
                                        FAQId = faq.FAQId,
                                        Question = faq.Question,
                                        Answer = faq.Answer,
                                        AddedBy = faq.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping FAQ: {ex.Message}");
                throw;
            }
        }

        public async Task<List<VideoTutorial>> GetVideoTutorialsAsync()
        {
            try
            {
                var result = await (from vt in _db.VideoTutorial
                                    where vt.IsDeleted != true
                                    select new VideoTutorial
                                    {
                                        TutorialId = vt.TutorialId,
                                        Title = vt.Title,
                                        VideoLink = vt.VideoLink,
                                        AddedBy = vt.AddedBy,
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Video Tutorial: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region LateFee

        //Task<LateFeeDto> GetLateFeeAsync(Filter filter);
        //Task<LateFeeAssetDto> GetLateFeeByAssetAsync(int assetId);

        public async Task<LateFeeDto> GetLateFeeAsync(Filter filter)
        {
            try
            {
                var result = await (from lf in _db.LateFees
                                    where lf.AddedBy == filter.AddedBy && lf.IsDeleted != true
                                    select new LateFeeDto
                                    {
                                        LateFeeId = lf.LateFeeId,
                                        DueDays = lf.DueDays,
                                        ChargeLateFeeActive = lf.ChargeLateFeeActive,
                                        Frequency = lf.Frequency,
                                        CalculateFee = lf.CalculateFee,
                                        Amount = lf.Amount,
                                        ChartAccountId = lf.ChartAccountId,
                                        Description = lf.Description,
                                        IsSendARemainder = lf.IsSendARemainder,
                                        IsNotifyTenants = lf.IsNotifyTenants,
                                        IsEnableSms = lf.IsEnableSms,
                                        IsChargeLateFee = lf.IsChargeLateFee,
                                        IsMonthlyLimit = lf.IsMonthlyLimit,
                                        IsDailyLimit = lf.IsDailyLimit,
                                        IsMinimumBalance = lf.IsMinimumBalance,
                                        IsChargeLateFeeonSpecific = lf.IsChargeLateFeeonSpecific,
                                        AddedBy = lf.AddedBy,
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Chart Account: {ex.Message}");
                throw;
            }
        }

        public async Task<LateFeeAssetDto> GetLateFeeByAssetAsync(int assetId)
        {
            try
            {
                var result = await (from lfa in _db.LateFeeAsset
                                    where lfa.AssetId == assetId && lfa.IsDeleted != true
                                    select new LateFeeAssetDto
                                    {
                                        LateFeeAssetId = lfa.LateFeeAssetId,
                                        AssetId = lfa.AssetId,
                                        CompanyDefaultStructure = lfa.CompanyDefaultStructure,
                                        SpecifyLateFeeStructure = lfa.SpecifyLateFeeStructure,
                                        DueDays = lfa.DueDays,
                                        Frequency = lfa.Frequency,
                                        CalculateFee = lfa.CalculateFee,
                                        Amount = lfa.Amount,
                                        ChartAccountId = lfa.ChartAccountId,
                                        Description = lfa.Description,
                                        IsSendARemainder = lfa.IsSendARemainder,
                                        IsNotifyTenants = lfa.IsNotifyTenants,
                                        IsEnableSms = lfa.IsEnableSms,
                                        IsChargeLateFee = lfa.IsChargeLateFee,
                                        IsMonthlyLimit = lfa.IsMonthlyLimit,
                                        IsDailyLimit = lfa.IsDailyLimit,
                                        IsMinimumBalance = lfa.IsMinimumBalance,
                                        IsChargeLateFeeonSpecific = lfa.IsChargeLateFeeonSpecific,
                                        AddedBy = lfa.AddedBy
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Chart Account: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveLateFeeAsync(LateFeeDto model)
        {
            var lateFee = _db.LateFees.FirstOrDefault(x => x.LateFeeId == model.LateFeeId && x.IsDeleted != true);

            if (lateFee == null)
                lateFee = new LateFee();

            lateFee.LateFeeId = model.LateFeeId;
            lateFee.ChargeLateFeeActive = model.ChargeLateFeeActive;
            lateFee.DueDays = model.DueDays ;
            lateFee.Frequency = model.Frequency ;
            lateFee.CalculateFee = model.CalculateFee ;
            lateFee.Amount = model.Amount ;
            lateFee.ChartAccountId = model.ChartAccountId;
            lateFee.Description = model.Description;
            lateFee.IsSendARemainder = model.IsSendARemainder ;
            lateFee.IsNotifyTenants = model.IsNotifyTenants ;
            lateFee.IsEnableSms = model.IsEnableSms ;
            lateFee.IsChargeLateFee = model.IsChargeLateFee ;
            lateFee.IsMonthlyLimit = model.IsMonthlyLimit ;
            lateFee.IsDailyLimit = model.IsDailyLimit ;
            lateFee.IsMinimumBalance = model.IsMinimumBalance ;
            lateFee.IsChargeLateFeeonSpecific = model.IsChargeLateFeeonSpecific ;
            lateFee.AddedBy = model.AddedBy;

            if (lateFee.LateFeeId > 0)
            {
                lateFee.ModifiedBy = model.AddedBy;
                lateFee.ModifiedDate = DateTime.Now;
                _db.LateFees.Update(lateFee);
            }
            else
            {
                lateFee.AddedBy = model.AddedBy;
                lateFee.AddedDate = DateTime.Now;
                _db.LateFees.Add(lateFee);
            }

            var result = await _db.SaveChangesAsync();


            return result > 0;

        }
        
        public async Task<bool> SaveLateFeeAssetAsync(LateFeeAssetDto model)
        {
            
                var lateFeeAsset = _db.LateFeeAsset.FirstOrDefault(x => x.LateFeeAssetId == model.LateFeeAssetId && x.IsDeleted != true);

                if (lateFeeAsset == null)
                lateFeeAsset = new LateFeeAsset();
            if (model.SpecifyLateFeeStructure == true)
            {
                lateFeeAsset.LateFeeAssetId = model.LateFeeAssetId;
                lateFeeAsset.AssetId = model.AssetId;
                lateFeeAsset.CompanyDefaultStructure = false;
                lateFeeAsset.SpecifyLateFeeStructure = true;
                lateFeeAsset.DueDays = model.DueDays;
                lateFeeAsset.Frequency = model.Frequency;
                lateFeeAsset.CalculateFee = model.CalculateFee;
                lateFeeAsset.Amount = model.Amount;
                lateFeeAsset.ChartAccountId = model.ChartAccountId;
                lateFeeAsset.Description = model.Description;
                lateFeeAsset.IsSendARemainder = model.IsSendARemainder;
                lateFeeAsset.IsNotifyTenants = model.IsNotifyTenants;
                lateFeeAsset.IsEnableSms = model.IsEnableSms;
                lateFeeAsset.IsChargeLateFee = model.IsChargeLateFee;
                lateFeeAsset.IsMonthlyLimit = model.IsMonthlyLimit;
                lateFeeAsset.IsDailyLimit = model.IsDailyLimit;
                lateFeeAsset.IsMinimumBalance = model.IsMinimumBalance;
                lateFeeAsset.IsChargeLateFeeonSpecific = model.IsChargeLateFeeonSpecific;
                lateFeeAsset.AddedBy = model.AddedBy;
            }
            else
            {
                model.CompanyDefaultStructure = true;
                model.SpecifyLateFeeStructure = false;
            }

            if (lateFeeAsset.LateFeeAssetId > 0)
            {
                lateFeeAsset.ModifiedBy = model.AddedBy;
                lateFeeAsset.ModifiedDate = DateTime.Now;
                _db.LateFeeAsset.Update(lateFeeAsset);
            }
            else
            {
                lateFeeAsset.AddedBy = model.AddedBy;
                lateFeeAsset.AddedDate = DateTime.Now;
                _db.LateFeeAsset.Add(lateFeeAsset);
            }

            var result = await _db.SaveChangesAsync();


            return result > 0;

        }

        #endregion
        #region Stripe subscription
        public async Task<bool> SavePaymentGuidAsync(PaymentGuidDto paymentGuidDto)
        {
            var paymentGuid = new PaymentGuid();

            paymentGuid.Guid = paymentGuidDto.Guid;
            paymentGuid.Description = paymentGuidDto.Description ;
            paymentGuid.DateTime = paymentGuidDto.DateTime ;
            paymentGuid.SessionId = paymentGuidDto.SessionId ;
            paymentGuid.UserId = paymentGuidDto.UserId ;
            paymentGuid.AddedBy = paymentGuidDto.AddedBy;
            paymentGuid.AddedDate = DateTime.Now;
            _db.PaymentGuids.Add(paymentGuid);

            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
        public async Task<bool> SavePaymentInformationAsync(PaymentInformationDto paymentInformationDto)
        {
            var paymentInformation = new PaymentInformation();
            paymentInformation.Id = paymentInformationDto.Id;
            paymentInformation.ProductPrice = paymentInformationDto.ProductPrice;
            paymentInformation.AmountCharged = paymentInformationDto.AmountCharged;
            paymentInformation.ChargeDate = paymentInformationDto.ChargeDate;
            paymentInformation.TransactionId = paymentInformationDto.TransactionId;
            paymentInformation.PaymentStatus = paymentInformationDto.PaymentStatus;
            paymentInformation.Currency = paymentInformationDto.Currency;
            paymentInformation.CustomerId = paymentInformationDto.CustomerId;

            paymentInformation.AddedBy = paymentInformationDto.AddedBy;
            paymentInformation.AddedDate = DateTime.Now;
            _db.PaymentInformations.Add(paymentInformation);

            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<bool> SavePaymentMethodInformationAsync(PaymentMethodInformationDto paymentMethodInformationDto)
        {
            var paymentMethodInformation = new PaymentMethodInformation();
            paymentMethodInformation.Id = paymentMethodInformationDto.Id;
            paymentMethodInformation.Country = paymentMethodInformationDto.Country;
            paymentMethodInformation.CardType = paymentMethodInformationDto.CardType;
            paymentMethodInformation.CardHolderName = paymentMethodInformationDto.CardHolderName;
            paymentMethodInformation.CardLast4Digit = paymentMethodInformationDto.CardLast4Digit;
            paymentMethodInformation.ExpiryMonth = paymentMethodInformationDto.ExpiryMonth;
            paymentMethodInformation.ExpiryYear = paymentMethodInformationDto.ExpiryYear;
            paymentMethodInformation.Email = paymentMethodInformationDto.Email;
            paymentMethodInformation.GUID = paymentMethodInformationDto.GUID;
            paymentMethodInformation.PaymentMethodId = paymentMethodInformationDto.PaymentMethodId;
            paymentMethodInformation.CustomerId = paymentMethodInformationDto.CustomerId;

            paymentMethodInformation.AddedBy = paymentMethodInformationDto.AddedBy;
            paymentMethodInformation.AddedDate = DateTime.Now;
            _db.PaymentMethodInformations.Add(paymentMethodInformation);

            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> SaveStripeSubscriptionAsync(StripeSubscriptionDto stripeSubscriptionDto)
        {
            var stripeSubscription = new StripeSubscription();
            stripeSubscription.Id = stripeSubscriptionDto.Id;
            stripeSubscription.UserId = stripeSubscriptionDto.UserId;
            stripeSubscription.StartDate = stripeSubscriptionDto.StartDate;
            stripeSubscription.EndDate = stripeSubscriptionDto.EndDate;
            stripeSubscription.SubscriptionId = stripeSubscriptionDto.SubscriptionId;
            stripeSubscription.EmailAddress = stripeSubscriptionDto.EmailAddress;
            stripeSubscription.IsCanceled = stripeSubscriptionDto.IsCanceled;
            stripeSubscription.BillingInterval = stripeSubscriptionDto.BillingInterval;
            stripeSubscription.SubscriptionType = stripeSubscriptionDto.SubscriptionType;
            stripeSubscription.IsTrial = stripeSubscriptionDto.IsTrial;
            stripeSubscription.GUID = stripeSubscriptionDto.GUID;
            stripeSubscription.Status = stripeSubscriptionDto.Status;
            stripeSubscription.Currency = stripeSubscriptionDto.Currency;
            stripeSubscription.CustomerId = stripeSubscriptionDto.CustomerId;

            stripeSubscription.AddedBy = stripeSubscriptionDto.AddedBy;
            stripeSubscription.AddedDate = DateTime.Now;
            _db.StripeSubscriptions.Add(stripeSubscription);

            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<StripeSubscriptionDto> CheckTrialDaysAsync(string currentUserId)
        {
            try
            {
                var result = await _db.StripeSubscriptions
                                      .Where(x => x.UserId == currentUserId && !x.IsCanceled && x.IsDeleted != true)
                                      .Select(s => new StripeSubscriptionDto
                                      {
                                          Id = s.Id,
                                          UserId = s.UserId,
                                          StartDate = s.StartDate,
                                          EndDate = s.EndDate,
                                          IsTrial = s.IsTrial,
                                          AddedBy = s.AddedBy,
                                      })
                                      .FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping Subscription User: {ex.Message}");
                throw;
            }
        }



        #endregion

    }
}
