using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Services.IServices;

namespace PMS_PropertyHapa.Controllers
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



        public async Task<IActionResult> GetTenant(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                var tenant = await _authService.GetTenantByIdAsync(tenantId.Value);
                return View(tenant);
            }
            return View(new TenantModelDto()); // Return an empty tenant for adding
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TenantModelDto tenant)
        {
            await _authService.CreateTenantAsync(tenant);
            return Json(new { success = true, message = "Tenant added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] TenantModelDto tenant)
        {
            await _authService.UpdateTenantAsync(tenant);
            return Json(new { success = true, message = "Tenant updated successfully" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int tenantId)
        {
            await _authService.DeleteTenantAsync(tenantId);
            return Json(new { success = true, message = "Tenant deleted successfully" });
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddTenant()
        {
            return View();
        }
    }
}
