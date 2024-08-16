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
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.Shared.Email;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class CommunicationController : Controller
    {

        private readonly IAuthService _authService;
        private readonly EmailSender _emailSenderBase;
        private readonly IPermissionService _permissionService;

        public CommunicationController(IAuthService authService, EmailSender emailSenderBase, IPermissionService permissionService)
        {
            _authService = authService;
            _emailSenderBase = emailSenderBase;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewCommunication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
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

        public async Task<IActionResult> AddCommunication()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddCommunication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
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
                var currenUserId = Request?.Cookies["userId"]?.ToString();

                var filter = new Filter();
                filter.AddedBy = currenUserId;
                var properties = await _authService.GetAssetsDllAsync(filter);
                //if (currenUserId != null)
                //{
                //    properties = properties.Where(s => s.AddedBy == currenUserId);
                //}
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
                //var count = await _authService.GetAllCommunicationAsync();
                var tenant = await _authService.GetAllTenantsAsync();
                //var tenants = tenant.Where(s => s.AppTid == count?.FirstOrDefault().UserID);
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    tenant = tenant.Where(s => s.AddedBy == currenUserId);
                }
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching Communications: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTenantsDll()
        {
            try
            {
                
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                var filter = new Filter();
                filter.AddedBy = currenUserId;
                var tenant = await _authService.GetAllTenantsDllAsync(filter);
                //if (currenUserId != null)
                //{
                //    tenant = tenant.Where(s => s.AddedBy == currenUserId);
                //}
                return Ok(tenant);
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
                var currenUserId = Request?.Cookies["userId"]?.ToString();

                bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddCommunication);
                if (!hasAccess)
                {
                    return Unauthorized();
                }
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

                        var tenants = await _authService.GetAllTenantsAsync();
                        tenants = tenants.Where(x => x.AddedBy == currenUserId);
                        if (tenantIds.Count > 0)
                        {
                            foreach (var tenantId in tenantIds)
                            {

                                var emailAddress = tenants.Where(s => s.TenantId == Convert.ToInt32(tenantId)).FirstOrDefault().EmailAddress;
                                List<string> emailAddressList = new List<string> { emailAddress };

                                var fileBytes = Convert.FromBase64String(CommunicationDto.Communication_File.Split(',')[1]);

                                using (var memoryStream = new MemoryStream(fileBytes))
                                {
                                    // Send email with file to the tenant
                                    //await _emailSenderBase.SendEmailWithFilebyStream(
                                    //    memoryStream,
                                    //    emailAddressList,
                                    //    CommunicationDto.Subject,
                                    //    CommunicationDto.Message,
                                    //    "AttachmentFileName.pdf" // Provide the file name here
                                    //);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in tenants)
                            {

                                var emailAddress = item.EmailAddress;
                                List<string> emailAddressList = new List<string> { emailAddress };

                                var fileBytes = Convert.FromBase64String(CommunicationDto.Communication_File.Split(',')[1]);

                                using (var memoryStream = new MemoryStream(fileBytes))
                                {
                                    // Send email with file to the tenant
                                    //await _emailSenderBase.SendEmailWithFilebyStream(
                                    //    memoryStream,
                                    //    emailAddressList,
                                    //    CommunicationDto.Subject,
                                    //    CommunicationDto.Message,
                                    //    "AttachmentFileName.pdf" // Provide the file name here
                                    //);
                                }
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
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddCommunication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

                        var tenants = await _authService.GetAllTenantsAsync();
                        tenants = tenants.Where(x => x.AddedBy == currenUserId);
                        if (tenantIds.Count > 0)
                        {
                            foreach (var tenantId in tenantIds)
                            {

                                var emailAddress = tenants.Where(s => s.TenantId == Convert.ToInt32(tenantId)).FirstOrDefault().EmailAddress;
                                List<string> emailAddressList = new List<string> { emailAddress };

                                var fileBytes = Convert.FromBase64String(CommunicationDto.Communication_File.Split(',')[1]);

                                using (var memoryStream = new MemoryStream(fileBytes))
                                {
                                    // Send email with file to the tenant
                                    await _emailSenderBase.SendEmailWithFilebyStream(
                                        memoryStream,
                                        emailAddressList,
                                        CommunicationDto.Subject,
                                        CommunicationDto.Message,
                                        "AttachmentFileName.pdf" // Provide the file name here
                                    );
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in tenants)
                            {

                                var emailAddress = item.EmailAddress;
                                List<string> emailAddressList = new List<string> { emailAddress };

                                var fileBytes = Convert.FromBase64String(CommunicationDto.Communication_File.Split(',')[1]);

                                using (var memoryStream = new MemoryStream(fileBytes))
                                {
                                    // Send email with file to the tenant
                                    await _emailSenderBase.SendEmailWithFilebyStream(
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
                }
                CommunicationDto.Communication_Id = Communication_Id;
                await _authService.UpdateCommunicationAsync(CommunicationDto);
                return Ok(new { success = true, message = "Communication added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding the Communication." });
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteCommunication(int Communication_Id)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddCommunication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            await _authService.DeleteCommunicationAsync(Communication_Id);
            return Json(new { success = true, message = "Announcement deleted successfully" });
        }

        public async Task<IActionResult> EditCommunication(int communicationId)
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.AddCommunication);
            if (!hasAccess)
            {
                return Unauthorized();
            }
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
                var tenantIds = Communication.TenantIds != null ? Communication.TenantIds.Split(',').Select(int.Parse).ToList() : new List<int>();
                var filteredTenants = tenant.Where(t => tenantIds.Contains((int)t.TenantId)).ToList();


                assets = await _authService.GetAllAssetsAsync();
                var assetIds = Communication.PropertyIds != null ? Communication.PropertyIds.Split(',').Select(int.Parse).ToList() : new List<int>();
                var filteredAssets = assets.Where(t => assetIds.Contains(t.AssetId)).ToList();

            }
            else
            {
                Communication = new CommunicationDto();
            }

            return View("AddCommunication", Communication);
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
