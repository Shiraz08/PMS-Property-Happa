using AutoMapper;
using Hangfire.Common;
using Humanizer.Localisation;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.ContentModel;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Twilio.Types;
using static PMS_PropertyHapa.Models.DTO.TenantModelDto;
using static PMS_PropertyHapa.Shared.Enum.SD;
using static System.Net.WebRequestMethods;

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

        public UserRepository(ApiDbContext db, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
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
                                 .AsNoTracking()
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
                OrganizationLogo = tenantOrganization?.OrganizationLogo,
                OrganizationIcon = tenantOrganization?.OrganizationIcon,
                Tid = tenantOrganization?.Id,
                TenantId = user.TenantId
            };
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
                var userDTOs = await _userManager.Users
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

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user != null)
            {
                //var otpCode = GenerateOTP();

                //var otpRecord = new OTP
                //{
                //    PhoneNumber = phoneNumber,
                //    Type = "PhoneNumber",
                //    Code = otpCode,
                //    Expiry = DateTime.UtcNow.AddMinutes(5)
                //};

                //_db.OTP.Add(otpRecord);
                //await _db.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> SavePhoneOTP(OTPDto oTPEmailDto)
        {
            //var otpCode = GenerateOTP();
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
            //var otpCode = GenerateOTP();
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

        //public async Task<bool> FindByEmailAddressAsync(string email)
        //{
        //    if (string.IsNullOrWhiteSpace(email))
        //    {
        //        return false;
        //    }

        //    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null)
        //    {
        //        var otpCode = GenerateOTP();

        //        var otpRecord = new OTP
        //        {
        //            //AppTenantId = user.Id,
        //            PhoneNumber = false,
        //            Email = true,
        //            Type = "Email",
        //            Code = otpCode,
        //            Expiry = DateTime.UtcNow.AddMinutes(5)
        //        };

        //        _db.OTP.Add(otpRecord);
        //        await _db.SaveChangesAsync();

        //        return true;
        //    }

        //    return false;
        //}

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
                                 .FirstOrDefaultAsync(u => u.Id == userId);
        }


        public async Task<ProfileModel> GetProfileModelAsync(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId);

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
                NewPictureBase64 = user.Picture,
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


        public async Task<List<PropertyTypeDto>> GetAllPropertyTypes()
        {
            try
            {
                var propertyTypes = await _db.PropertyType
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


        public async Task<List<PropertyTypeDto>> GetAllPropertyTypesAsync()
        {
            try
            {
                var propertyTypes = await _db.PropertyType
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
                                   .Where(t => t.AppTenantId == Guid.Parse(tenantId))
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
            var tenant = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == propertytypeId);

            if (tenant == null)
                return new PropertyTypeDto();

            var tenantDto = new PropertyTypeDto
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
            };

            return tenantDto;
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
            var propertyType = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == tenant.PropertyTypeId);

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
            var tenant = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == tenantId);
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
                                                .Where(t => t.AppTenantId == Guid.Parse(tenantId))
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



        public async Task<List<PropertySubTypeDto>> GetPropertySubTypeByIdAsync(int propertytypeId)
        {
            var tenants = await _db.PropertySubType
                                   .AsNoTracking()
                                   .Where(t => t.PropertyTypeId == propertytypeId)
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
            var tenant = await _db.PropertySubType.FirstOrDefaultAsync(t => t.PropertySubTypeId == propertysubtypeId);

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
            var propertyType = await _db.PropertyType.FirstOrDefaultAsync(t => t.PropertyTypeId == tenant.PropertyTypeId);
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
            var tenant = await _db.PropertySubType.FirstOrDefaultAsync(t => t.PropertySubTypeId == propertysubtypeId);
            if (tenant == null) return false;

            _db.PropertySubType.Remove(tenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }



        #endregion


        #region Tenant
        public async Task<IEnumerable<TenantModelDto>> GetAllTenantsAsync()
        {
            var tenants = await _db.Tenant
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
                Picture = tenant.Picture,
                Document = tenant.Document,
                AddedBy = tenant.AddedBy,
                AppTid = tenant.AppTenantId.ToString()
            }).ToList();
            return tenantDtos;
        }


        public async Task<List<TenantModelDto>> GetTenantsByIdAsync(string tenantId)
        {
            var tenants = await _db.Tenant
                                   .AsNoTracking()
                                   .Where(t => t.AppTenantId == Guid.Parse(tenantId))
                                   .ToListAsync();

            if (tenants == null || !tenants.Any()) return new List<TenantModelDto>();

            // Manual mapping from Tenant to TenantModelDto
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
                Picture = tenant.Picture,
                Document = tenant.Document
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
                  .FirstOrDefaultAsync(t => t.TenantId == tenantId);

            if (tenant == null)
                return new TenantModelDto();

            var tenantDto = new TenantModelDto
            {
                TenantId = tenant.TenantId,
                Picture = tenant.Picture,
                Document = tenant.Document,
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
                Pets = tenant.Pets.Select(p => new PetDto
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Breed = p.Breed,
                    Type = p.Type,
                    Quantity = p.Quantity,
                    Picture = p.Picture
                }).ToList(),
                Vehicles = tenant.Vehicle.Select(v => new VehicleDto
                {
                    VehicleId = v.VehicleId,
                    Manufacturer = v.Manufacturer,
                    ModelName = v.ModelName,
                    ModelVariant = v.ModelVariant,
                    Year = v.Year
                }).ToList(),
                Dependent = tenant.TenantDependent.Select(d => new TenantDependentDto
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
                CoTenant = tenant.CoTenant.Select(c => new CoTenantDto
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
                Picture = tenantDto.Picture,
                Document = tenantDto.Document,
                AddedBy = tenantDto.AddedBy,
                AddedDate = DateTime.Now
            };

            _db.Tenant.Add(newTenant);
            await _db.SaveChangesAsync();


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
                        Picture = petDto.Picture,
                        AppTenantId = tenantDto.AppTenantId
                    };

                    _db.Pets.Add(pet);
                }
                await _db.SaveChangesAsync();
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
                        Year = vehicleDto.Year
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
                        AppTenantId = tenantDto.AppTenantId
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
                        AppTenantId = tenantDto.AppTenantId
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
            var tenant = await _db.Tenant.Include(t => t.Pets).Include(t => t.Vehicle).Include(t => t.TenantDependent).Include(t => t.CoTenant).FirstOrDefaultAsync(t => t.TenantId == tenantDto.TenantId);
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

            // Update or add pets
            foreach (var petDto in tenantDto.Pets)
            {
                var existingPet = tenant.Pets.FirstOrDefault(p => p.PetId == petDto.PetId);
                if (existingPet != null)
                {
                    existingPet.Name = petDto.Name;
                    existingPet.Breed = petDto.Breed;
                    existingPet.Type = petDto.Type;
                    existingPet.Quantity = petDto.Quantity;
                    existingPet.Picture = petDto.Picture;
                    existingPet.AppTenantId = petDto.AppTenantId;
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
                        Picture = petDto.Picture,
                        AppTenantId = tenantDto.AppTenantId
                    };
                    tenant.Pets.Add(newPet);
                }
            }

            // Update or add vehicles
            foreach (var vehicleDto in tenantDto.Vehicles)
            {
                var existingVehicle = tenant.Vehicle.FirstOrDefault(v => v.VehicleId == vehicleDto.VehicleId);
                if (existingVehicle != null)
                {
                    existingVehicle.Manufacturer = vehicleDto.Manufacturer;
                    existingVehicle.ModelName = vehicleDto.ModelName;
                    existingVehicle.ModelVariant = vehicleDto.ModelVariant;
                    existingVehicle.Year = vehicleDto.Year;
                }
                else
                {
                    var newVehicle = new Vehicle
                    {
                        TenantId = tenant.TenantId,
                        Manufacturer = vehicleDto.Manufacturer,
                        ModelName = vehicleDto.ModelName,
                        Year = vehicleDto.Year
                    };
                    tenant.Vehicle.Add(newVehicle);
                }
            }

            foreach (var dependentDto in tenantDto.Dependent)
            {
                var existingDependent = tenant.TenantDependent.FirstOrDefault(d => d.TenantDependentId == dependentDto.TenantDependentId);
                if (existingDependent != null)
                {
                    existingDependent.FirstName = dependentDto.FirstName;
                    existingDependent.LastName = dependentDto.LastName;
                    existingDependent.EmailAddress = dependentDto.EmailAddress;
                    existingDependent.PhoneNumber = dependentDto.PhoneNumber;
                    existingDependent.DOB = dependentDto.DOB;
                    existingDependent.Relation = dependentDto.Relation;
                    existingDependent.AppTenantId = tenantDto.AppTenantId;
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
                        AppTenantId = tenantDto.AppTenantId
                    };
                    tenant.TenantDependent.Add(newDependent);
                }
            }

            foreach (var coTenantDto in tenantDto.CoTenant)
            {
                var existingCoTenant = tenant.CoTenant.FirstOrDefault(ct => ct.CoTenantId == coTenantDto.CoTenantId);
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
                        AppTenantId = tenantDto.AppTenantId
                    };
                    tenant.CoTenant.Add(newCoTenant);
                }
            }
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }


        public async Task<bool> DeleteTenantAsync(string tenantId)
        {
            var tenant = await _db.Tenant.FirstOrDefaultAsync(t => t.TenantId == Convert.ToInt32(tenantId));
            if (tenant == null) return false;

            _db.Tenant.Remove(tenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }



        #endregion



        #region Landlord

        public async Task<OwnerDto> GetSingleLandlordByIdAsync(int ownerId)
        {
            var tenantDto = await _db.Owner.FirstOrDefaultAsync(t => t.OwnerId == ownerId);

            if (tenantDto == null)
                return new OwnerDto();
            var tenant = new OwnerDto
            {
                OwnerId = tenantDto.OwnerId,
                FirstName = tenantDto.FirstName,
                MiddleName = tenantDto.MiddleName,
                LastName = tenantDto.LastName,
                EmailAddress = tenantDto.EmailAddress,
                EmailAddress2 = tenantDto.EmailAddress2,
                PhoneNumber = tenantDto.PhoneNumber,
                PhoneNumber2 = tenantDto.PhoneNumber2,
                Fax = tenantDto.Fax,
                TaxId = tenantDto.TaxId,
                Document = tenantDto.Document,
                EmergencyContactInfo = tenantDto.EmergencyContactInfo,
                LeaseAgreementId = tenantDto.LeaseAgreementId,
                OwnerNationality = tenantDto.OwnerNationality,
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
                AppTenantId = tenantDto.AppTenantId,
                Address = tenantDto.Address,
                Address2 = tenantDto.Address2,
                Locality = tenantDto.Locality,
                Region = tenantDto.Region,
                PostalCode = tenantDto.PostalCode,
                Country = tenantDto.Country,
                CountryCode = tenantDto.CountryCode,
                Picture = tenantDto.Picture
            };

            var ownerOrganization = await _db.OwnerOrganization.FirstOrDefaultAsync(o => o.OwnerId == ownerId);
            if (ownerOrganization != null)
            {
                tenant.OrganizationName = ownerOrganization.OrganizationName;
                tenant.OrganizationDescription = ownerOrganization.OrganizationDescription;
                tenant.OrganizationIcon = ownerOrganization.OrganizationIcon;
                tenant.OrganizationLogo = ownerOrganization.OrganizationLogo;
                tenant.Website = ownerOrganization.Website;
            }
            else
            {

                tenant.OrganizationName = "";
                tenant.OrganizationDescription = "";
                tenant.OrganizationIcon = "";
                tenant.OrganizationLogo = "";
                tenant.Website = "";
            }

            return tenant;
        }



        public async Task<bool> CreateOwnerAsync(OwnerDto tenantDto)
        {
            var newTenant = new Owner
            {
                FirstName = tenantDto.FirstName,
                MiddleName = tenantDto.MiddleName,
                LastName = tenantDto.LastName,
                EmailAddress = tenantDto.EmailAddress,
                EmailAddress2 = tenantDto.EmailAddress2,
                PhoneNumber = tenantDto.PhoneNumber,
                PhoneNumber2 = tenantDto.PhoneNumber2,
                Fax = tenantDto.Fax,
                TaxId = tenantDto.TaxId,
                EmergencyContactInfo = tenantDto.EmergencyContactInfo,
                LeaseAgreementId = tenantDto.LeaseAgreementId,
                OwnerNationality = tenantDto.OwnerNationality,
                Gender = tenantDto.Gender,
                DOB = tenantDto.DOB,
                VAT = tenantDto.VAT,
                Status = true,
                LegalName = tenantDto.LegalName,
                Document = tenantDto.Document,
                Account_Name = tenantDto.Account_Name,
                Account_Holder = tenantDto.Account_Holder,
                Account_IBAN = tenantDto.Account_IBAN,
                Account_Swift = tenantDto.Account_Swift,
                Account_Bank = tenantDto.Account_Bank,
                Account_Currency = tenantDto.Account_Currency,
                AppTenantId = tenantDto.AppTenantId,
                Address = tenantDto.Address,
                Address2 = tenantDto.Address2,
                Locality = tenantDto.Locality,
                Region = tenantDto.Region,
                PostalCode = tenantDto.PostalCode,
                Country = tenantDto.Country,
                CountryCode = tenantDto.CountryCode,
                Picture = tenantDto.Picture,
                AddedBy = tenantDto.AddedBy,
                AddedDate = DateTime.Now
            };



            await _db.Owner.AddAsync(newTenant);
            await _db.SaveChangesAsync();


            var newOwnerOrganization = new OwnerOrganization
            {
                OwnerId = newTenant.OwnerId,
                OrganizationName = tenantDto.OrganizationName,
                OrganizationDescription = tenantDto.OrganizationDescription,
                OrganizationIcon = tenantDto.OrganizationIcon,
                OrganizationLogo = tenantDto.OrganizationLogo,
                Website = tenantDto.Website
            };
            await _db.OwnerOrganization.AddAsync(newOwnerOrganization);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }


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
                    PropertyId = leaseDto.PropertyId,
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
                            RentPeriod = rentChargeDto.RentPeriod
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
                        };

                        await _db.SecurityDeposit.AddAsync(securityDeposit);
                        await _db.SaveChangesAsync();
                    }
                }

                if (leaseDto.FeeCharges != null)
                {
                    foreach (var feeDto in leaseDto.FeeCharges)
                    {
                        var securityDeposit = new PMS_PropertyHapa.Models.Entities.FeeCharge
                        {
                            LeaseId = newLease.LeaseId,
                            Amount = feeDto.Amount,
                            Description = feeDto.Description,
                            FeeDate = feeDto.FeeDate,
                        };

                        await _db.FeeCharge.AddAsync(securityDeposit);
                        await _db.SaveChangesAsync();
                    }
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

                if (leaseDto.FeeCharges != null)
                {
                    totalRentAmount += leaseDto.FeeCharges.Sum(fc => fc.Amount);
                }

                //var ownerName = await _db.Assets.FirstOrDefaultAsync(x => x.AssetId == leaseDto.PropertyId).OwnerId;

                if (leaseDto.IsMonthToMonth)
                {
                    DateTime invoiceDate = leaseDto.StartDate;
                    while (invoiceDate <= leaseDto.EndDate)
                    {
                        var newInvoice = new Invoice
                        {
                            LeaseId = maxLeaseId,
                            //OwnerId = leaseDto.OwnerId,
                            OwnerName = "Test",
                            InvoiceCreatedDate = DateTime.Now,
                            TenantId = leaseDto.TenantId,
                            TenantName = leaseDto.TenantIdValue,
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
                        //OwnerId = leaseDto.OwnerId,
                        OwnerName = "Test",
                        InvoiceCreatedDate = DateTime.Now,
                        TenantId = leaseDto.TenantId,
                        TenantName = leaseDto.TenantIdValue,
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
                .Where(l => l.LeaseId == leaseId)
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
                RentCharges = lease.RentCharges.Select(rc => new RentChargeDto
                {

                    RentChargeId = rc.RentChargeId,
                    Amount = rc.Amount,
                    Description = rc.Description,
                    RentDate = rc.RentDate,
                    RentPeriod = rc.RentPeriod
                }).ToList(),
                FeeCharges = lease.FeeCharge.Select(rc => new FeeChargeDto
                {
                    FeeChargeId = rc.FeeChargeId,
                    Amount = rc.Amount,
                    Description = rc.Description,
                    FeeDate = rc.FeeDate
                }).ToList(),

                SecurityDeposits = lease.SecurityDeposit.Select(sd => new SecurityDepositDto
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
                    // Map other properties as needed
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
                    RentCharges = lease.RentCharges.Select(rc => new RentChargeDto
                    {

                        RentChargeId = rc.RentChargeId,
                        Amount = rc.Amount,
                        Description = rc.Description,
                        RentDate = rc.RentDate,
                        RentPeriod = rc.RentPeriod
                    }).ToList() ?? new List<RentChargeDto>(),
                    FeeCharges = lease.FeeCharge.Select(rc => new FeeChargeDto
                    {
                        FeeChargeId = rc.FeeChargeId,
                        Amount = rc.Amount,
                        Description = rc.Description,
                        FeeDate = rc.FeeDate
                    }).ToList() ?? new List<FeeChargeDto>(),

                    SecurityDeposits = lease.SecurityDeposit.Select(sd => new SecurityDepositDto
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
                        // Map other properties as needed
                    } : null
                }).ToList();

                return leaseDtos;
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
                    .FirstOrDefaultAsync(l => l.LeaseId == leaseDto.LeaseId);

                if (existingLease == null)
                {
                    return false;
                }

                existingLease.StartDate = leaseDto.StartDate;
                existingLease.EndDate = leaseDto.EndDate;
                existingLease.IsSigned = leaseDto.IsSigned;
                existingLease.SelectedProperty = leaseDto.SelectedProperty;
                existingLease.PropertyId = leaseDto.PropertyId;
                existingLease.SelectedUnit = leaseDto.SelectedUnit;
                existingLease.SignatureImagePath = leaseDto.SignatureImagePath;
                existingLease.IsFixedTerm = leaseDto.IsFixedTerm;
                existingLease.IsMonthToMonth = leaseDto.IsMonthToMonth;
                existingLease.HasSecurityDeposit = leaseDto.HasSecurityDeposit;
                existingLease.LateFeesPolicy = leaseDto.LateFeesPolicy;
                existingLease.TenantsTenantId = leaseDto.TenantId;
                existingLease.AppTenantId = leaseDto.AppTenantId;


                if (leaseDto.RentCharges != null)
                {
                    foreach (var rcDto in leaseDto.RentCharges)
                    {
                        var existingRc = existingLease.RentCharges.FirstOrDefault(rc => rc.RentChargeId == rcDto.RentChargeId);
                        if (existingRc != null)
                        {
                            // Update existing RentCharge
                            existingRc.Amount = rcDto.Amount;
                            existingRc.Description = rcDto.Description;
                            existingRc.RentDate = rcDto.RentDate;
                            existingRc.RentPeriod = rcDto.RentPeriod;
                        }
                        else
                        {

                            existingLease.RentCharges.Add(new RentCharge
                            {
                                Amount = rcDto.Amount,
                                Description = rcDto.Description,
                                LeaseId = existingLease.LeaseId,
                                RentDate = rcDto.RentDate,
                                RentPeriod = rcDto.RentPeriod
                            });
                        }
                    }
                }


                if (leaseDto.FeeCharges != null)
                {
                    foreach (var rcDto in leaseDto.FeeCharges)
                    {
                        var existingRc = existingLease.FeeCharge.FirstOrDefault(rc => rc.FeeChargeId == rcDto.FeeChargeId);
                        if (existingRc != null)
                        {
                            // Update existing RentCharge
                            existingRc.Amount = rcDto.Amount;
                            existingRc.Description = rcDto.Description;
                            existingRc.FeeDate = rcDto.FeeDate;
                        }
                        else
                        {

                            existingLease.FeeCharge.Add(new PMS_PropertyHapa.Models.Entities.FeeCharge
                            {
                                Amount = rcDto.Amount,
                                Description = rcDto.Description,
                                LeaseId = existingLease.LeaseId,
                                FeeDate = rcDto.FeeDate,
                            });
                        }
                    }
                }



                if (leaseDto.SecurityDeposits != null)
                {
                    foreach (var sdDto in leaseDto.SecurityDeposits)
                    {
                        var existingSd = existingLease.SecurityDeposit.FirstOrDefault(sd => sd.SecurityDepositId == sdDto.SecurityDepositId);
                        if (existingSd != null)
                        {

                            existingSd.Amount = sdDto.Amount;
                            existingSd.Description = sdDto.Description;
                            existingLease.LeaseId = sdDto.LeaseId;
                        }
                        else
                        {

                            existingLease.SecurityDeposit.Add(new SecurityDeposit
                            {
                                Amount = sdDto.Amount,
                                Description = sdDto.Description,
                                LeaseId = existingLease.LeaseId
                            });
                        }
                    }
                }

                var oldInvoices = await _db.Invoices.Where(x => x.LeaseId == leaseDto.LeaseId).ToListAsync();

                    oldInvoices.ForEach(x => x.IsDeleted = true);
                    _db.UpdateRange(oldInvoices);

                decimal totalRentAmount = 0;

                if (leaseDto.RentCharges != null)
                {
                    totalRentAmount += leaseDto.RentCharges.Sum(rc => rc.Amount);
                }

                if (leaseDto.SecurityDeposits != null)
                {
                    totalRentAmount += leaseDto.SecurityDeposits.Sum(sd => sd.Amount);
                }

                if (leaseDto.FeeCharges != null)
                {
                    totalRentAmount += leaseDto.FeeCharges.Sum(fc => fc.Amount);
                }

                if (leaseDto.IsMonthToMonth)
                {
                    DateTime invoiceDate = leaseDto.StartDate;
                    while (invoiceDate <= leaseDto.EndDate)
                    {
                        var newInvoice = new Invoice
                        {
                            LeaseId = leaseDto.LeaseId,
                            //OwnerId = leaseDto.OwnerId,
                            OwnerName = "Test up",
                            InvoiceCreatedDate = DateTime.Now,
                            TenantId = leaseDto.TenantId,
                            TenantName = leaseDto.TenantIdValue,
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
                        //OwnerId = leaseDto.OwnerId,
                        OwnerName = "Test up",
                        InvoiceCreatedDate = DateTime.Now,
                        TenantId = leaseDto.TenantId,
                        TenantName = leaseDto.TenantIdValue,
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



        public async Task<bool> UpdateOwnerAsync(OwnerDto tenantDto)
        {
            var tenant = await _db.Owner.FirstOrDefaultAsync(t => t.OwnerId == tenantDto.OwnerId);
            if (tenant == null) return false;

            var ownerOrganization = await _db.OwnerOrganization.FirstOrDefaultAsync(o => o.OwnerId == tenantDto.OwnerId);


            tenant.FirstName = tenantDto.FirstName;
            tenant.MiddleName = tenantDto.MiddleName;
            tenant.LastName = tenantDto.LastName;
            tenant.EmailAddress = tenantDto.EmailAddress;
            tenant.EmailAddress2 = tenantDto.EmailAddress2;
            tenant.PhoneNumber = tenantDto.PhoneNumber;
            tenant.PhoneNumber2 = tenantDto.PhoneNumber2;
            tenant.Fax = tenantDto.Fax;
            tenant.TaxId = tenantDto.TaxId;
            tenant.Document = tenantDto.Document;
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
            tenant.Picture = tenantDto.Picture;

            if (ownerOrganization != null)
            {
                ownerOrganization.OrganizationName = tenantDto.OrganizationName;
                ownerOrganization.OrganizationDescription = tenantDto.OrganizationDescription;
                ownerOrganization.OrganizationIcon = tenantDto.OrganizationIcon;
                ownerOrganization.OrganizationLogo = tenantDto.OrganizationLogo;
                ownerOrganization.Website = tenantDto.Website;

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
                    Website = tenantDto.Website
                };
                _db.OwnerOrganization.Add(ownerOrganization);
            }

            _db.Owner.Update(tenant);

            var result = await _db.SaveChangesAsync();
            return result > 0;
        }


        //Inovices
        public async Task<List<Invoice>> GetInvoicesAsync(int leaseId)
        {
            var invoices = await _db.Invoices.Where(l => l.LeaseId == leaseId && l.IsDeleted != true).ToListAsync();

            return invoices;
        }  
        
        public async Task<Invoice> GetInvoiceByIdAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FirstOrDefaultAsync(l => l.InvoiceId == invoiceId && l.IsDeleted != true);

            return invoice;
        }

        public async Task<bool> AllInvoicePaidAsync(int leaseId)
        {
            var invoices = await _db.Invoices.Where(t => t.LeaseId == leaseId && t.InvoicePaid != true &&  t.IsDeleted != true).ToListAsync();

            invoices.ForEach(x => x.InvoicePaid = true);
            _db.Invoices.UpdateRange(invoices);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
        
        
        public async Task<bool> AllInvoiceOwnerPaidAsync(int leaseId)
        {
            var invoices = await _db.Invoices.Where(t => t.LeaseId == leaseId && t.InvoicePaidToOwner != true &&  t.IsDeleted != true).ToListAsync();

            invoices.ForEach(x => x.InvoicePaidToOwner = true);
            _db.Invoices.UpdateRange(invoices);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<bool> InvoicePaidAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FirstOrDefaultAsync(t => t.InvoiceId == invoiceId);

            invoice.InvoicePaid = true;
             _db.Invoices.Update(invoice);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
        
        
        public async Task<bool> InvoiceOwnerPaidAsync(int invoiceId)
        {
            var invoice = await _db.Invoices.FirstOrDefaultAsync(t => t.InvoiceId == invoiceId);

            invoice.InvoicePaidToOwner = true;
            _db.Invoices.Update(invoice);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }


        public async Task<bool> DeleteOwnerAsync(string ownerId)
        {
            var tenant = await _db.Owner.FirstOrDefaultAsync(t => t.OwnerId == Convert.ToInt32(ownerId));
            if (tenant == null) return false;

            var owner = await _db.OwnerOrganization.FirstOrDefaultAsync(t => t.OwnerId == Convert.ToInt32(ownerId));
            if (owner == null) return false;

            _db.OwnerOrganization.Remove(owner);
            _db.Owner.Remove(tenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
        #endregion



        #region TenantOrg
        public async Task<TenantOrganizationInfoDto> GetTenantOrgByIdAsync(int tenantId)
        {
            var tenant = await _db.TenantOrganizationInfo.FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return new TenantOrganizationInfoDto();

            var tenantDto = new TenantOrganizationInfoDto
            {
                TenantUserId = tenant.TenantUserId,
                OrganizationName = tenant.OrganizationName,
                OrganizationDescription = tenant.OrganizationDescription,
                OrganizationIcon = tenant.OrganizationIcon,
                OrganizationLogo = tenant.OrganizationLogo,
                OrganizatioPrimaryColor = tenant.OrganizatioPrimaryColor,
                OrganizationSecondColor = tenant.OrganizationSecondColor,
            };

            return tenantDto;
        }


        public async Task<bool> UpdateTenantOrgAsync(TenantOrganizationInfoDto tenantDto)
        {
            if (tenantDto.Id < 0) return false;

            var newTenant = _db.TenantOrganizationInfo.FirstOrDefault(x => x.TenantUserId == tenantDto.TenantUserId);
            if (newTenant == null)
                newTenant = new TenantOrganizationInfo();


            newTenant.Id = tenantDto.Id;
            newTenant.TenantUserId = tenantDto.TenantUserId;
            newTenant.OrganizationName = tenantDto.OrganizationName;
            newTenant.OrganizationDescription = tenantDto.OrganizationDescription;
            if (tenantDto.OrganizationIcon != null)
            {
                newTenant.OrganizationIcon = tenantDto.OrganizationIcon;
            }
            if (tenantDto.OrganizationLogo != null)
            {
                newTenant.OrganizationLogo = tenantDto.OrganizationLogo;
            }
            newTenant.OrganizatioPrimaryColor = tenantDto.OrganizatioPrimaryColor;
            newTenant.OrganizationSecondColor = tenantDto.OrganizationSecondColor;

            if (newTenant.Id > 0)
                _db.TenantOrganizationInfo.Update(newTenant);

            else
                _db.TenantOrganizationInfo.Add(newTenant);


            var result = await _db.SaveChangesAsync();
            return result > 0;
        }
        #endregion



        #region Assets
        public async Task<List<AssetDTO>> GetAllAssetsAsync()
        {
            try
            {
                var propertyTypes = await _db.Assets
                                             .Include(asset => asset.Units)
                                             .AsNoTracking()
                                             .ToListAsync();



                var propertyTypeDtos = propertyTypes.Select(tenant => new AssetDTO
                {
                    AssetId = tenant.AssetId,
                    SelectedPropertyType = tenant.SelectedPropertyType,
                    SelectedBankAccountOption = tenant.SelectedBankAccountOption,
                    SelectedReserveFundsOption = tenant.SelectedReserveFundsOption,
                    SelectedSubtype = tenant.SelectedSubtype,
                    SelectedOwnershipOption = tenant.SelectedOwnershipOption,
                    BuildingNo = tenant.BuildingNo,
                    BuildingName = tenant.BuildingName,
                    Street1 = tenant.Street1,
                    Street2 = tenant.Street2,
                    City = tenant.City,
                    Country = tenant.Country,
                    Zipcode = tenant.Zipcode,
                    State = tenant.State,
                    AppTid = tenant.AppTenantId,
                    Image = tenant.Image,
                    AddedBy = tenant.AddedBy,
                    Units = tenant.Units.Select(unit => new UnitDTO
                    {
                        UnitId = unit.UnitId,
                        UnitName = unit.UnitName,
                    }).ToList()

                }).ToList();


                return propertyTypeDtos;
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

        public async Task<bool> CreateAssetAsync(AssetDTO assetDTO)
        {
            var user = await _userManager.FindByIdAsync(assetDTO.AddedBy);
            if (user != null)
            {
                var subscription = await _db.Subscriptions.FirstOrDefaultAsync(x => x.Id == user.SubscriptionId);

                if (subscription != null && subscription.SubscriptionName == SubscriptionTypes.Free.ToString())
                {
                    var leaseCount = await _db.Assets
                        .Where(x => x.AddedBy == assetDTO.AddedBy)
                        .AsNoTracking()
                        .CountAsync();

                    if (leaseCount >= 5)
                    {
                        return false;
                    }
                }

            }

            var existingOwner = await _db.Owner.FirstOrDefaultAsync(o => o.EmailAddress == assetDTO.OwnerEmail);
            if (existingOwner == null)
            {

                existingOwner = new Owner
                {
                    FirstName = assetDTO.OwnerFirstName,
                    LastName = assetDTO.OwnerLastName,
                    EmailAddress = assetDTO.OwnerEmail,
                    Address = assetDTO.OwnerAddress,
                    District = assetDTO.OwnerDistrict,
                    Region = assetDTO.OwnerRegion,
                    CountryCode = assetDTO.OwnerCountryCode,
                    Country = assetDTO.OwnerCountry,
                    AppTenantId = Guid.Parse(assetDTO.AppTid),
                    Picture = assetDTO.OwnerImage

                };
                await _db.Owner.AddAsync(existingOwner);
            }

            var newAsset = new Assets
            {
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
                Image = assetDTO.Image,
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
                    Rent = u.Rent
                };
                newAsset.Units.Add(unit);
            }
            await _db.Assets.AddAsync(newAsset);
            var result = await _db.SaveChangesAsync();

            return result > 0;
        }


        public async Task<bool> UpdateAssetAsync(AssetDTO assetDTO)
        {
            var existingOwner = await _db.Owner.FirstOrDefaultAsync(o => o.EmailAddress == assetDTO.OwnerEmail);
            if (existingOwner == null)
            {

                existingOwner = new Owner
                {
                    FirstName = assetDTO.OwnerFirstName,
                    LastName = assetDTO.OwnerLastName,
                    EmailAddress = assetDTO.OwnerEmail,
                    Address = assetDTO.OwnerAddress,
                    District = assetDTO.OwnerDistrict,
                    Region = assetDTO.OwnerRegion,
                    CountryCode = assetDTO.OwnerCountryCode,
                    Country = assetDTO.OwnerCountry,
                    AppTenantId = Guid.Parse(assetDTO.AppTid),
                    Picture = assetDTO.OwnerImage
                };
                await _db.Owner.AddAsync(existingOwner);
            }


            var existingAsset = await _db.Assets.FindAsync(assetDTO.AssetId);
            if (existingAsset == null)
            {
                return false;
            }

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
            existingAsset.Image = assetDTO.Image;
            existingAsset.AppTenantId = assetDTO.AppTid;


            foreach (var unitDTO in assetDTO.Units)
            {
                var existingUnit = existingAsset.Units.FirstOrDefault(u => u.UnitId == unitDTO.UnitId);
                if (existingUnit != null)
                {
                    existingUnit.UnitName = unitDTO.UnitName;
                    existingUnit.Beds = unitDTO.Beds;
                    existingUnit.Bath = unitDTO.Bath;
                    existingUnit.Size = unitDTO.Size;
                    existingUnit.Rent = unitDTO.Rent;
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
                        Rent = unitDTO.Rent
                    };
                    existingAsset.Units.Add(newUnit);
                }
            }

            foreach (var existingUnit in existingAsset.Units.ToList())
            {
                if (!assetDTO.Units.Any(u => u.UnitId == existingUnit.UnitId))
                {
                    _db.AssetsUnits.Remove(existingUnit);
                }
            }

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }


        public async Task<bool> DeleteAssetAsync(int assetId)
        {
            var asset = await _db.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            _db.Assets.Remove(asset);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }


        #endregion



        #region Communication
        public async Task<List<CommunicationDto>> GetAllCommunicationAsync()
        {
            try
            {
                var communicationTypes = await _db.Communication
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
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
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
                                           Picture = owner.Picture,
                                           PhoneNumber = owner.PhoneNumber,
                                           PhoneNumber2 = owner.PhoneNumber2,
                                           EmergencyContactInfo = owner.EmergencyContactInfo,
                                           LeaseAgreementId = owner.LeaseAgreementId,
                                           OwnerNationality = owner.OwnerNationality,
                                           Gender = owner.Gender,
                                           Document = owner.Document,
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


        #endregion





        public async Task<List<SubscriptionDto>> GetAllSubscriptionsAsync()
        {
            return await _db.Subscriptions.Select(subscription => new SubscriptionDto
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



        #region Taks Request

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
                Console.WriteLine($"An error occurred while mapping owners and organizations: {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<TaskRequestDto>> GetMaintenanceTasksAsync()
        {
            try
            {
                var result = await (from t in _db.TaskRequest
                                    from a in _db.Assets.Where(x => x.AssetId == t.AssetId).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == t.TenantId).DefaultIfEmpty()
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
                                        TaskRequestFile = t.TaskRequestFile,
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
                                                         Account = item.Account,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping owners and organizations: {ex.Message}");
                throw;
            }
        }
        
        public async Task<List<TaskRequestDto>> GetTaskRequestsAsync()
        {
            try
            {
                var result = await (from t in _db.TaskRequest
                                    from a in _db.Assets.Where(x => x.AssetId == t.AssetId).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == t.TenantId).DefaultIfEmpty()
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
                                        TaskRequestFile = t.TaskRequestFile,
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
                                                         Account = item.Account,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping owners and organizations: {ex.Message}");
                throw;
            }
        }
        public async Task<TaskRequestDto> GetTaskByIdAsync(int id)
        {
            try
            {
                var result = await (from t in _db.TaskRequest
                                    from a in _db.Assets.Where(x => x.AssetId == t.AssetId).DefaultIfEmpty()
                                    from o in _db.Owner.Where(x => x.OwnerId == t.OwnerId).DefaultIfEmpty()
                                    from tnt in _db.Tenant.Where(x => x.TenantId == t.TenantId).DefaultIfEmpty()
                                    where t.TaskRequestId == id
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
                                        TaskRequestFile = t.TaskRequestFile,
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
                                                         Account = item.Account,
                                                         Memo = item.Memo
                                                     }).ToList()
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping owners and organizations: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SaveTaskAsync(TaskRequestDto taskRequestDto)
        {
            var taskRequest = _db.TaskRequest.FirstOrDefault(x => x.TaskRequestId == taskRequestDto.TaskRequestId);

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
            if (taskRequestDto.TaskRequestFile != null)
            {
                taskRequest.TaskRequestFile = taskRequestDto.TaskRequestFile;
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
                .Where(item => item.TaskRequestId == taskRequestDto.TaskRequestId && !lineItemIds.Contains(item.LineItemId))
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
                var existingLineItems = _db.LineItem.Where(item => item.TaskRequestId == taskRequestDto.TaskRequestId).ToList();
                foreach (var lineItemDto in taskRequestDto.LineItems)
                {
                    var existingLineItem = existingLineItems.FirstOrDefault(item => item.LineItemId == lineItemDto.LineItemId);

                    if (existingLineItem != null)
                    {
                        existingLineItem.Quantity = lineItemDto.Quantity;
                        existingLineItem.Price = lineItemDto.Price;
                        existingLineItem.Account = lineItemDto.Account;
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
                            Account = lineItemDto.Account,
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

            return result > 0;

        }
        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _db.TaskRequest.FindAsync(id);
            if (task == null) return false;

            task.IsDeleted = true;
            _db.TaskRequest.Update(task);
            var saveResult = await _db.SaveChangesAsync();

            var lineItems = await _db.LineItem.Where(x => x.TaskRequestId == id).ToListAsync();

            lineItems.ForEach(x => x.IsDeleted = true);
            _db.LineItem.UpdateRange(lineItems);
            var lineItemSaveResult = await _db.SaveChangesAsync();

            return saveResult > 0 || lineItemSaveResult > 0;
        }

        public async Task<bool> SaveTaskHistoryAsync(TaskRequestHistoryDto taskRequestHistoryDto)
        {
            var taskRequest = await _db.TaskRequest.FirstOrDefaultAsync(x => x.TaskRequestId == taskRequestHistoryDto.TaskRequestId);

            if (taskRequest != null)
            {
                taskRequest.Status = taskRequestHistoryDto.Status;
                taskRequest.ModifiedBy = taskRequestHistoryDto.AddedBy;
                taskRequest.ModifiedDate = DateTime.Now;
                _db.TaskRequest.Update(taskRequest);

                await _db.SaveChangesAsync();
            }

            if (taskRequest.Type == TaskTypes.WorkOrderRequest)
            {

            }

            var taskRequestHistory = await _db.TaskRequestHistory.FirstOrDefaultAsync(x => x.TaskRequestHistoryId == taskRequestHistoryDto.TaskRequestHistoryId);

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
            return result > 0;
        }



        #endregion

        #region LandloadGetData
        public async Task<object> GetLandlordDataById(int id)
        {
            var result = await (from owner in _db.Owner
                                where owner.OwnerId == id
                                join asset in _db.Assets on owner.OwnerId equals asset.AssetId into assets
                                from a in assets.DefaultIfEmpty()
                                join lease in _db.Lease on owner.OwnerId equals lease.TenantsTenantId into leases
                                from l in leases.DefaultIfEmpty()
                                select new
                                {
                                    Owner = owner,
                                    Asset = a,
                                    Lease = l,
                                    Tasks = _db.TaskRequest.Where(task => task.OwnerId == owner.OwnerId || task.AssetId == a.AssetId).ToList(),
                                    PropertyDetails = _db.PropertyType.FirstOrDefault(property => property.PropertyTypeId == a.AssetId)
                                }).FirstOrDefaultAsync();

            return result;
        }


        #endregion

        public async Task<object> GetTenantDataById(int id)
        {
            var result = await (from tenant in _db.Tenant
                                where tenant.TenantId == id
                                join lease in _db.Lease on tenant.TenantId equals lease.TenantsTenantId into leases
                                from l in leases.DefaultIfEmpty()
                                join asset in _db.Assets on l.AppTenantId equals asset.AppTenantId into assets
                                from a in assets.DefaultIfEmpty()
                                select new
                                {
                                    Tenant = tenant,
                                    Lease = l,
                                    Asset = a,
                                    Tasks = _db.TaskRequest.Where(task => task.TenantId == tenant.TenantId || task.AssetId == a.AssetId).ToList(),
                                    PropertyDetails = _db.PropertyType.FirstOrDefault(property => property.PropertyTypeId == a.AssetId)
                                }).FirstOrDefaultAsync();

            return result;
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
                        .SelectMany(lease => _db.Tenant.Where(t => t.TenantId == lease.TenantsTenantId  && t.AddedBy == filter.UserId && t.IsDeleted != true).DefaultIfEmpty(), (lease, tenant) => new
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

                if (filter.TenantFilter != null  && filter.TenantFilter.Any())
                {
                    var tenantIds = filter.TenantFilter.Select(id => int.Parse(id)).ToList();
                    query = query.Where(x => tenantIds.Contains(x.TenantsTenantId) && x.AddedBy == filter.UserId && x.IsDeleted !=true);
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
        public async Task<VendorCategory> GetVendorCategoryByIdAsync(int id)
        {
            try
            {
                var result = await (from v in _db.VendorCategory
                                    where v.VendorCategoryId == id
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
                Console.WriteLine($"An error occurred while mapping  Vendor Category: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SaveVendorCategoryAsync(VendorCategory model)
        {
            var vendorCategory = _db.VendorCategory.FirstOrDefault(x => x.VendorCategoryId == model.VendorCategoryId);

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


        #region Vendor 


        public async Task<List<VendorDto>> GetVendorsAsync()
        {
            try
            {
                var result = await (from v in _db.Vendor
                                    from vo in _db.VendorOrganization.Where(x=>x.VendorId == v.VendorId).DefaultIfEmpty()
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
                                        Picture = v.Picture,
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
                                        Classification = v.Classification,
                                        VendorCategoriesIds = v.VendorCategoriesIds,
                                        HasInsurance = v.HasInsurance,
                                        PropertyId = v.PropertyId,
                                        UnitIds = v.UnitIds,
                                        TaxId = v.TaxId,
                                        AccountName = v.AccountName,
                                        AccountHolder = v.AccountHolder,
                                        AccountIBAN = v.AccountIBAN,
                                        AccountSwift = v.AccountSwift,
                                        AccountBank = v.AccountBank,
                                        AccountCurrency = v.AccountCurrency,
                                        OrganizationName = vo.OrganizationName,
                                        OrganizationDescription = vo.OrganizationDescription,
                                        OrganizationIcon = vo.OrganizationIcon,
                                        OrganizationLogo = vo.OrganizationLogo,
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
        public async Task<VendorDto> GetVendorByIdAsync(int id)
        {
            try
            {
                var result = await (from v in _db.Vendor
                                    from vo in _db.VendorOrganization.Where(x => x.VendorId == v.VendorId).DefaultIfEmpty()
                                    where v.VendorId == id
                                    select new VendorDto
                                    {
                                        VendorId = v.VendorId,
                                        FirstName = v.FirstName,
                                        MI = v.MI,
                                        LastName = v.LastName,
                                        Company = v.Company,
                                        JobTitle = v.JobTitle,
                                        Notes = v.Notes,
                                        Picture = v.Picture,
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
                                        Classification = v.Classification,
                                        VendorCategoriesIds = v.VendorCategoriesIds,
                                        HasInsurance = v.HasInsurance,
                                        PropertyId = v.PropertyId,
                                        UnitIds = v.UnitIds,
                                        TaxId = v.TaxId,
                                        AccountName = v.AccountName,
                                        AccountHolder = v.AccountHolder,
                                        AccountIBAN = v.AccountIBAN,
                                        AccountSwift = v.AccountSwift,
                                        AccountBank = v.AccountBank,
                                        AccountCurrency = v.AccountCurrency,
                                        OrganizationName = vo.OrganizationName,
                                        OrganizationDescription = vo.OrganizationDescription,
                                        OrganizationIcon = vo.OrganizationIcon,
                                        OrganizationLogo = vo.OrganizationLogo,
                                        Website = vo.Website,
                                        AddedBy = v.AddedBy
                                    })
                     .AsNoTracking()
                     .FirstOrDefaultAsync();


                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping  Vendor : {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SaveVendorAsync(VendorDto model)
        {
            var vendor = _db.Vendor.FirstOrDefault(x => x.VendorId == model.VendorId);

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
                vendor.Picture = model.Picture;
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
            vendor.Classification = model.Classification;
            vendor.VendorCategoriesIds = model.VendorCategoriesIds;
            vendor.HasInsurance = model.HasInsurance;
            vendor.PropertyId = model.PropertyId;
            vendor.UnitIds = model.UnitIds;
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

            var vendorOrganization = _db.VendorOrganization.FirstOrDefault(x => x.VendorId == vendor.VendorId);

            if (vendorOrganization == null)
            {
                vendorOrganization = new VendorOrganization();
                
                vendorOrganization.VendorId = vendor.VendorId;
                vendorOrganization.OrganizationName = model.OrganizationName;
                vendorOrganization.OrganizationDescription = model.OrganizationDescription;
                if (model.OrganizationIcon != null)
                {
                    vendorOrganization.OrganizationIcon = model.OrganizationIcon;
                }
                if (model.OrganizationLogo != null)
                {
                    vendorOrganization.OrganizationLogo = model.OrganizationLogo;
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
                    vendorOrganization.OrganizationIcon = model.OrganizationIcon;
                }
                if (model.OrganizationLogo != null)
                {
                    vendorOrganization.OrganizationLogo = model.OrganizationLogo;
                }
                vendorOrganization.Website = model.Website;
                _db.VendorOrganization.Update(vendorOrganization);
            }

            var result2 = await _db.SaveChangesAsync();

            return result1 > 0 && result2 > 0;

        }
        public async Task<bool> DeleteVendorAsync(int id)
        {
            var vendor = await _db.Vendor.FindAsync(id);
            if (vendor == null) return false;

            vendor.IsDeleted = true;
            _db.Vendor.Update(vendor);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
        }


        #endregion



        #region Applications 


        public async Task<List<ApplicationsDto>> GetApplicationsAsync()
        {
            try
            {
                var result = await (from a in _db.Applications
                                    from p in _db.Assets.Where(x=>x.AssetId == a.PropertyId).DefaultIfEmpty()
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
                                        StubPicture = a.StubPicture,
                                        LicensePicture = a.LicensePicture,
                                        IsAgree = a.IsAgree,
                                        Pets = _db.ApplicationPets.Where(x=>x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                        Vehicles = _db.ApplicationVehicles.Where(x=>x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                        Dependent = _db.ApplicationDependent.Where(x=>x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
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
                                    where a.ApplicationId == id
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
                                        StubPicture = a.StubPicture,
                                        LicensePicture = a.LicensePicture,
                                        IsAgree = a.IsAgree,
                                        Pets = _db.ApplicationPets.Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
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
            var application = _db.Applications.FirstOrDefault(x => x.ApplicationId == model.ApplicationId);

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
            if (model.LicensePicture != null)
            {
                application.LicensePicture = model.LicensePicture;
            }
            if (model.StubPicture != null)
            {
                application.StubPicture = model.StubPicture;
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
                    var existingPet = existingPets.FirstOrDefault(item => item.PetId == petDto.PetId);

                    if (existingPet != null)
                    {
                       existingPet.ApplicationId = application.ApplicationId;
                       existingPet.Name = petDto.Name;
                       existingPet.Breed = petDto.Breed;
                       existingPet.Type = petDto.Type;
                       existingPet.Quantity = petDto.Quantity;
                       existingPet.Picture = petDto.Picture != null ? petDto.Picture : existingPet.Picture;
                       existingPet.ModifiedBy = model.AddedBy;
                       existingPet.ModifiedDate = DateTime.Now;
                        _db.ApplicationPets.Update(existingPet);
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
                        _db.ApplicationPets.Add(newPet);
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
                    var existingVehicle = existingVehicles.FirstOrDefault(item => item.VehicleId == vehicleDto.VehicleId);

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
                    var existingDependent = existingDependents.FirstOrDefault(item => item.DependentId == dependentDto.DependentId);

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
        public async Task<AccountType> GetAccountTypeByIdAsync(int id)
        {
            try
            {
                var result = await (from at in _db.AccountType
                                    where at.AccountTypeId == id
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
                Console.WriteLine($"An error occurred while mapping  Account Type: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SaveAccountTypeAsync(AccountType model)
        {
            var accountType = _db.AccountType.FirstOrDefault(x => x.AccountTypeId == model.AccountTypeId);

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
        public async Task<bool> DeleteAccountTypeAsync(int id)
        {
            var accountType = await _db.AccountType.FindAsync(id);
            if (accountType == null) return false;

            accountType.IsDeleted = true;
            _db.AccountType.Update(accountType);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
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
        public async Task<AccountSubType> GetAccountSubTypeByIdAsync(int id)
        {
            try
            {
                var result = await (from ast in _db.AccountSubType
                                    where ast.AccountSubTypeId == id
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
                Console.WriteLine($"An error occurred while mapping  Account Sub Type: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SaveAccountSubTypeAsync(AccountSubType model)
        {
            var accountSubType = _db.AccountSubType.FirstOrDefault(x => x.AccountSubTypeId == model.AccountSubTypeId);

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
        public async Task<bool> DeleteAccountSubTypeAsync(int id)
        {
            var accountSubType = await _db.AccountSubType.FindAsync(id);
            if (accountSubType == null) return false;

            accountSubType.IsDeleted = true;
            _db.AccountSubType.Update(accountSubType);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
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
                Console.WriteLine($"An error occurred while mapping Account Sub Types: {ex.Message}");
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
                                    where ca.ChartAccountId == id
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
                Console.WriteLine($"An error occurred while mapping  Account Sub Type: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SaveChartAccountAsync(ChartAccount model)
        {
            var chartAccount = _db.ChartAccount.FirstOrDefault(x => x.ChartAccountId == model.ChartAccountId);

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
        public async Task<bool> DeleteChartAccountAsync(int id)
        {
            var chartAccount = await _db.ChartAccount.FindAsync(id);
            if (chartAccount == null) return false;

            chartAccount.IsDeleted = true;
            _db.ChartAccount.Update(chartAccount);
            var saveResult = await _db.SaveChangesAsync();

            return saveResult > 0;
        }

        #endregion

        #region Buget

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
                var result = await _db.Budgets.Include(x=>x.Items).Where(x=>x.BudgetId == id && x.IsDeleted != true).FirstOrDefaultAsync();

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
            foreach(var x in budgetItems) 
            {
                if(x.Total != "Total" && x.AccountName != "AccountName")
                {
                    BudgetItem item = new BudgetItem()
                    {
                        AccountName = x.AccountName,
                        Total = decimal.Parse(x.Total.Replace(".", ",")),
                        Period = x.Period,
                        BudgetItemMonth = new BudgetItemMonth()
                        {
                            Jan = x.BudgetItemMonth.Jan != null ? decimal.Parse(x.BudgetItemMonth.Jan.Replace(".",",")) : 0,
                            Feb = x.BudgetItemMonth.Feb != null ? decimal.Parse(x.BudgetItemMonth.Feb.Replace(".",",")) : 0,
                            March = x.BudgetItemMonth.March != null ? decimal.Parse(x.BudgetItemMonth.March.Replace(".",",")) : 0,
                            April = x.BudgetItemMonth.April != null ? decimal.Parse(x.BudgetItemMonth.April.Replace(".",",")) : 0,
                            May = x.BudgetItemMonth.May != null ? decimal.Parse(x.BudgetItemMonth.May.Replace(".",",")) : 0,
                            June = x.BudgetItemMonth.June != null ? decimal.Parse(x.BudgetItemMonth.June.Replace(".",",")) : 0,
                            July = x.BudgetItemMonth.July != null ? decimal.Parse(x.BudgetItemMonth.July.Replace(".",",")) : 0,
                            Aug = x.BudgetItemMonth.Aug != null ? decimal.Parse(x.BudgetItemMonth.Aug.Replace(".",",")) : 0,
                            Sep = x.BudgetItemMonth.Sep != null ? decimal.Parse(x.BudgetItemMonth.Sep.Replace(".",",")) : 0,
                            Oct = x.BudgetItemMonth.Oct != null ? decimal.Parse(x.BudgetItemMonth.Oct.Replace(".",",")) : 0,
                            Nov = x.BudgetItemMonth.Nov != null ? decimal.Parse(x.BudgetItemMonth.Nov.Replace(".",",")) : 0,
                            Dec = x.BudgetItemMonth.Dec != null ? decimal.Parse(x.BudgetItemMonth.Dec.Replace(".",",")) : 0,
                            quat1 = x.BudgetItemMonth.quat1 != null ? decimal.Parse(x.BudgetItemMonth.quat1.Replace(".",",")) : 0,
                            quat2 = x.BudgetItemMonth.quat2 != null ? decimal.Parse(x.BudgetItemMonth.quat2.Replace(".",",")) : 0,
                            quat4 = x.BudgetItemMonth.quat4 != null ? decimal.Parse(x.BudgetItemMonth.quat4.Replace(".",",")) : 0,
                            quat5 = x.BudgetItemMonth.quat5 != null ? decimal.Parse(x.BudgetItemMonth.quat5.Replace(".",",")) : 0,
                            YearStart = x.BudgetItemMonth.YearStart != null ? decimal.Parse(x.BudgetItemMonth.YearStart.Replace(".",",")) : 0,
                            YearEnd = x.BudgetItemMonth.YearEnd != null ? decimal.Parse(x.BudgetItemMonth.YearEnd.Replace(".",",")) : 0,
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
    }
}
