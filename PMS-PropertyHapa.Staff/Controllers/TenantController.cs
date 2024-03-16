using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.ImageUpload;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.Security.Claims;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class TenantController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public TenantController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }


        public async Task<IActionResult> Index()
        {
            var owner = await _authService.GetAllTenantsAsync();
            return View(owner);
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

            if (tenant.Picture != null)
            {
                tenant.Picture = $"data:image/png;base64,{tenant.Picture}";
            }

            if (tenant.Pets != null)
            {
                foreach (var pet in tenant.Pets)
                {
                    if (pet.PictureUrl2 != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await pet.PictureUrl2.CopyToAsync(memoryStream);
                            var pictureBytes = memoryStream.ToArray();
                            pet.Picture = $"data:image/png;base64,{Convert.ToBase64String(pictureBytes)}";
                        }
                        pet.PictureUrl2 = null; 
                    }
                }
            }



            await _authService.CreateTenantAsync(tenant);
            return Json(new { success = true, message = "Tenant added successfully" });
        }


        [HttpPost]
        public async Task<IActionResult> Update(TenantModelDto tenant)
        {
            tenant.AppTenantId = Guid.Parse(tenant.AppTid);

            if (tenant.Picture != null)
            {
                tenant.Picture = $"data:image/png;base64,{tenant.Picture}";
            }

            if (tenant.Pets != null)
            {
                foreach (var pet in tenant.Pets)
                {
                    if (pet.PictureUrl2 != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await pet.PictureUrl2.CopyToAsync(memoryStream);
                            var pictureBytes = memoryStream.ToArray();
                            pet.Picture = $"data:image/png;base64,{Convert.ToBase64String(pictureBytes)}";
                        }
                        // As mentioned earlier, setting IFormFile to null after conversion might not be necessary
                         pet.PictureUrl2 = null;
                    }
                }
            }

            await _authService.UpdateTenantAsync(tenant);
            return Json(new { success = true, message = "Tenant updated successfully" });
        }



        [HttpDelete]
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
