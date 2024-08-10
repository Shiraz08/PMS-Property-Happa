using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers  
{
    public class AccountingController : Controller
    {
        private readonly IPermissionService _permissionService;

        public AccountingController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Index()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewAccounting);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> Rent()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewRent);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> AssestExpense()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewAssestExpense);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> Deposit()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewDeposit);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> Billing()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewBilling);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View(); 
        }
        public async Task<IActionResult> Invoices()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(currenUserId, (int)UserPermissions.ViewInvoices);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
    }
}
