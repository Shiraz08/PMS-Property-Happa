using AutoMapper;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.ContentModel;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static PMS_PropertyHapa.Models.DTO.TenantModelDto;

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
                TenantId = registerationRequestDTO?.TenantId ?? 0
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
                Document = tenantDto.Document
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
                Picture = tenantDto.Picture
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
                    SelectedUnit = leaseDto.SelectedUnit,
                    IsMonthToMonth = leaseDto.IsMonthToMonth,
                    HasSecurityDeposit = leaseDto.HasSecurityDeposit,
                    LateFeesPolicy = leaseDto.LateFeesPolicy,
                    TenantsTenantId = leaseDto.TenantId,
                    AppTenantId = Guid.Parse(leaseDto.AppTenantId)
                };

                await _db.Lease.AddAsync(newLease);
                await _db.SaveChangesAsync();

                if (leaseDto.RentCharges != null && leaseDto.SecurityDeposits != null)
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
                RentCharges = lease.RentCharges.Select(rc => new RentChargeDto
                {

                    RentChargeId = rc.RentChargeId,
                    Amount = rc.Amount,
                    Description = rc.Description,
                    RentDate = rc.RentDate,
                    RentPeriod = rc.RentPeriod
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
                     .Include(l => l.Tenants)
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
                    RentCharges = lease.RentCharges.Select(rc => new RentChargeDto
                    {

                        RentChargeId = rc.RentChargeId,
                        Amount = rc.Amount,
                        Description = rc.Description,
                        RentDate = rc.RentDate,
                        RentPeriod = rc.RentPeriod
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
                existingLease.SelectedUnit = leaseDto.SelectedUnit;
                existingLease.SignatureImagePath = leaseDto.SignatureImagePath;
                existingLease.IsFixedTerm = leaseDto.IsFixedTerm;
                existingLease.IsMonthToMonth = leaseDto.IsMonthToMonth;
                existingLease.HasSecurityDeposit = leaseDto.HasSecurityDeposit;
                existingLease.LateFeesPolicy = leaseDto.LateFeesPolicy;
                existingLease.TenantsTenantId = leaseDto.TenantId;
                existingLease.AppTenantId = Guid.Parse(leaseDto.AppTenantId);



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

            var newTenant = new TenantOrganizationInfo
            {
                Id = tenantDto.Id,
                TenantUserId = tenantDto.TenantUserId,
                OrganizationName = tenantDto.OrganizationName,
                OrganizationDescription = tenantDto.OrganizationDescription,
                OrganizationIcon = tenantDto.OrganizationIcon,
                OrganizationLogo = tenantDto.OrganizationLogo,
                OrganizatioPrimaryColor = tenantDto.OrganizatioPrimaryColor,
                OrganizationSecondColor = tenantDto.OrganizationSecondColor,

            };

            _db.TenantOrganizationInfo.Update(newTenant);
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
                    Street1 = tenant.Street1,
                    Street2 = tenant.Street2,
                    City = tenant.City,
                    Country = tenant.Country,
                    Zipcode = tenant.Zipcode,
                    State = tenant.State,
                    //     OwnerName = tenant.OwnerName,
                    //     OwnerEmail = tenant.OwnerEmail,
                    //     OwnerCompanyName = tenant.OwnerCompanyName,
                    //    OwnerAddress = tenant.OwnerAddress
                    //     OwnerDistrict = tenant.OwnerDistrict,
                    //     OwnerRegion = tenant.OwnerRegion,
                    //     OwnerCountryCode = tenant.OwnerCountryCode,
                    //     OwnerCountry = tenant.OwnerCountry,
                }).ToList();


                return propertyTypeDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while mapping property types: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateAssetAsync(AssetDTO assetDTO)
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

            var newAsset = new Assets
            {
                SelectedPropertyType = assetDTO.SelectedPropertyType,
                SelectedBankAccountOption = assetDTO.SelectedBankAccountOption,
                SelectedReserveFundsOption = assetDTO.SelectedReserveFundsOption,
                SelectedSubtype = assetDTO.SelectedSubtype,
                SelectedOwnershipOption = assetDTO.SelectedOwnershipOption,
                Street1 = assetDTO.Street1,
                Street2 = assetDTO.Street2,
                City = assetDTO.City,
                Country = assetDTO.Country,
                Zipcode = assetDTO.Zipcode,
                State = assetDTO.State,
                Image = assetDTO.Image,
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
            existingAsset.Street1 = assetDTO.Street1;
            existingAsset.Street2 = assetDTO.Street2;
            existingAsset.City = assetDTO.City;
            existingAsset.Country = assetDTO.Country;
            existingAsset.Zipcode = assetDTO.Zipcode;
            existingAsset.State = assetDTO.State;
            existingAsset.Image = assetDTO.Image;


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
                Name = subscription.Name,
                Price = subscription.Price,
                Description = subscription.Description,
                AppTenantId = subscription.AppTenantId,
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
                Name = subscription.Name,
                Price = subscription.Price,
                Description = subscription.Description,
                AppTenantId = subscription.AppTenantId,
                TenantId = subscription.TenantId
            };
        }

        public async Task<bool> CreateSubscriptionAsync(SubscriptionDto subscriptionDto)
        {
            var subscription = new Subscription
            {
                Name = subscriptionDto.Name,
                Price = subscriptionDto.Price,
                Description = subscriptionDto.Description,
                AppTenantId = subscriptionDto.AppTenantId,
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

            subscription.Name = subscriptionDto.Name;
            subscription.Price = subscriptionDto.Price;
            subscription.Description = subscriptionDto.Description;
            subscription.AppTenantId = subscriptionDto.AppTenantId;
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


    }






}
