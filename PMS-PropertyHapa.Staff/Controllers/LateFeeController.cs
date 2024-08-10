using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class LateFeeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPermissionService _permissionService;

        public LateFeeController(IAuthService authService, RoleManager<IdentityRole> roleManager, IPermissionService permissionService)
        {
            _authService = authService;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Settings()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewSettings);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddLateFee);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> AddLateFeeAsset(int assetId)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAssets);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            ViewBag.AssetId = assetId;
            return View();
        }

        public async Task<IActionResult> GetLateFee()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            var filter = new Filter();
            filter.AddedBy = currenUserId;

            var lateFee = await _authService.GetLateFee(filter);

            return Ok(lateFee);
        }
        public async Task<IActionResult> GetLateFeeByAsset(int assetId)
        {
            var lateFeeAsset = await _authService.GetLateFeeByAsset(assetId);

            return Ok(lateFeeAsset);
        }

        public async Task<IActionResult> SaveLateFee(LateFeeDto lateFee)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddLateFee);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (lateFee == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            lateFee.AddedBy = Request?.Cookies["userId"]?.ToString();

            await _authService.SaveLateFeeAsync(lateFee);
            return Json(new { success = true, message = "LateFee added successfully" });
            //return Ok();
        }

        public async Task<IActionResult> SaveLateFeeAsset(LateFeeAssetDto lateFeeAsset)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAssets);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (lateFeeAsset == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            lateFeeAsset.AddedBy = Request?.Cookies["userId"]?.ToString();

            await _authService.SaveLateFeeAssetAsync(lateFeeAsset);
            return Json(new { success = true, message = "LateFee added successfully" });
        }



    }
}
