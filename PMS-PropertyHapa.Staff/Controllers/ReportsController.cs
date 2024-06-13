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
        public IActionResult TaskReports()
        {
            return View();
        }
        public IActionResult LeaseReports()
        {
            return View();
        }
        public IActionResult InvoiceReports()
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



    }
}
