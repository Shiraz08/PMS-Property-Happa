using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.CodeDom;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class ChartAccountsController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;

        public ChartAccountsController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewChartofAccounts);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetChartAccounts()
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

        [HttpPost]
        public async Task<IActionResult> SaveChartAccount([FromBody] ChartAccount chartAccount)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddChartofAccounts);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (chartAccount == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            chartAccount.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveChartAccountAsync(chartAccount);
            return Json(new { success = true, message = "Chart Account added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChartAccount(int id)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddChartofAccounts);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var response = await _authService.DeleteChartAccountAsync(id);
            if (!response.IsSuccess)
            {
                return Ok(new { success = false, message = string.Join(", ", response.ErrorMessages) });

            }
            return Ok(new { success = true, message = "Chart Account deleted successfully" });
        }

        public async Task<IActionResult> GetChartAccountById(int id)
        {
            ChartAccount chartAccount = await _authService.GetChartAccountByIdAsync(id);
            if (chartAccount == null)
            {
                return StatusCode(500, "Chart Account request not found");
            }
            return Ok(chartAccount);
        }

        public async Task<IActionResult> ChartAccountDll()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            var filter = new Filter();
            filter.AddedBy = currenUserId;
            IEnumerable<ChartAccountDto> chartAccounts = new List<ChartAccountDto>();
            chartAccounts = await _authService.GetChartAccountsDllAsync(filter);
            //if (currenUserId != null)
            //{
            //    chartAccounts = chartAccounts.Where(s => s.AddedBy == currenUserId);
            //}
            return Ok(chartAccounts);
        }

    }
}
