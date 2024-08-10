using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;

        public BudgetController(IAuthService authService, IPermissionService permissionService)
        {
            _authService = authService;
            _permissionService = permissionService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewBudget);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetBudgets()
        {
            IEnumerable<BudgetDto> budgets = new List<BudgetDto>();
            budgets = await _authService.GetBudgetsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                budgets = budgets.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(budgets);
        }

        public async Task<IActionResult> Create()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddBudget);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveBudget(BudgetDto budget, string BudgetItemsJson)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddBudget);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (budget == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            budget.ItemsJson = BudgetItemsJson;
            budget.AddedBy = Request?.Cookies["userId"]?.ToString();
           await _authService.SaveBudgetAsync(budget);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DuplicateBudget(int id)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddBudget);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            var budget = await _authService.GetBudgetByIdAsync(id);
            return View(budget);
        }

        public async Task<IActionResult> SaveDuplicateBudget(BudgetDto budget, string BudgetItemsJson)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddBudget);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            if (budget == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }
            budget.ItemsJson = BudgetItemsJson;
            budget.AddedBy = Request?.Cookies["userId"]?.ToString();
            await _authService.SaveDuplicateBudgetAsync(budget);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteBudget(int id)
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.AddBudget);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            await _authService.DeleteBudgetAsync(id);
            return  RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //public async Task<IActionResult> SaveBudget([FromBody] Budget budget)
        //{
        //    if (budget == null)
        //    {
        //        return Json(new { success = false, message = "Received data is null." });
        //    }
        //    budget.AddedBy = Request?.Cookies["userId"]?.ToString();
        //    await _authService.SaveBudgetAsync(budget);
        //    return Json(new { success = true, message = "Sub Account added successfully" });
        //}




    }
}
