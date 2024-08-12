using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Staff.Services.IServices;
using NuGet.ContentModel;
using PMS_PropertyHapa.Shared.ImageUpload;
using PMS_PropertyHapa.Models.Entities;
using Twilio.Http;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class AssestsController : Controller
    {

        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;

        public AssestsController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewAssets);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var assets = await _authService.GetAllAssetsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                assets = assets.Where(s => s.AddedBy == currenUserId);
            }
            return View(assets);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsset([FromForm] AssetDTO assetDTO)
        {
            try
            {
                //if (assetDTO.PictureFile != null)
                //{
                //    using (var memoryStream = new MemoryStream())
                //    {
                //        assetDTO.PictureFileName = assetDTO.PictureFile.FileName;
                //        await assetDTO.PictureFile.CopyToAsync(memoryStream);
                //        var pictureBytes = memoryStream.ToArray();
                //        assetDTO.Image = Convert.ToBase64String(pictureBytes);
                //    }
                //    assetDTO.PictureFile = null;
                //}
                var userId = Request?.Cookies["userId"]?.ToString();

                bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAssets);
                if (!hasAccess)
                {
                    return Unauthorized();
                }
                assetDTO.AddedBy = Request?.Cookies["userId"]?.ToString();
                var response =  await _authService.CreateAssetAsync(assetDTO);
                if (!response.IsSuccess)
                {
                    return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

                }
                return Ok(new { success = true, message = "Asset added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred while adding the asset. {ex.Message}" });
            }
        }



        [HttpPost]
        public async Task<IActionResult> UpdateAsset([FromForm] AssetDTO assetDTO)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            try
            {
                var userId = Request?.Cookies["userId"]?.ToString();

                bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAssets);
                if (!hasAccess)
                {
                    return Unauthorized();
                }
                assetDTO.AddedBy = Request?.Cookies["userId"]?.ToString();
                var response = await _authService.UpdateAssetAsync(assetDTO);
                if (!response.IsSuccess)
                {
                    return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

                }
                return Ok(new { success = true, message = "Asset added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding the asset." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteAsset(int assetId)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAssets);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var response = await _authService.DeleteAssetAsync(assetId);
            if (!response.IsSuccess)
            {
                return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

            }
            return Ok(new { success = true, message = "Asset deleted successfully" });

        }


        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            try
            {
                var asset = await _authService.GetAllAssetsAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    asset = asset.Where(s => s.AddedBy == currenUserId);
                }


                return Ok(asset);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching assets: {ex.Message}");
            }
        }



        public async Task<IActionResult> AddAssest()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAssets);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            //Changes by Raas
            var owners = await _authService.GetAllLandlordAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                owners = owners.Where(s => s.AddedBy == currenUserId);
            }
            var viewModel = new AssetAndOwnersViewModel
            {
                Owners = owners
            };

            return View(viewModel);
        }


        public async Task<IActionResult> EditAsset(int assetId)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddAssets);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var asset = new AssetDTO();
            IEnumerable<OwnerDto> owners;
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            if (assetId > 0)
            {
                 asset = await _authService.GetAssetByIdAsync(assetId);

                if (asset == null)
                {
                    return NotFound();
                }

                owners = await _authService.GetAllLandlordAsync();
                if (currenUserId != null)
                {
                    owners = owners.Where(s => s.AddedBy == currenUserId);
                }
            }
            else
            {
                asset = new AssetDTO();
                owners = new List<OwnerDto>();
            }

            var viewModel = new AssetAndOwnersViewModel
            {
                Asset = asset,
                Owners = owners.ToList()
            };

            return View("AddAssest", viewModel); 
        }

        public async Task<IActionResult> GetUnitsDetail(int assetId)
        {

            IEnumerable<UnitDTO> unitDetails = new List<UnitDTO>();
            unitDetails = await _authService.GetUnitsDetailAsync(assetId);
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            return Ok(unitDetails);
        }

        public async  Task<IActionResult> AssetDetails(int id)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewAssets);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            ViewBag.assetId = id;
            return View();
        }
        
        public async Task<IActionResult> GetAssetById(int assetId)
        {
            var asset = await _authService.GetAssetByIdAsync(assetId);
            return Ok(asset);
        }
        
    }
}
