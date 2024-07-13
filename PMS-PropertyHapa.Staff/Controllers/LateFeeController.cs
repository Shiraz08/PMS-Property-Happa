using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class LateFeeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public LateFeeController(IAuthService authService, RoleManager<IdentityRole> roleManager)
        {
            _authService = authService;
            _roleManager = roleManager;
        }
        public IActionResult Settings()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddLateFeeAsset(int assetId)
        {
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
