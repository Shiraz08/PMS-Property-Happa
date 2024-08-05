using MagicVilla_VillaAPI.Repository.IRepostiory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Web;
using System.Security.Claims;
using System.Net.Http.Headers;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.API.Services;
using PMS_PropertyHapa.API.ViewModels;
using Microsoft.Extensions.Options;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    [Route("api/v1/UsersAuth")]
    [ApiController]
    //  [ApiVersionNeutral]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        protected APIResponse _response;
        private readonly GoogleCloudStorageService _googleCloudStorageService;
        private readonly GoogleCloudStorageOptions _googleCloudStorageOptions;

        public UsersController(IUserRepository userRepo, UserManager<ApplicationUser> userManager, GoogleCloudStorageService googleCloudStorageService, IOptions<GoogleCloudStorageOptions> googleCloudStorageOptions)
        {
            _userRepo = userRepo;
            _response = new();
            _userManager = userManager;
            _googleCloudStorageService = googleCloudStorageService;
            _googleCloudStorageOptions = googleCloudStorageOptions.Value;
        }

        [HttpGet("Error")]
        public async Task<IActionResult> Error()
        {
            throw new FileNotFoundException();
        }

        [HttpGet("ImageError")]
        public async Task<IActionResult> ImageError()
        {
            throw new BadImageFormatException("Fake Image Exception");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var tokenDto = await _userRepo.Login(model);
            if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Email or password is incorrect"); 
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = tokenDto;
            return Ok(_response);
        }

   

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            bool ifUserNameUnique = _userRepo.IsUniqueUser(model.UserName);
            if (!ifUserNameUnique)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            var user = await _userRepo.Register(model);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await _userRepo.RefreshAccessToken(tokenDTO);
                if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Token Invalid");
                    return BadRequest(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = tokenDTOResponse;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.Result = "Invalid Input";
                return BadRequest(_response);
            }

        }


        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
        {

            if (ModelState.IsValid)
            {
                await _userRepo.RevokeRefreshToken(tokenDTO);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            _response.IsSuccess = false;
            _response.Result = "Invalid Input";
            return BadRequest(_response);
        }



        #region Registeration 

        [HttpPost("register/tenant")]
        public async Task<IActionResult> RegisterTenant([FromBody] RegisterationRequestDTO model)
        {
            var user = await _userRepo.RegisterTenant(model);

            if (user == null || user.userID == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering tenant");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = user;
            return Ok(_response);
        }




        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterationRequestDTO model)
        {
            try
            {


                var user = await _userRepo.RegisterAdmin(model);
                if (user == null || user.userID == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Error while registering admin");
                    return BadRequest(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = user;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost("register/user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterationRequestDTO model)
        {

            var user = await _userRepo.RegisterUser(model);
            if (user == null || user.userID == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering user");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = user;
            return Ok(_response);
        }



        #endregion

        #region Update Profile API

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var profileModel = await _userRepo.GetProfileModelAsync(userId);

            if (profileModel == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User not found");
                return NotFound(_response); 
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = profileModel;
            return Ok(_response);
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepo.GetAllUsersAsync();

                if (users == null || !users.Any())
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Error while fetching users" };
                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = users.ToList();
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Update basic information
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.Address2 = model.Address2;
            user.Locality = model.Locality;
            user.District = model.District;
            user.Region = model.Region;
            user.PostalCode = model.PostalCode;
            user.Country = model.Country;
            user.CountryCode = model.CountryCode;
            user.Status = model.Status;
            user.TermsAndConditons = model.TermsAndConditons;
            user.Currency = model.Currency;

            if (model.NewPicture != null)
            {
                user.Picture = model.NewPicture.FileName;
            }

            var result = await _userManager.UpdateAsync(user);


            if (model.NewPicture != null)
            {
                var ext = Path.GetExtension(model.NewPicture.FileName);
                var res = await _googleCloudStorageService.UploadImageAsync(model.NewPicture, "AspNetUsers_Picture_" + user.Id + ext);
            }


            if (result.Succeeded)
            {
                return Ok(new { pictureUrl = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"AspNetUsers_Picture_" + user.Id + Path.GetExtension(user.Picture)}" });
            }

            return BadRequest(result.Errors);
        }


        [HttpGet("Roles/{userId}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _userRepo.GetUserRolesAsync(userId);

            if (roles == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("User not found");
                return NotFound(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = roles;
            return Ok(_response);
        }
        
        [HttpGet("IsUserTrial/{userId}")]
        public async Task<IActionResult> IsUserTrial(string userId)
        {
            try
            {
                var trial = await _userRepo.IsUserTrialAsync(userId);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = trial;
                    return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }

        #endregion



        #region ChangePassword, ForgetPassword, Reset Password

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto model)
        {
            if (!await _userRepo.ValidateCurrentPassword(model.UserId.ToString(), model.currentPassword))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Current password is incorrect");
                return BadRequest(_response);
            }

            if (model.newPassword != model.newRepeatPassword)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("New password and confirmation password do not match");
                return BadRequest(_response);
            }
            if (!await _userRepo.ChangePassword(model.UserId.ToString(), model.currentPassword, model.newPassword))
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while changing password");
                return StatusCode(500, _response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Password changed successfully";
            return Ok(_response);
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

          Task<string> email;
            try
            {
               email = _userRepo.DecryptEmail(model.Email);
            }
            catch
            {
                return BadRequest("Invalid reset link.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("The password and confirmation password do not match.");
            }

            // Update the model with the decrypted email
            model.Email = email.Result.ToString();

            var result = await _userRepo.ResetPasswordAsync(model);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Password has been reset successfully";
            return Ok(_response);
        }




        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgetPassword model)
        {
            var user = await _userRepo.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound; 
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("No user found with this email.");
                return NotFound(_response);  
            }

            // Encrypt the email
            var encryptedEmail = _userRepo.EncryptEmail(user.Email);

            var baseUrl = $"https://localhost:7182";
            var resetPasswordUrl = $"{baseUrl}/auth/ResetPassword?email={encryptedEmail.Result}";
            await _userRepo.SendResetPasswordEmailAsync(user, resetPasswordUrl);

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = "Reset password email sent successfully";
            return Ok(_response);
        }

        #endregion



        #region TenantCrud 

        [HttpGet("Tenant")]
        public async Task<ActionResult<IEnumerable<TenantModelDto>>> GetAllTenants()
        {
            try
            {
                var tenants = await _userRepo.GetAllTenantsAsync();
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("Tenant/{tenantId}")]
        public async Task<IActionResult> GetTenantById(string tenantId)
        {
            try
            {
                var tenantDto = await _userRepo.GetTenantsByIdAsync(tenantId);

                if (tenantDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenantDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }


        [HttpGet("GetSingleTenant/{tenantId}")]
        public async Task<IActionResult> GetSingleTenant(int tenantId)
        {
            try
            {
                var tenantDto = await _userRepo.GetSingleTenantByIdAsync(tenantId);

                if (tenantDto != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = tenantDto;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("No user found with this id.");
                    return NotFound(_response);
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error Occured");
                return NotFound(_response);
            }
        }


        [HttpPost("Tenant")]
        public async Task<ActionResult<bool>> CreateTenant(TenantModelDto tenant)
        {
            try
            {
                var isSuccess = await _userRepo.CreateTenantAsync(tenant);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("Tenant/{tenantId}")]
        public async Task<ActionResult<bool>> UpdateTenant(int tenantId, TenantModelDto tenant)
        {
            try
            {
                tenant.TenantId = tenantId; // Ensure tenantId is set
                var isSuccess = await _userRepo.UpdateTenantAsync(tenant);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("Tenant/{tenantId}")]
        public async Task<ActionResult<APIResponse>> DeleteTenant(int tenantId)
        {
            try
            {
                var isSuccess = await _userRepo.DeleteTenantAsync(tenantId);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                return NotFound(_response);
            }
        }
        #endregion
    }
}