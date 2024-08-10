using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IPermissionService _permissionService;
        public ServicesController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }
        public async Task<IActionResult> PaymentIntegration()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.PaymentIntegration);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> QuickBookIntegration()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.QuickBookIntegration);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
    }
}
