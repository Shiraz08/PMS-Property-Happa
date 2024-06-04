using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.CodeDom;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class ChartAccountsController : Controller
    {
        private readonly IAuthService _authService;

        public ChartAccountsController(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<ChartAccountDto> chartAccounts = new List<ChartAccountDto>();
            chartAccounts = await _authService.GetChartAccountsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                chartAccounts = chartAccounts.Where(s => s.AddedBy == currenUserId);
            }
            return View(chartAccounts);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChartAccount([FromBody] ChartAccount chartAccount)
        {
            if (chartAccount == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            chartAccount.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveChartAccountAsync(chartAccount);
            return Json(new { success = true, message = "Sub Account added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChartAccount(int id)
        {
            await _authService.DeleteChartAccountAsync(id);
            return Json(new { success = true, message = "Sub Account deleted successfully" });
        }

        public async Task<IActionResult> GetChartAccountById(int id)
        {
            ChartAccount chartAccount = await _authService.GetChartAccountByIdAsync(id);
            if (chartAccount == null)
            {
                return StatusCode(500, "Sub Account request not found");
            }
            return Ok(chartAccount);
        }

        public async Task<IActionResult> ChartAccountDll()
        {
            IEnumerable<ChartAccountDto> chartAccounts = new List<ChartAccountDto>();
            chartAccounts = await _authService.GetChartAccountsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                chartAccounts = chartAccounts.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(chartAccounts);
        }

    }
}
