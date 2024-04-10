using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Staff.Services.IServices;
using NuGet.ContentModel;
using PMS_PropertyHapa.Shared.ImageUpload;
using System.Linq;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class CommunicationController : Controller
    {

        private readonly IAuthService _authService;


        public CommunicationController(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var communication = await _authService.GetAllCommunicationAsync();
            return View(communication);
        }

        public IActionResult AddCommunication()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetProperties()
        {
            try
            {
                var properties = await _authService.GetAllAssetsAsync();
                return Ok(properties);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching Communications: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTenants()
        {
            try
            {
                var tenants = await _authService.GetAllTenantsAsync();
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching Communications: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCommunication(CommunicationDto CommunicationDto)
        {
            try
            {
                if (CommunicationDto.CommunicationFile != null && CommunicationDto.CommunicationFile.Length > 0)
                {
                    var (fileName, base64String) = await ImageUploadUtility.UploadImageAsync(CommunicationDto.CommunicationFile, "uploads");
                    CommunicationDto.Communication_File = $"data:image/png;base64,{base64String}";
                }

                CommunicationDto.CommunicationFile = null;

                await _authService.CreateCommunicationAsync(CommunicationDto);
                return Ok(new { success = true, message = "Communication added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred while adding the Communication. {ex.Message}" });
            }
        }



        [HttpPost]
        public async Task<IActionResult> UpdateCommunication(int Communication_Id, CommunicationDto CommunicationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                CommunicationDto.Communication_Id = Communication_Id;
                await _authService.UpdateCommunicationAsync(CommunicationDto);
                return Ok(new { success = true, message = "Communication added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding the Communication." });
            }
        }



        [HttpDelete]
        public async Task<IActionResult> DeleteCommunication(int Communication_Id)
        {
            await _authService.DeleteCommunicationAsync(Communication_Id);
            return Json(new { success = true, message = "Tenant deleted successfully" });
        }


        [HttpGet]
        public async Task<IActionResult> GetCommunications()
        {
            try
            {
                var Communication = await _authService.GetAllCommunicationAsync();
                return Ok(Communication);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching Communications: {ex.Message}");
            }
        }


        public async Task<IActionResult> EditCommunication(int Communication_Id)
        {
            CommunicationDto Communication;
            IEnumerable<TenantModelDto> tenant;
            IEnumerable<AssetDTO> assets;

            if (Communication_Id > 0)
            {
                var Communications = await _authService.GetAllCommunicationAsync();
                Communication = Communications.FirstOrDefault(s => s.Communication_Id == Communication_Id);

                if (Communication == null)
                {
                    return NotFound();
                }

                tenant = await _authService.GetAllTenantsAsync();
                var tenantIds = Communication.TenantIds.Split(',').Select(int.Parse).ToList();
                var filteredTenants = tenant.Where(t => tenantIds.Contains((int)t.TenantId)).ToList();


                assets = await _authService.GetAllAssetsAsync();
                var assetIds = Communication.PropertyIds.Split(',').Select(int.Parse).ToList();
                var filteredAssets = assets.Where(t => assetIds.Contains(t.AssetId)).ToList();

            }
            else
            {
                Communication = new CommunicationDto();
            }

            return View("AddCommunication");
        }




    }
}
