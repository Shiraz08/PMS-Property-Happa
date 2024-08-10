using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    
    public class ReportsController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;
        public ReportsController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Index()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewReports);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public IActionResult TaskReports()
        {
            return View();
        }
        public async Task<IActionResult> LeaseReports()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewLeaseReports);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> AssetReports()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewAssetReports);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> LandlordReports()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewLandlordReports);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> TenantReports()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewTenantReports);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> FinanceReports()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewFinanceReports);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public IActionResult SmartReports()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetLeaseReports([FromBody] ReportFilter reportFilter)
        {

            var res = await _authService.GetLeaseReports(reportFilter);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetInvoiceReports([FromBody] ReportFilter reportFilter)
        {
            var res = await _authService.GetInvoiceReports(reportFilter);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetTaskRequestReports([FromBody] ReportFilter reportFilter)
        {
            var res = await _authService.GetTaskRequestReports(reportFilter);
            return Ok(res);
        }
        
        [HttpPost]
        public async Task<IActionResult> GetFinanceReports([FromBody] ReportFilter reportFilter)
        {
            var res = await _authService.GetFinanceReports(reportFilter);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetUnitsByAsset([FromBody] ReportFilter reportFilter)
        {
            reportFilter.AddedBy = Request?.Cookies["userId"]?.ToString();
            var res = await _authService.GetUnitsByAsset(reportFilter);
            return Ok(res);
        }
    }
}
