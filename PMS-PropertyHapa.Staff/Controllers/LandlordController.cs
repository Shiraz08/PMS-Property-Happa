using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Staff.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Enum;
using PMS_PropertyHapa.Shared.ImageUpload;
using PMS_PropertyHapa.Staff.Auth.Controllers;
using PMS_PropertyHapa.Staff.Services.IServices;
using Newtonsoft.Json;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class LandlordController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;
        public LandlordController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
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
        public async Task<IActionResult> Index()
        {
            var owner = await _authService.GetAllLandlordAsync();
            return View(owner);
        }
        public IActionResult AddLandlord()
        {
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> GetLandlord()
        {
            try
            {
                var owner = await _authService.GetAllLandlordAsync();
                return Ok(owner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching assets: {ex.Message}");
            }
        }



        [HttpPost]
        public async Task<IActionResult> Create(OwnerDto owner)
        {
            if (Guid.TryParse(owner.AppTid, out Guid appTenantId))
            {
                owner.AppTenantId = appTenantId;
            }
            else
            {
                // Handle the case where AppTid is not a valid Guid string
                return Json(new { success = false, message = "Invalid AppTid format" });
            }

            if (owner.Picture != null || owner.OrganizationLogo != null || owner.OrganizationIcon != null)
            {
                owner.Picture = $"data:image/png;base64,{owner.Picture}";
                owner.OrganizationLogo = $"data:image/png;base64,{owner.OrganizationLogo}";
                owner.OrganizationIcon = $"data:image/png;base64,{owner.OrganizationIcon}";
            }
            await _authService.CreateLandlordAsync(owner);
            return Json(new { success = true, message = "Owner added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Update(OwnerDto owner)
        {
            owner.AppTenantId = Guid.Parse(owner.AppTid);

            if (owner.PictureUrl != null && owner.PictureUrl.Length > 0)
            {
                var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(owner.PictureUrl, "uploads");
                owner.Picture = $"data:image/png;base64,{base64String}";
            }

            if (owner.Picture != null || owner.OrganizationLogo != null || owner.OrganizationIcon != null)
            {
                owner.Picture = $"data:image/png;base64,{owner.Picture}";
                owner.OrganizationLogo = $"data:image/png;base64,{owner.OrganizationLogo}";
                owner.OrganizationIcon = $"data:image/png;base64,{owner.OrganizationIcon}";
            }
                await _authService.UpdateLandlordAsync(owner);
            return Json(new { success = true, message = "Owner updated successfully" });
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(string ownerId)
        {
            await _authService.DeleteLandlordAsync(ownerId);
            return Json(new { success = true, message = "Owner deleted successfully" });
        }



        public async Task<IActionResult> EditLandlord(int ownerId)
        {
            OwnerDto owner = null;

            if (ownerId > 0)
            {
                owner = await _authService.GetSingleLandlordAsync(ownerId);

                if (owner == null)
                {
                    return NotFound();
                }

            }

            return View("AddLandlord", owner);
        }



    }
}
