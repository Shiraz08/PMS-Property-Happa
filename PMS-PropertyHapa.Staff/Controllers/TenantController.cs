using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.ImageUpload;
using PMS_PropertyHapa.Staff.Models;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.Security.Claims;
using PMS_PropertyHapa.Shared.Email;
using Humanizer.Localisation;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using System.Drawing;
using System.IO;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class TenantController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        EmailSender _emailSender = new EmailSender();

        public TenantController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }


        public async Task<IActionResult> Index()
        {
            var owner = await _authService.GetAllTenantsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                owner = owner.Where(s => s.AddedBy == currenUserId);
            }
            return View(owner);
        }

        [HttpGet]
        public async Task<IActionResult> GetTenantDataById(int id)
        {
            TenantDataDto tenant = await _authService.GetTenantDataById(id);
            return Ok(tenant);
        }

        public async Task<IActionResult> GetAllTenants()
        {

            var tenants = await _authService.GetAllTenantsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                tenants = tenants.Where(s => s.AddedBy == currenUserId);
            }
            if (tenants != null)
            {
                return Ok(tenants);
            }

            else
            {

                return Json(new { data = new List<TenantModelDto>() });
            }
        }


        public async Task<IActionResult> GetTenant(string tenantId)
        {
            if (!string.IsNullOrEmpty(tenantId))
            {
                var tenants = await _authService.GetTenantsByIdAsync(tenantId);

                if (tenants != null && tenants.Count > 0)
                {

                    return Ok(tenants);
                }
                else
                {

                    return Json(new { data = new List<TenantModelDto>() });
                }
            }
            else
            {

                return BadRequest("Tenant ID is required.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create(TenantModelDto tenant)
        {
            if (!Guid.TryParse(tenant.AppTid, out Guid appTenantId))
            {
                return Json(new { success = false, message = "Invalid AppTid format" });
            }

            tenant.AppTenantId = appTenantId;

            if (tenant.PictureUrl != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    tenant.PictureName = tenant.PictureUrl.FileName;
                    await tenant.PictureUrl.CopyToAsync(memoryStream);
                    var pictureBytes = memoryStream.ToArray();
                    tenant.Picture = Convert.ToBase64String(pictureBytes);
                }
                tenant.PictureUrl = null;
            }
            
            if (tenant.DocumentUrl != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    tenant.DocumentName = tenant.DocumentUrl.FileName;
                    await tenant.DocumentUrl.CopyToAsync(memoryStream);
                    var pictureBytes = memoryStream.ToArray();
                    tenant.Document = Convert.ToBase64String(pictureBytes);
                }
                tenant.DocumentUrl = null;
            }

            if (tenant.Pets != null)
            {
                foreach (var pet in tenant.Pets)
                {
                    if (pet.PictureUrl2 != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            pet.PictureName = pet.PictureUrl2.FileName;
                            await pet.PictureUrl2.CopyToAsync(memoryStream);
                            var pictureBytes = memoryStream.ToArray();
                            pet.Picture = Convert.ToBase64String(pictureBytes);
                        }
                        pet.PictureUrl2 = null;
                    }
                }
            }

            tenant.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.CreateTenantAsync(tenant);


            // Register tenant as a user if required
            if (tenant.AddTenantAsUser)
            {
                var newtenant = await _authService.GetAllTenantsAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    newtenant = newtenant.Where(s => s.AddedBy == currenUserId);
                }
                var registrationRequest = new RegisterationRequestDTO
                {
                    UserName = tenant.EmailAddress,
                    Name = $"{tenant.FirstName} {tenant.LastName}",
                    Email = tenant.EmailAddress,
                    Password = "Test@123",
                    Role = "Tenant",
                    TenantId = newtenant.Max(s => s.TenantId)
                };

                APIResponse result = await _authService.RegisterAsync<APIResponse>(registrationRequest);
                if (!result.IsSuccess)
                {
                    return Json(new { success = false, message = "Failed to register tenant as user." });
                }

                var emailContent = $"Welcome {tenant.FirstName} {tenant.LastName},\n\nThank you for registering. Here are your details:\nUsername: {tenant.EmailAddress}\nPassword: Test@123\nTenant ID: {registrationRequest.TenantId}\n\nThank you!";
                await _emailSender.SendEmailAsync(tenant.EmailAddress, "Welcome to Our Service!", emailContent);
            }

            return Json(new { success = true, message = "Tenant added successfully" });
        }



        [HttpPost]
        public async Task<IActionResult> CreateTenantData(TenantModelDto tenant)
        {
            if (!Guid.TryParse(tenant.AppTid, out Guid appTenantId))
            {
                return Json(new { success = false, message = "Invalid AppTid format" });
            }

            tenant.AppTenantId = appTenantId;

            if (tenant.PictureUrl != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    tenant.PictureName = tenant.PictureUrl.FileName;
                    await tenant.PictureUrl.CopyToAsync(memoryStream);
                    var pictureBytes = memoryStream.ToArray();
                    tenant.Picture = Convert.ToBase64String(pictureBytes);
                }
                tenant.PictureUrl = null;
            }

            if (tenant.DocumentUrl != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    tenant.DocumentName = tenant.DocumentUrl.FileName;
                    await tenant.DocumentUrl.CopyToAsync(memoryStream);
                    var pictureBytes = memoryStream.ToArray();
                    tenant.Document = Convert.ToBase64String(pictureBytes);
                }
                tenant.DocumentUrl = null;
            }

            if (tenant.Pets != null)
            {
                foreach (var pet in tenant.Pets)
                {
                    if (pet.PictureUrl2 != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            pet.PictureName = pet.PictureUrl2.FileName;
                            await pet.PictureUrl2.CopyToAsync(memoryStream);
                            var pictureBytes = memoryStream.ToArray();
                            pet.Picture = Convert.ToBase64String(pictureBytes);
                        }
                        pet.PictureUrl2 = null;
                    }
                }
            }

            bool response2 = await _authService.CreateTenantAsync(tenant);
            if (response2)
            {
                var tenantdatafetch = await _authService.GetAllTenantsAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    tenantdatafetch = tenantdatafetch.Where(s => s.AddedBy == currenUserId);
                }
                var maxTenantId = tenantdatafetch.Max(s => s.TenantId);
                tenant.TenantId = maxTenantId;

                return Json(new { success = true, message = "Tenant added successfully", tenant = tenant });
            }
            else
            {
                return Json(new { success = false, message = "Failed to add tenant" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(TenantModelDto tenant)
        {
            tenant.AppTenantId = Guid.Parse(tenant.AppTid);

            if (tenant.PictureUrl != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    tenant.PictureName = tenant.PictureUrl.FileName;
                    await tenant.PictureUrl.CopyToAsync(memoryStream);
                    var pictureBytes = memoryStream.ToArray();
                    tenant.Picture = Convert.ToBase64String(pictureBytes);
                }
                tenant.PictureUrl = null;
            }

            if (tenant.DocumentUrl != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    tenant.DocumentName = tenant.DocumentUrl.FileName;
                    await tenant.DocumentUrl.CopyToAsync(memoryStream);
                    var pictureBytes = memoryStream.ToArray();
                    tenant.Document = Convert.ToBase64String(pictureBytes);
                }
                tenant.DocumentUrl = null;
            }

            if (tenant.Pets != null)
            {
                foreach (var pet in tenant.Pets)
                {
                    if (pet.PictureUrl2 != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            pet.PictureName = pet.PictureUrl2.FileName;
                            await pet.PictureUrl2.CopyToAsync(memoryStream);
                            var pictureBytes = memoryStream.ToArray();
                            pet.Picture = Convert.ToBase64String(pictureBytes);
                        }
                        pet.PictureUrl2 = null;
                    }
                }
            }

            await _authService.UpdateTenantAsync(tenant);
            return Json(new { success = true, message = "Tenant updated successfully" });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateTenantData([FromForm] TenantModelDto tenant)
        {
            try
            {
                tenant.AppTenantId = Guid.Parse(tenant.AppTid);

                if (tenant.PictureUrl != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await tenant.PictureUrl.CopyToAsync(memoryStream);
                        tenant.Picture = $"data:image/png;base64,{Convert.ToBase64String(memoryStream.ToArray())}";
                    }
                }

                if (tenant.Pets != null)
                {
                    foreach (var pet in tenant.Pets)
                    {
                        if (pet.Picture != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await pet.PictureUrl2.CopyToAsync(memoryStream);
                                pet.Picture = $"data:image/png;base64,{Convert.ToBase64String(memoryStream.ToArray())}";
                            }
                        }
                    }
                }

                await _authService.UpdateTenantAsync(tenant);
                return Json(new { success = true, message = "Tenant updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating tenant: " + ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> Delete(string tenantId)
        {
            await _authService.DeleteTenantAsync(tenantId);
            return Json(new { success = true, message = "Tenant deleted successfully" });
        }
        public IActionResult AddTenant()
        {
            var model = new TenantModelDto();

            return View(model);
        }

        public async Task<IActionResult> EditTenant(int tenantId)
        {
            TenantModelDto tenant;

            if (tenantId > 0)
            {
                tenant = await _authService.GetSingleTenantAsync(tenantId);

                if (tenant == null)
                {
                    return NotFound();
                }
            }
            else
            {
                tenant = new TenantModelDto();
            }

            return View("AddTenant", tenant);
        }



    }
}
