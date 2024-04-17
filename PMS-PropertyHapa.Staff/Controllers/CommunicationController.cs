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
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.EmailSenderFile;
using Microsoft.EntityFrameworkCore;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class CommunicationController : Controller
    {

        private readonly IAuthService _authService;
        private readonly EmailSenderBase _emailSenderBase;


        public CommunicationController(IAuthService authService, EmailSenderBase emailSenderBase)
        {
            _authService = authService;
            _emailSenderBase = emailSenderBase;
        }

        public async Task<IActionResult> Index()
        {
            var communications = await _authService.GetAllCommunicationAsync();
            var propertiesCount = (await _authService.GetAllAssetsAsync()).Count();
            var tenantsCount = (await _authService.GetAllTenantsAsync()).Count();

            var model = new CommunicationsViewModel
            {
                Communications = communications,
                TotalPropertiesCount = propertiesCount,
                TotalTenantsCount = tenantsCount
            };

            return View(model);
        }

        public IActionResult AddCommunication()
        {
            return View();
        }


        public IActionResult TiwiloView()
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
                var count = await _authService.GetAllCommunicationAsync();
                var tenant = await _authService.GetAllTenantsAsync();
                var tenants = tenant.Where(s => s.AppTid == count?.FirstOrDefault().UserID);
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

                if (CommunicationDto.IsByEmail)
                {
                    if (CommunicationDto.Subject != null && CommunicationDto.Message != null && CommunicationDto.Communication_File != null)
                    {
                        List<string> tenantIds = CommunicationDto.TenantIds.Split(',').ToList();

                        foreach (var tenantId in tenantIds)
                        {
                            var tenant = await _authService.GetAllTenantsAsync();
                            var emailAddress = tenant.Where(s=>s.TenantId == Convert.ToInt32(tenantId)).FirstOrDefault().EmailAddress;
                            List<string> emailAddressList = new List<string> { emailAddress };

                            var fileBytes = Convert.FromBase64String(CommunicationDto.Communication_File.Split(',')[1]);

                            using (var memoryStream = new MemoryStream(fileBytes))
                            {
                                // Send email with file to the tenant
                                await _emailSenderBase.SendEmailWithFile(
                                    memoryStream,
                                    emailAddressList,
                                    CommunicationDto.Subject,
                                    CommunicationDto.Message,
                                    "AttachmentFileName.pdf" // Provide the file name here
                                );
                            }
                        }
                    }
                }
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
            return Json(new { success = true, message = "Announcement deleted successfully" });
        }





        public async Task<IActionResult> EditCommunication(int communicationId)
        {
            CommunicationDto Communication;
            IEnumerable<TenantModelDto> tenant;
            IEnumerable<AssetDTO> assets;

            if (communicationId > 0)
            {
                var Communications = await _authService.GetAllCommunicationAsync();
                Communication = Communications.FirstOrDefault(s => s.Communication_Id == communicationId);

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



        [HttpPost]
        public async Task<IActionResult> AddTiwilo(TiwiloDto model)
        {
            if (!String.IsNullOrEmpty(model.UserID))
            {
                bool updateAccount = await _authService.UpdateAccountAsync(model);

                if (updateAccount)
                {
                    return Json(new { success = true, message = "Data updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update data" });
                }
            }
            else
            {

                return Json(new { success = false, message = "Invalid user ID" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetUserTwilioCredentials(string userId)
        {
            var users = await _authService.GetAllUsersAsync();
            var getusers = users.Where(users => users.AppTId == userId).ToList();
           
            return Ok(new { AccountSid = getusers.FirstOrDefault().AccountSid, AuthToken = getusers.FirstOrDefault().AuthToken, TiwiloPhone = getusers.FirstOrDefault().TiwiloPhone });
        }


    }
}
