using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    
    public class ReportsController : Controller
    {
        private readonly IAuthService _authService;
        public ReportsController(IAuthService authService)
        {
            _authService = authService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult TaskReports()
        {
            return View();
        }
        public IActionResult LeaseReports()
        {
            return View();
        }
        public IActionResult AssetReports()
        {
            return View();
        }
        public IActionResult LandlordReports()
        {
            return View();
        }
        public IActionResult TenantReports()
        {
            return View();
        }
        public IActionResult FinanceReports()
        {
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
