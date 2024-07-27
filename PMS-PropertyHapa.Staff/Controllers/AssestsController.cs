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

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class AssestsController : Controller
    {

        private readonly IAuthService _authService;

        public AssestsController(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<IActionResult> Index()
        {
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

                assetDTO.AddedBy = Request?.Cookies["userId"]?.ToString();
                await _authService.CreateAssetAsync(assetDTO);
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
                assetDTO.AddedBy = Request?.Cookies["userId"]?.ToString();
                await _authService.UpdateAssetAsync(assetDTO);
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

        public  IActionResult AssetDetails(int id)
        {
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
