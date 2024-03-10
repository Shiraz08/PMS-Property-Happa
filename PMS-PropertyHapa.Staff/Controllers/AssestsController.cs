using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Staff.Services.IServices;
using NuGet.ContentModel;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class AssestsController : Controller
    {

        private readonly IAuthService _authService;

        public AssestsController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost]
        public async Task<IActionResult> AddAsset(AssetDTO assetDTO)
        {

            try
            {
                await _authService.CreateAssetAsync(assetDTO);
                return Ok(new { success = true, message = "Asset added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred while adding the asset. {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateAsset(int assetId, AssetDTO assetDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                assetDTO.AssetId = assetId;
                await _authService.UpdateAssetAsync(assetDTO);
                return Ok(new { success = true, message = "Asset added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding the asset." });
            }
        }



        [HttpDelete]
        public async Task<IActionResult> DeleteAsset(int assetId)
        {
            await _authService.DeleteAssetAsync(assetId);
            return Json(new { success = true, message = "Tenant deleted successfully" });
        }


        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            try
            {
                var asset = await _authService.GetAllAssetsAsync();
                return Ok(asset);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching assets: {ex.Message}");
            }
        }


        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AddAssest()
        {
            //Changes by Raas
            var landlords = await _authService.GetAllLandlordAsync();
            var viewModel = new AssetAndOwnersViewModel
            {
                Owners = landlords
            };

            return View(viewModel);
        }


        public async Task<IActionResult> EditAsset(int assetId)
        {
            AssetDTO asset;
            IEnumerable<OwnerDto> owners;

            if (assetId > 0)
            {
                var assets = await _authService.GetAllAssetsAsync();
                asset = assets.FirstOrDefault(s => s.AssetId == assetId);

                if (asset == null)
                {
                    return NotFound();
                }

                owners = await _authService.GetAllLandlordAsync();
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


    }
}
