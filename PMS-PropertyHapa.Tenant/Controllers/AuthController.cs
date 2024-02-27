using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Tenant.Controllers;
using PMS_PropertyHapa.Tenant.Services.IServices;
using PMS_PropertyHapa.Shared.Enum;
using PMS_PropertyHapa.Shared.ImageUpload;
using Microsoft.AspNetCore.Authorization;

namespace PMS_PropertyHapa.Tenant.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
        }
        [HttpGet]
        public IActionResult Login()
        {
            // Check if there's an error message passed via ViewBag and pass it to the view if needed
            ViewBag.ErrorMessage = ViewBag.ErrorMessage ?? string.Empty;
            LoginRequestDTO obj = new();
            return View(obj);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO login)
        {
            try
            {
                ApplicationUser appUser = _context.Users.FirstOrDefault(x => x.Email == login.Email);
                if (appUser != null)
                {
                    try
                    {
                        await _signInManager.SignOutAsync();
                        var result = await _signInManager.PasswordSignInAsync(appUser, login.Password, login.Remember, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            var isTenant = await _userManager.IsInRoleAsync(appUser, "Tenant");
                            if (isTenant)
                            {
                                return Json(new { success = true, message = "Login successful.", redirectTo = Url.Action("Index", "Home") });
                            }
                            else
                            {
                                return Json(new { success = false, message = "Login Failed: Only Tenants are allowed to log in." });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Login Failed: Invalid email or password." });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "An error occurred while attempting to sign in." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Login Failed: Invalid email or password." });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "An unexpected error occurred during login." });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetProfile(string userId)
        {

            try
            {
                var profileModel = await _authService.GetProfileAsync(userId);

                if (profileModel != null)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Profile fetched successfully.",
                        result = profileModel
                    });


                }
                else
                {
                    return NotFound(new { message = "User not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }




        // Action to update profile information
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (model.NewPicture != null)
            {
                var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(model.NewPicture, "uploads");
                model.Picture = fileName;
                model.NewPictureBase64 = base64String;
            }

            model.NewPicture = null;
            var success = await _authService.UpdateProfileAsync(model);

            if (success)
            {

                return RedirectToAction("ProfileUpdated");
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the profile.");
            return View(model);
        }






        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordRequestDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Assuming the service returns a result object that indicates success or failure
            var result = await _authService.ChangePasswordAsync<APIResponse>(changePasswordDto);

            if (result.IsSuccess)
            {
                return Ok(new { message = "Password successfully changed." });
            }
            else
            {
                return BadRequest(new { errors = result.ErrorMessages });
            }
        }


        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                  new SelectListItem{Text=SD.Admin,Value=SD.Admin},
                new SelectListItem{Text=SD.User,Value=SD.User},
            };
            ViewBag.RoleList = roleList;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationRequestDTO obj)
        {
            if (string.IsNullOrEmpty(obj.Role))
            {
                obj.Role = SD.User;
            }
            APIResponse result = await _authService.RegisterAsync<APIResponse>(obj);
            if (result != null && result.IsSuccess)
            {
                return RedirectToAction("Login");
            }
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.Admin,Value=SD.Admin},
                new SelectListItem{Text=SD.Customer,Value=SD.Customer},
            };
            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }



        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordDto
            {
                Email = email
            };
            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _authService.ResetPasswordAsync<APIResponse>(model);

            if (response.IsSuccess)
            {
                return Ok(new { message = "Password reset successfully." });
            }
            else
            {
                return BadRequest(new { message = "Failed to reset password.", errors = response.ErrorMessages });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching users: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult UpdateProfile()
        {
            var model = new ProfileModel();

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    statusCode = 400,
                    isSuccess = false,
                    errorMessages = new List<string> { "Validation failed. Please check the input fields." },
                    result = (string)null
                });
            }

            var response = await _authService.ForgotPasswordAsync(model);

            if (response != null && response.IsSuccess)
            {

                return Json(new
                {
                    statusCode = 200,
                    isSuccess = true,
                    errorMessages = new List<string>(),
                    result = "Reset password email sent successfully"
                });
            }
            else
            {

                string errorMessage = response?.ErrorMessages?.FirstOrDefault() ?? "An unexpected error occurred. Please try again.";
                return Json(new
                {
                    statusCode = 400,
                    isSuccess = false,
                    errorMessages = new List<string> { errorMessage },
                    result = (string)null
                });
            }
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            var token = _tokenProvider.GetToken();
            await _authService.LogoutAsync<APIResponse>(token);
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }




        #region TenantDataFetching
      

        public async Task<IActionResult> GetTenantOrganizationInfo(int tenantId)
        {
            if (tenantId > 0)
            {
                var tenants = await _authService.GetTenantOrganizationByIdAsync(tenantId);

                if (tenants != null)
                {

                    return Json(new { data = tenants });
                }
                else
                {

                    return Json(new { data = new TenantOrganizationInfoDto() });
                }
            }
            else
            {
                return BadRequest("Tenant ID is required.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveTenantOrganizationInfo([FromBody] TenantOrganizationInfoDto tenantOrganizationInfoDto)
        {
            tenantOrganizationInfoDto.TenantUserId = Guid.Parse(tenantOrganizationInfoDto.TempTenantUserId);
            await _authService.UpdateTenantOrganizationAsync(tenantOrganizationInfoDto);
            return Json(new { success = true, message = "Color Schema updated successfully" });
        }

        #endregion



        [HttpGet]
        public async Task<IActionResult> GetAllFilterUsers(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return BadRequest("Search text is required.");
            }

            var filteredUsers = await _context.Users
                .Where(u => u.UserName.Contains(searchText) || u.Email.Contains(searchText))
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.AddedDate
                })
                .ToListAsync();

            return Ok(filteredUsers);
        }
    }
}
